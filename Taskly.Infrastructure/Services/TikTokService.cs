using Microsoft.EntityFrameworkCore;
using Microsoft.Playwright;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Taskly.Application.Constants;
using Taskly.Application.Dtos;
using Taskly.Application.Interfaces;
using Taskly.Domain;
using Taskly.Domain.Entities;

namespace Taskly.Infrastructure.Services
{
    public class TikTokService : ITikTokService
    {
        private readonly ApplicationDbContext _context;
        private readonly IAiService _aiService;
        
        public TikTokService(ApplicationDbContext context, IAiService aiService)
        {
            _context = context;
            _aiService = aiService;
        }

        public async Task<IPage> DirectMessagingAsync(IPage page, MessengerDto messengerDto)
        {
            // Navigate to the target user's profile
            await page.GotoAsync(messengerDto.Lead.ProfileUrl, new PageGotoOptions
            {
                WaitUntil = WaitUntilState.DOMContentLoaded,
                Timeout = 30000
            });

            // --- STEP 1: Click the "Message" button ---
            var messageButton = page.Locator("[data-e2e='message-button']");
            if (await messageButton.CountAsync() == 0)
            {
                return page; // Skip if user’s DMs are closed
            }

            await messageButton.First.WaitForAsync(new LocatorWaitForOptions
            {
                State = WaitForSelectorState.Attached,
                Timeout = 10000
            });
            await messageButton.First.ClickAsync();

            // --- STEP 2: Wait for the message input box ---
            var messageInput = page.Locator("div[contenteditable='true'].public-DraftEditor-content");
            await messageInput.WaitForAsync(new LocatorWaitForOptions
            {
                State = WaitForSelectorState.Visible,
                Timeout = 10000
            });

            // --- STEP 3: Type and send message ---
            await messageInput.ClickAsync();
            await messageInput.FillAsync(messengerDto.Text); // FillAsync works on editable divs

            // Some platforms require Enter key to send
            await page.Keyboard.PressAsync("Enter");

            // --- STEP 4: Wait to ensure the message is sent ---
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle, new PageWaitForLoadStateOptions
            {
                Timeout = 10000
            });

            return page;
        }

        public async Task<string> GetVideoDescription(IPage page)
        {
            // Locate the element by its data-e2e attribute and get its text content
            var videoDescriptionElement = page.Locator("[data-e2e='video-desc']");
            string videoDescription = await videoDescriptionElement.TextContentAsync();

            return videoDescription?.Trim() ?? string.Empty;
        }

        public async Task<IPage> GoToExplorePageAsync(IPage page, SearchDto searchDto)
        {
            // Go to X Explore page
            await page.GotoAsync("https://www.tiktok.com/en/", new PageGotoOptions
            {
                WaitUntil = WaitUntilState.DOMContentLoaded,
                Timeout = 30000
            });

            // Option 1: Using data-e2e attribute
            await page.ClickAsync("[data-e2e='nav-search']");

            var searchInput = await page.WaitForSelectorAsync("[data-e2e='search-user-input']");

            // Type your search query
            await searchInput.FillAsync(searchDto.Keyword);

            // Click into it (optional, but sometimes needed)
            await searchInput.ClickAsync();

            // Wait for the results page to load
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            return page;
        }

        public async Task<IPage> LoginAsync(IPage page, SearchDto searchDto)
        {
            var socialLogin = await _context.SocialLogins.FirstOrDefaultAsync(x =>
                               x.UserId == searchDto.UserId &&
                               x.Platform == "TikTok");

            if (socialLogin == null)
            {
                // Internal logging: "Twitter login credentials not found for UserId: {searchDto.UserId}"
                return page;
            }

            var userName = TokenEncryptor.Decrypt(socialLogin.UsernameHash);
            var passWord = TokenEncryptor.Decrypt(socialLogin.PasswordHash);

            // --- START LOGIN SEQUENCE ---
            await page.GotoAsync("https://www.tiktok.com/login/phone-or-email/email", new PageGotoOptions { WaitUntil = WaitUntilState.DOMContentLoaded, Timeout = 30000 });

            // Step 1: Enter username/email
            var usernameInput = page.Locator("input[name='username']");
            await usernameInput.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible, Timeout = 10000 });
            await usernameInput.FillAsync(userName);

            // Step 2: Enter password
            var passwordInput = page.Locator("input[name='password']");
            await passwordInput.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible, Timeout = 10000 });
            await passwordInput.FillAsync(passWord);

            // Step 3: Click the login button using data-e2e
            var loginButton = page.Locator("[data-e2e='login-button']");
            await loginButton.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible, Timeout = 10000 });
            await loginButton.ClickAsync();

            // Wait for navigation after login
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle, new PageWaitForLoadStateOptions { Timeout = 30000 });

            // --- END LOGIN SEQUENCE ---
            return page;
        }

        public async Task SearchAsync(SearchDto searchDto)
        {
            if (string.IsNullOrWhiteSpace(searchDto.Url))
                return;

            using var playwright = await Playwright.CreateAsync();
            await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Headless = searchDto.PrivateMode });
            var page = await browser.NewPageAsync();

            try 
            {
                // Login to Tik-Tok if credentials are provided
                page = await LoginAsync(page, searchDto);

                // Navigate to the specified URL (e.g., search results or user timeline)
                page = await GoToExplorePageAsync(page, searchDto);

                // Store the base URL for constructing absolute URLs later
                string tiktokUrl = page.Url;

                // Selector for a single video article container
                var videoArticleContainerSelector = "article[data-e2e='recommend-list-item-container']";

                // Use a HashSet to keep track of processed videos by their data-scroll-index.
                // This prevents reprocessing the same video if scrolling brings it back into view.
                var processedVideoIdentifiers = new HashSet<string>();
                int maxScrollAttempts = 5; // Define how many times you want to scroll down the page. Adjust as needed.

                Console.WriteLine($"Starting to scrape. Will attempt up to {maxScrollAttempts} scroll cycles.");

                for (int i = 0; i < maxScrollAttempts; i++) 
                {
                    // Scroll down to load more content
                    // This scrolls by the height of the current viewport.
                    await page.EvaluateAsync("window.scrollBy(0, window.innerHeight)");
                    await Task.Delay(2000); // Give the page time for new content to load and render (adjust delay if needed)

                    // Get all video article locators currently in the DOM
                    var currentVideoLocators = await page.Locator(videoArticleContainerSelector).AllAsync();

                    bool newVideosFoundInThisScroll = false;

                    foreach (var videoLocator in currentVideoLocators)
                    {
                        // Attempt to get the data-scroll-index for uniqueness tracking
                        // If you can find a more stable unique ID (like a video URL), use that instead.
                        string? dataScrollIndex = await videoLocator.GetAttributeAsync("data-scroll-index");

                        // If we can't get a unique identifier or have already processed this one, skip
                        if (string.IsNullOrWhiteSpace(dataScrollIndex) || processedVideoIdentifiers.Contains(dataScrollIndex))
                        {
                            continue;
                        }

                        processedVideoIdentifiers.Add(dataScrollIndex); // Mark as processed
                        newVideosFoundInThisScroll = true;

                        // --- Extract Data for this NEWLY DISCOVERED video ---

                        string? username = null;
                        string? profileUrl = null;
                        string? videoDescription = null;
                        string? likesCount = null;
                        string? commentsCount = null;
                        string? sharesCount = null;

                        // Username and Profile URL
                        var usernameLocator = videoLocator.Locator("div[data-e2e='video-author-uniqueid']").First;
                        var profileLinkLocator = videoLocator.Locator("a.css-nw70yw-5e6d46e3--StyledAuthorAnchor").First;

                        if (await usernameLocator.IsVisibleAsync())
                        {
                            username = await usernameLocator.InnerTextAsync();
                        }
                        if (await profileLinkLocator.IsVisibleAsync())
                        {
                            string? relativeUrl = await profileLinkLocator.GetAttributeAsync("href");
                            if (!string.IsNullOrWhiteSpace(relativeUrl))
                            {
                                profileUrl = new Uri(new Uri(tiktokUrl), relativeUrl).AbsoluteUri;
                            }
                        }

                        // Video Description
                        var descriptionLocator = videoLocator.Locator("div[data-e2e='video-desc']").First;
                        if (await descriptionLocator.IsVisibleAsync())
                        {
                            videoDescription = await descriptionLocator.InnerTextAsync();
                            videoDescription = videoDescription?.Trim();
                        }

                        // Likes Count
                        var likesLocator = videoLocator.Locator("strong[data-e2e='like-count']").First;
                        if (await likesLocator.IsVisibleAsync())
                        {
                            likesCount = await likesLocator.InnerTextAsync();
                        }

                        // Comments Count
                        var commentsLocator = videoLocator.Locator("strong[data-e2e='comment-count']").First;
                        if (await commentsLocator.IsVisibleAsync())
                        {
                            commentsCount = await commentsLocator.InnerTextAsync();
                        }

                        // Shares Count
                        var sharesLocator = videoLocator.Locator("strong[data-e2e='share-count']").First;
                        if (await sharesLocator.IsVisibleAsync())
                        {
                            sharesCount = await sharesLocator.InnerTextAsync();
                        }

                        // Only process and save if core data exists
                        if (!string.IsNullOrWhiteSpace(username) || !string.IsNullOrWhiteSpace(videoDescription))
                        {
                            // Use AI service to check if the content is relevant
                            var isRelevant = await _aiService.CheckIfContentIsRelevantAsync(videoDescription, searchDto.Query);

                            if (isRelevant)
                            {
                                var lead = new Leads()
                                {
                                    Name = username,
                                    ProfileUrl = profileUrl,
                                    Status = "New",
                                    Platform = "TikTok",
                                    PostDescription = videoDescription,
                                    PostUrl = tiktokUrl,
                                    Keywords = searchDto.Keyword,
                                    Query = searchDto.Query,
                                    PostDate = DateTime.Now
                                };

                                await _context.Leads.AddAsync(lead);
                                await _context.SaveChangesAsync();
                            }
                        }
                    }

                    // If no new unique videos were found in this scroll cycle (after the first scroll),
                    // it's likely we've reached the end of available content or hit a loading issue.
                    if (!newVideosFoundInThisScroll && i > 0)
                    {
                        Console.WriteLine("No new unique videos detected after scrolling. Ending scraping.");
                        break;
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
    }
}
