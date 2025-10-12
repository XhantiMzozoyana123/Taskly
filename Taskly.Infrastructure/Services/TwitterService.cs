using Microsoft.EntityFrameworkCore;
using Microsoft.Playwright;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.Xml;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Taskly.Application.Constants;
using Taskly.Application.Dtos;
using Taskly.Application.Interfaces;
using Taskly.Domain;
using Taskly.Domain.Entities;

namespace Taskly.Infrastructure.Services
{
    public class TwitterService : ITwitterService
    {
        private readonly ApplicationDbContext _context;
        private readonly IAiService _aiService;

        public TwitterService(ApplicationDbContext context, IAiService aiService)
        {
            _context = context;
            _aiService = aiService;
        }

        public async Task<IPage> GoToExplorePageAsync(IPage page, SearchDto searchDto)
        {
            // Go to X Explore page
            await page.GotoAsync("https://x.com/explore", new PageGotoOptions
            {
                WaitUntil = WaitUntilState.DOMContentLoaded,
                Timeout = 30000
            });

            // Wait for the search box to appear
            var searchInput = page.Locator("input[data-testid='SearchBox_Search_Input']");
            await searchInput.WaitForAsync();

            // Fill in the search query
            await searchInput.FillAsync(searchDto.Keyword);

            // Press Enter to trigger the search
            await searchInput.PressAsync("Enter");

            // Wait for the results page to load
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            return page;
        }

        public async Task SearchAsync(SearchDto searchDto)
        {
            if (string.IsNullOrWhiteSpace(searchDto.Url))
            {
                // Internal logging for invalid URL or missing Twitter login credentials.
                return;
            }

            using var playwright = await Playwright.CreateAsync();
            await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Headless = searchDto.PrivateMode });
            var page = await browser.NewPageAsync();

            try
            {
                // Login to Twitter (X)
                page = await LoginAsync(page, searchDto);

                // Navigate to the specified URL (e.g., search results or user timeline)
                page = await GoToExplorePageAsync(page, searchDto);

                // Wait for the first tweet element to be visible using the actual main div class
                var mainTweetContainerSelector = "div.css-175oi2r.r-1iusvr4.r-16y2uox.r-1777fci.r-kzbkwu";
                var tweetContainerLocator = page.Locator(mainTweetContainerSelector).First;
                await tweetContainerLocator.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible, Timeout = 15000 });

                var scrapedTweetPostUrls = new HashSet<string>(); // Track already scraped tweet post URLs to avoid duplicates

                // Loop for scrolling and loading multiple pages of tweets
                for (int i = 0; i < searchDto.PageNumber; i++)
                {
                    // Get all tweet containers currently loaded on the page
                    var tweetLocators = await page.Locator(mainTweetContainerSelector).AllAsync();

                    foreach (var currentTweetLocator in tweetLocators)
                    {
                        // --- 1. Extract Post URL (Permalink) first for unique tracking ---
                        string? postUrl = null;
                        // Selector: The 'a' tag within the tweet's time block, which has href containing '/status/' and aria-label ending with 'ago'
                        var postLinkLocator = currentTweetLocator.Locator("a[href*='/status/'][role='link'][aria-label$='ago']").First;
                        if (await postLinkLocator.IsVisibleAsync())
                        {
                            string? relativePostUrl = await postLinkLocator.GetAttributeAsync("href");
                            if (!string.IsNullOrWhiteSpace(relativePostUrl))
                            {
                                // Construct absolute URL
                                postUrl = new Uri(new Uri(searchDto.Url), relativePostUrl).AbsoluteUri;
                            }
                        }

                        // Use the absolute post URL as the unique identifier
                        if (string.IsNullOrWhiteSpace(postUrl) || scrapedTweetPostUrls.Contains(postUrl))
                            continue; // Skip if URL is invalid or already scraped

                        scrapedTweetPostUrls.Add(postUrl); // Add to set

                        // Initialize other data points for the current tweet
                        string? username = null;
                        string? profileUrl = null;
                        string? tweetText = null;
                        DateTime? publishedDate = null;

                        // --- 2. Extract Username (Full Display Name) and Profile URL ---
                        // The user's display name is within the first <span> with specific classes inside the first <div> with dir='ltr'
                        // under the main user link within data-testid="User-Name".
                        var userDisplayNameLocator = currentTweetLocator.Locator("div[data-testid='User-Name'] a[role='link'] div[dir='ltr'] span.r-poiln3 span.r-poiln3").First;
                        if (await userDisplayNameLocator.IsVisibleAsync())
                        {
                            username = await userDisplayNameLocator.InnerTextAsync(); // Get the full display name (e.g., Ayilola of Nigeria /Ogbonge doc)
                        }

                        // The profile URL is the href of the main 'a' tag within the 'User-Name' testid block.
                        var userProfileLinkLocator = currentTweetLocator.Locator("div[data-testid='User-Name'] a[role='link']").First;
                        if (await userProfileLinkLocator.IsVisibleAsync())
                        {
                            string? relativeProfileUrl = await userProfileLinkLocator.GetAttributeAsync("href");
                            if (!string.IsNullOrWhiteSpace(relativeProfileUrl))
                            {
                                profileUrl = new Uri(new Uri(searchDto.Url), relativeProfileUrl).AbsoluteUri;
                            }
                        }

                        // --- 3. Extract Tweet Text ---
                        // The tweet text is directly within the div[data-testid='tweetText']
                        var tweetTextLocator = currentTweetLocator.Locator("div[data-testid='tweetText']").First;
                        if (await tweetTextLocator.IsVisibleAsync())
                        {
                            tweetText = await tweetTextLocator.InnerTextAsync();
                            tweetText = tweetText?.Trim(); // Clean up text
                        }

                        // --- 4. Extract Published Datetime ---
                        // The datetime attribute of the <time> tag, which is within the post permalink anchor.
                        var timeLocator = currentTweetLocator.Locator("a[href*='/status/'][role='link'][aria-label$='ago'] time[datetime]").First;
                        if (await timeLocator.IsVisibleAsync())
                        {
                            var dateTimeAttribute = await timeLocator.GetAttributeAsync("datetime");
                            if (!string.IsNullOrWhiteSpace(dateTimeAttribute) && DateTime.TryParse(dateTimeAttribute, out DateTime parsedDateTime))
                            {
                                publishedDate = parsedDateTime;
                            }
                        }

                        // Only process and save if core data exists
                        if (!string.IsNullOrWhiteSpace(username) || !string.IsNullOrWhiteSpace(tweetText))
                        {
                            // Use AI service to check if the content is relevant
                            var isRelevant = await _aiService.CheckIfContentIsRelevantAsync(tweetText, searchDto.Query);

                            if (isRelevant)
                            {
                                var lead = new Leads()
                                {
                                    Name = username,
                                    ProfileUrl = profileUrl,
                                    Status = "New",
                                    Platform = "Twitter",
                                    PostDescription = tweetText,
                                    PostUrl = postUrl,
                                    Keywords = searchDto.Keyword,
                                    Query = searchDto.Query,
                                    PostDate = publishedDate.Value
                                };

                                await _context.Leads.AddAsync(lead);
                                await _context.SaveChangesAsync();
                            }
                        }
                    }

                    // Scrolling strategy: Scroll down to load more content
                    await page.EvaluateAsync(@"window.scrollBy(0, window.innerHeight);");
                    await Task.Delay(2000); // Wait for new tweets to load after scrolling.
                }
            }
            catch (Exception ex)
            {
                // Internal logging for general exceptions.
                throw ex;
            }
            finally
            {
                await browser.CloseAsync();
            }
        }

        public async Task<IPage> LoginAsync(IPage page, SearchDto searchDto)
        {
            var socialLogin = await _context.SocialLogins.FirstOrDefaultAsync(x =>
                   x.UserId == searchDto.UserId &&
                   x.Platform == "Twitter");

            if (socialLogin == null)
            {
                // Internal logging: "Twitter login credentials not found for UserId: {searchDto.UserId}"
                return page;
            }

            var userName = TokenEncryptor.Decrypt(socialLogin.UsernameHash);
            var passWord = TokenEncryptor.Decrypt(socialLogin.PasswordHash);

            // --- START LOGIN SEQUENCE ---
            await page.GotoAsync("https://x.com/i/flow/login", new PageGotoOptions { WaitUntil = WaitUntilState.DOMContentLoaded, Timeout = 30000 });

            // Step 1: Enter username/email/phone
            var usernameInput = page.Locator("input[name='text']");
            await usernameInput.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible, Timeout = 10000 });
            await usernameInput.FillAsync(userName);
            // Click the 'Next' button (using a more generic selector as text can be localized, but 'Next' is common)
            await page.Locator("div[role='button']:has-text('Next')").ClickAsync();


            // Step 2: Enter password
            var passwordInput = page.Locator("input[name='password']");
            await passwordInput.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible, Timeout = 10000 });
            await passwordInput.FillAsync(passWord);
            await page.Locator("div[data-testid='LoginForm_Login_Button']").ClickAsync(); // Click the 'Log In' button

            // Wait for navigation after login to the authenticated homepage/feed
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle, new PageWaitForLoadStateOptions { Timeout = 30000 }); // Increased timeout

            // Now that we are (hopefully) logged in, navigate to the target Twitter URL
            await page.GotoAsync(searchDto.Url, new PageGotoOptions { WaitUntil = WaitUntilState.DOMContentLoaded, Timeout = 30000 });

            // --- END LOGIN SEQUENCE ---
            return page;
        }

        public async Task<IPage> DirectMessagingAsync(IPage page, MessengerDto messengerDto)
        {
            // Go to the user's X profile page
            await page.GotoAsync(messengerDto.Lead.ProfileUrl, new PageGotoOptions
            {
                WaitUntil = WaitUntilState.DOMContentLoaded,
                Timeout = 30000
            });

            // Wait for the Message button to appear
            var messageButton = page.Locator("button[data-testid='sendDMFromProfile']");
            await messageButton.WaitForAsync(new LocatorWaitForOptions
            {
                State = WaitForSelectorState.Visible,
                Timeout = 10000
            });

            // Click the Message button
            await messageButton.ClickAsync();

            // Wait for the DM input to appear (Draft.js editor)
            var dmInput = page.Locator("div.public-DraftStyleDefault-block");
            await dmInput.WaitForAsync(new LocatorWaitForOptions
            {
                State = WaitForSelectorState.Visible,
                Timeout = 10000
            });

            // Focus the Draft.js input and type the message
            await dmInput.ClickAsync();
            await page.Keyboard.TypeAsync(messengerDto.Text);

            // Send the message (Enter sends DM)
            await page.Keyboard.PressAsync("Enter");

            // Wait for network idle to ensure send completes
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            return page;
        }
    }
}