using Microsoft.EntityFrameworkCore;
using Microsoft.Playwright;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Taskly.Application.Constants; // Assuming TokenEncryptor is here
using Taskly.Application.Dtos;
using Taskly.Application.Interfaces;
using Taskly.Domain; // Assuming ApplicationDbContext is here
using Taskly.Domain.Entities; // Assuming InstagramPost, SocialLogins are here

namespace Taskly.Infrastructure.Services
{
    public class InstagramService : IInstagramService
    {
        private readonly ApplicationDbContext _context;
        private readonly IAiService _aiService;

        public InstagramService(ApplicationDbContext context, IAiService aiService)
        {
            _context = context;
            _aiService = aiService;
        }

        public async Task SearchAsync(SearchDto searchDto)
        {
            if (string.IsNullOrWhiteSpace(searchDto.Url))
            {
                // Internal logging for invalid URL or missing Instagram login credentials.
                return;
            }

            using var playwright = await Playwright.CreateAsync();
            await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Headless = searchDto.PrivateMode });
            var page = await browser.NewPageAsync();

            try
            {
                // Login to Instagram
                page = await LoginAsync(page, searchDto);

                // Go to Expore Page to search for hashtags or keywords
                page = await GoToExplorePageAsync(page, searchDto);

                // Wait for at least one Instagram post link to be visible
                // The class `_a6hd` seems consistent in your HTML snippets for post links
                var postLinkLocator = page.Locator("a._a6hd").First;
                await postLinkLocator.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible, Timeout = 15000 });

                var scrapedPostUrls = new HashSet<string>(); // Track already scraped post URLs

                // Loop for scrolling and loading multiple pages of Instagram posts
                for (int i = 0; i < searchDto.PageNumber; i++) // searchDto.PageNumber now controls scroll iterations
                {
                    // Get all anchor tags that represent Instagram posts currently loaded
                    var instagramPostLocators = await page.Locator("a._a6hd").AllAsync();

                    foreach (var currentPostLocator in instagramPostLocators)
                    {
                        string? postUrl = await currentPostLocator.GetAttributeAsync("href");

                        // Make post URL absolute immediately for reliable tracking
                        string? absolutePostUrl = null;
                        if (!string.IsNullOrWhiteSpace(postUrl))
                        {
                            absolutePostUrl = new Uri(new Uri(searchDto.Url), postUrl).AbsoluteUri;
                        }

                        // Check if this post has already been scraped
                        if (!string.IsNullOrWhiteSpace(absolutePostUrl) && scrapedPostUrls.Contains(absolutePostUrl))
                            continue; // Already scraped, skip

                        if (!string.IsNullOrWhiteSpace(absolutePostUrl))
                        {
                            scrapedPostUrls.Add(absolutePostUrl); // Add to set
                        }

                        string? imageUrl = null;
                        string? altText = null;

                        // Locate the img tag within the current post link
                        var imgLocator = currentPostLocator.Locator("img.x5yr21d").First;

                        if (await imgLocator.IsVisibleAsync())
                        {
                            imageUrl = await imgLocator.GetAttributeAsync("src");
                            altText = await imgLocator.GetAttributeAsync("alt");
                        }

                        // Only process and save if core data exists
                        if (!string.IsNullOrWhiteSpace(absolutePostUrl) && !string.IsNullOrWhiteSpace(imageUrl))
                        {
                            var postDto = await GetAuthorPost(absolutePostUrl);

                            // Use AI service to check if the content (altText) is relevant
                            var isRelevant = await _aiService.CheckIfContentIsRelevantAsync(altText, searchDto.Query);
                            if (isRelevant)
                            {
                                var lead = new Leads()
                                {
                                    Name = postDto.Author,
                                    ProfileUrl = postDto.ProfileUrl,
                                    Status = "New",
                                    Platform = "Instagram",
                                    PostDescription = postDto.Text,
                                    PostUrl = postUrl,
                                    Keywords = searchDto.Keyword,
                                    Query = searchDto.Query,
                                    PostDate = postDto.PublishedDate
                                };

                                await _context.Leads.AddAsync(lead);
                                await _context.SaveChangesAsync();
                            }
                        }
                    }

                    // Scrolling strategy: Scroll down to load more content
                    // Instagram uses infinite scrolling. We scroll a fixed amount (or to bottom)
                    // and wait for new content to render.
                    await page.EvaluateAsync(@"window.scrollBy(0, window.innerHeight);"); // Scroll by one viewport height
                    await Task.Delay(2000); // Wait for new posts to load after scrolling. Adjust as needed.

                    // Optional: You could add a check here to see if scrolling actually loaded new content
                    // e.g., compare scrapedPostUrls.Count before and after scroll. If no new posts, break.
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
                   x.Platform == "Instagram");

            if (socialLogin == null)
            {
                // Internal logging: "Instagram login credentials not found for UserId: {searchDto.UserId}"
                return page; // Return current page state, login will likely fail
            }

            var userName = TokenEncryptor.Decrypt(socialLogin.UsernameHash);
            var passWord = TokenEncryptor.Decrypt(socialLogin.PasswordHash);

            // --- START LOGIN SEQUENCE ---
            await page.GotoAsync("https://www.instagram.com/accounts/login/", new PageGotoOptions { WaitUntil = WaitUntilState.DOMContentLoaded });

            await page.Locator("input[name='username']").FillAsync(userName);
            await page.Locator("input[name='password']").FillAsync(passWord);

            // Click login button. Instagram's login button can sometimes be a generic submit.
            // Using a more specific selector from your HTML: "button[type='submit']"
            await page.Locator("button[type='submit']").ClickAsync();

            await page.WaitForLoadStateAsync(LoadState.NetworkIdle, new PageWaitForLoadStateOptions { Timeout = 20000 }); // Increased timeout for Instagram login

            // Handle "Not Now" on "Save Your Login Info?" popup, if it appears
            var notNowButton = page.Locator("button >> text='Not Now'").First;
            if (await notNowButton.IsVisibleAsync(new LocatorIsVisibleOptions { Timeout = 5000 })) // Check if visible within 5s
            {
                await notNowButton.ClickAsync();
                await page.WaitForLoadStateAsync(LoadState.NetworkIdle, new PageWaitForLoadStateOptions { Timeout = 5000 });
            }

            // Handle "Turn on Notifications" "Not Now" button, if it appears after login
            var notificationNotNowButton = page.Locator("button >> text='Not Now'").Nth(1); // Try the second 'Not Now' if first was for save login
            if (await notificationNotNowButton.IsVisibleAsync(new LocatorIsVisibleOptions { Timeout = 5000 }))
            {
                await notificationNotNowButton.ClickAsync();
                await page.WaitForLoadStateAsync(LoadState.NetworkIdle, new PageWaitForLoadStateOptions { Timeout = 5000 });
            }
            // --- END LOGIN SEQUENCE ---

            // Now that we are (hopefully) logged in, navigate to the target Instagram URL
            await page.GotoAsync(searchDto.Url, new PageGotoOptions { WaitUntil = WaitUntilState.DOMContentLoaded });

            return page;
        }

        public async Task<IPage> GoToExplorePageAsync(IPage page, SearchDto searchDto)
        {
            // Wait for the element to appear (recommended)
            await page.WaitForSelectorAsync("div[aria-selected='true'] svg[aria-label='Explore']");

            // Click on the parent div (the outermost clickable element)
            await page.ClickAsync("div[aria-selected='true']:has(svg[aria-label='Explore'])");

            // Type your keyword
            await page.FillAsync("input[aria-label='Search input']", searchDto.Keyword);

            // Optional: press Enter
            await page.Keyboard.PressAsync("Enter");

            // Wait for results (optional)
            await page.WaitForTimeoutAsync(2000);

            // Extract the Instagram username from the URL for the entity
            var uri = new Uri(searchDto.Url);
            var instagramUsername = uri.Segments.LastOrDefault(s => !string.IsNullOrWhiteSpace(s) && !s.EndsWith("/"))?.TrimEnd('/');

            return page;
        }

        public async Task<PostContentDto> GetAuthorPost(string postUrl)
        {
            PostContentDto postContentDto = new PostContentDto();

            if (string.IsNullOrWhiteSpace(postUrl))
                return null;

            using var playwright = await Playwright.CreateAsync();
            await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
            {
                Headless = true
            });
            
            var page = await browser.NewPageAsync();

            try
            {

                await page.GotoAsync(postUrl, new PageGotoOptions
                {
                    WaitUntil = WaitUntilState.DOMContentLoaded
                });

                // Wait for the username/caption/time to appear
                await page.Locator("span._ap3a._aaco._aacw._aacx._aad7._aade, span.x193iq5w.xeuugli, time.xdwrcjd")
                          .First
                          .WaitForAsync(new LocatorWaitForOptions
                          {
                              State = WaitForSelectorState.Visible,
                              Timeout = 10000
                          });

                // Scrape username
                string? username = null;
                var usernameLocator = page.Locator("span._ap3a._aaco._aacw._aacx._aad7._aade");
                if (await usernameLocator.IsVisibleAsync())
                {
                    username = await usernameLocator.InnerTextAsync();
                }

                // Scrape caption
                string? caption = null;
                var captionLocator = page.Locator("span.x193iq5w.xeuugli");
                if (await captionLocator.IsVisibleAsync())
                {
                    caption = await captionLocator.InnerHTMLAsync();
                    caption = caption.Replace("<br>", "\n")
                                     .Replace("<br/>", "\n")
                                     .Replace("<br />", "\n");
                    caption = System.Text.RegularExpressions.Regex.Replace(caption, "<.*?>", string.Empty).Trim();
                }

                // Scrape post datetime
                DateTime? postDate = null;
                var timeLocator = page.Locator("time.xdwrcjd");
                if (await timeLocator.IsVisibleAsync())
                {
                    var datetimeAttr = await timeLocator.GetAttributeAsync("datetime");
                    if (!string.IsNullOrWhiteSpace(datetimeAttr) && DateTime.TryParse(datetimeAttr, out DateTime parsedDate))
                    {
                        postDate = parsedDate;
                    }
                }

                postContentDto.Author = username ?? string.Empty;
                postContentDto.Text = caption ?? string.Empty;
                postContentDto.PostUrl = postUrl;
                postContentDto.ProfileUrl = $"https://www.instagram.com/{username}/";
                postContentDto.PublishedDate = postDate.Value;

                await browser.CloseAsync();
            }
            catch (Exception ex)
            {
                // Internal logging for exceptions during post scraping
                await browser.CloseAsync();
                return null;
            }

            return postContentDto;
        }

        public async Task<IPage> DirectMessagingAsync(IPage page, MessengerDto messengerDto)
        {
            // Navigate to the user's Instagram profile
            await page.GotoAsync(messengerDto.Lead.ProfileUrl, new PageGotoOptions
            {
                WaitUntil = WaitUntilState.DOMContentLoaded,
                Timeout = 30000
            });

            // Wait for and click the "Message" button
            var messageButton = page.Locator("div[role='button']:has-text('Message')");
            if (await messageButton.CountAsync() == 0)
            {
                return page; // Skip users with closed DMs
            }

            await messageButton.First.ClickAsync();

            // Wait for the Lexical Editor input to appear
            var messageInput = page.Locator("div[contenteditable='true'][role='textbox']");
            await messageInput.WaitForAsync(new LocatorWaitForOptions
            {
                State = WaitForSelectorState.Visible,
                Timeout = 10000
            });

            // Focus and type the message
            await messageInput.ClickAsync();
            await page.Keyboard.TypeAsync(messengerDto.Text);

            // Press Enter to send the DM
            await page.Keyboard.PressAsync("Enter");

            // Wait for idle network to ensure message is sent
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            return page;
        }
    }
}