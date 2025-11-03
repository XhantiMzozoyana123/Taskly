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
        private readonly ICookieService _cookieService;

        public TwitterService(ApplicationDbContext context, IAiService aiService, ICookieService cookieService)
        {
            _context = context;
            _aiService = aiService;
            _cookieService = cookieService;
        }

        public async Task SearchAsync(SearchDto searchDto)
        {
            if (string.IsNullOrWhiteSpace(searchDto.Keyword))
                return;

            IPage page = null;
            IBrowser browser = null;

            try
            {
                // Login to Twitter
                (page, browser) = await _cookieService.LoadCookieOnPageAsync(searchDto.CookiePath, searchDto.PrivateMode);
                
                // Navigate to the specified URL (e.g., search results or user timeline)
                page = await GoToTweetsPageAsync(page, searchDto);

                // Wait for the first tweet element to be visible using the actual main div class
                var mainTweetContainerSelector = ".css-175oi2r";
                var tweetContainerLocator = page.Locator(mainTweetContainerSelector).First;
                await tweetContainerLocator.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible, Timeout = 15000 });

                var scrapedTweetPostUrls = new HashSet<string>(); // Track already scraped tweet post URLs to avoid duplicates

                // Loop for scrolling and loading multiple pages of tweets
                for (int i = 0; i < searchDto.PageNumber; i++)
                {
                    // Scrolling strategy: Scroll down to load more content
                    await page.EvaluateAsync(@"window.scrollBy(1, window.innerHeight);");

                    // Get all tweet containers currently loaded on the page
                    var tweetLocators = await page.Locator(mainTweetContainerSelector).AllAsync();

                    foreach (var currentTweetLocator in tweetLocators)
                    {
                        // --- 1. Extract Post URL (Permalink) first for unique tracking ---
                        string? postUrl = null;
                        // Selector: The 'a' tag within the tweet's time block, which has href containing '/status/' and aria-label ending with 'ago'
                        var postLinkLocator = currentTweetLocator.Locator("a[href*='/status/']").First;
                        if (await postLinkLocator.IsVisibleAsync())
                        {
                            postUrl = await postLinkLocator.GetAttributeAsync("href");
                            postUrl = !string.IsNullOrWhiteSpace(postUrl) ? $"https://x.com{postUrl}" : null;
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
                            profileUrl = !string.IsNullOrWhiteSpace(relativeProfileUrl) ? $"https://x.com{relativeProfileUrl}" : null;
                        }

                        // --- 3. Extract Tweet Text ---
                        // The tweet text is directly within the div[data-testid='tweetText']
                        var tweetTextLocator = currentTweetLocator.Locator("div[data-testid='tweetText']").First;
                        if (await tweetTextLocator.IsVisibleAsync())
                        {
                            tweetText = await tweetTextLocator.InnerTextAsync();
                            tweetText = tweetText?.Trim();
                        }

                        // --- 4. Extract Published Datetime ---
                        //// The datetime attribute of the <time> tag, which is within the post permalink anchor.
                        var timeLocator = currentTweetLocator.Locator("a[href*='/status/']").First;
                        timeLocator = timeLocator.Locator("time").First;

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
                                try
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
                                catch(Exception ex)
                                {
                                    throw ex;
                                }
                            }
                        }

                        // Scrolling strategy: Scroll down to load more content
                        await page.EvaluateAsync(@"window.scrollBy(2, window.innerHeight);");
                        await Task.Delay(2000); // Wait for new tweets to load after scrolling.
                    }
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

        public async Task<IPage> GoToTweetsPageAsync(IPage page, SearchDto searchDto)
        {
            // Go to X Explore page
            await page.GotoAsync($"https://x.com/search?q={searchDto.Keyword.Replace(" ", "+")}", new PageGotoOptions
            {
                WaitUntil = WaitUntilState.DOMContentLoaded
            });

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