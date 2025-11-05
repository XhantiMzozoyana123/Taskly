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
        private readonly ICookieService _cookieService;
        private readonly IUiLogger _logger; // Injecting IUiLogger

        public TikTokService(ApplicationDbContext context, IAiService aiService, ICookieService cookieService, IUiLogger logger)
        {
            _context = context;
            _aiService = aiService;
            _cookieService = cookieService;
            _logger = logger; // Initializing the logger
        }

        public async Task<IPage> DirectMessagingAsync(IPage page, MessengerDto messengerDto)
        {
            _logger.LogInfo($"Navigating to target user's TikTok profile for direct messaging: {messengerDto.Lead.ProfileUrl}");
            // Navigate to the target user's profile
            await page.GotoAsync(messengerDto.Lead.ProfileUrl, new PageGotoOptions
            {
                WaitUntil = WaitUntilState.DOMContentLoaded,
                Timeout = 30000
            });
            _logger.LogInfo("Navigated to TikTok profile.");

            // --- STEP 1: Click the "Message" button ---
            var messageButton = page.Locator("[data-e2e='message-button']");
            if (await messageButton.CountAsync() == 0)
            {
                _logger.LogWarning($"Message button not found for profile: {messengerDto.Lead.ProfileUrl}. DMs might be closed or selector changed.");
                return page; // Skip if user’s DMs are closed
            }

            await messageButton.First.WaitForAsync(new LocatorWaitForOptions
            {
                State = WaitForSelectorState.Attached,
                Timeout = 10000
            });
            await messageButton.First.ClickAsync();
            _logger.LogInfo("Clicked 'Message' button.");

            // --- STEP 2: Wait for the message input box ---
            var messageInput = page.Locator("div[contenteditable='true'].public-DraftEditor-content");
            await messageInput.WaitForAsync(new LocatorWaitForOptions
            {
                State = WaitForSelectorState.Visible,
                Timeout = 10000
            });
            _logger.LogInfo("Message input box located.");

            // --- STEP 3: Type and send message ---
            await messageInput.ClickAsync();
            await messageInput.FillAsync(messengerDto.Text); // FillAsync works on editable divs
            _logger.LogInfo($"Typed message into DM input (first 50 chars): '{messengerDto.Text.Substring(0, Math.Min(messengerDto.Text.Length, 50))}...'");

            // Some platforms require Enter key to send
            await page.Keyboard.PressAsync("Enter");
            _logger.LogInfo("Pressed Enter to send message.");

            // --- STEP 4: Wait to ensure the message is sent ---
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle, new PageWaitForLoadStateOptions
            {
                Timeout = 10000
            });
            _logger.LogInfo("Direct message sent, waiting for network idle.");

            return page;
        }

        public async Task<string> GetVideoDescription(IPage page)
        {
            _logger.LogInfo("Attempting to get video description.");
            var videoDescriptionElement = page.Locator("[data-e2e='video-desc']");
            string videoDescription = await videoDescriptionElement.TextContentAsync();

            if (!string.IsNullOrWhiteSpace(videoDescription))
            {
                _logger.LogInfo($"Extracted video description (first 100 chars): {videoDescription.Substring(0, Math.Min(videoDescription.Length, 100))}");
            }
            else
            {
                _logger.LogWarning("Video description element found, but content was empty or null.");
            }
            return videoDescription?.Trim() ?? string.Empty;
        }

        public async Task<IPage> GoToDiscoveryPageAsync(IPage page, SearchDto searchDto)
        {
            _logger.LogInfo($"Navigating to TikTok Discovery page for keyword: '{searchDto.Keyword}'");
            // Go to Tik-Tok Discovery page
            await page.GotoAsync($"https://www.tiktok.com/search?q={searchDto.Keyword.Replace(" ", "+")}", new PageGotoOptions
            {
                WaitUntil = WaitUntilState.DOMContentLoaded
            });
            _logger.LogInfo("Successfully navigated to TikTok Discovery page.");
            await Task.Delay(5000);

            if (await page.Locator("button:has-text(\"Try again\")").IsVisibleAsync())
            {
                _logger.LogWarning("Found 'Try again' button, attempting to click it.");
                await page.Locator("button:has-text(\"Try again\")").ClickAsync();
                _logger.LogInfo("'Try again' button clicked.");
            }

            // Wait for the video container to appear
            await page.WaitForSelectorAsync("a.css-143ggr2-5e6d46e3--AVideoContainer");
            _logger.LogInfo("First video container found on the discovery page.");

            // Click the video
            await page.ClickAsync("a.css-143ggr2-5e6d46e3--AVideoContainer");
            _logger.LogInfo("Clicked the first video container.");

            // Optional: wait a bit to see the result
            await page.WaitForTimeoutAsync(9000);
            _logger.LogInfo("Waited 9 seconds after clicking the video.");

            return page;
        }

        public async Task SearchAsync(SearchDto searchDto)
        {
            if (string.IsNullOrWhiteSpace(searchDto.Keyword))
            {
                _logger.LogWarning("Search keyword is empty or null, skipping TikTok search operation.");
                return;
            }

            IPage page = null;
            IBrowser browser = null;

            try
            {
                _logger.LogInfo($"Attempting to log in and navigate to TikTok for keyword: '{searchDto.Keyword}'");
                // Login to Tik-Tok
                (page, browser) = await _cookieService.LoadCookieOnPageAsync(searchDto.CookiePath, searchDto.PrivateMode);
                _logger.LogInfo("Successfully loaded cookie and initialized browser page for TikTok.");

                // Navigate to the specified URL (e.g., search results or user timeline)
                page = await GoToDiscoveryPageAsync(page, searchDto);
                _logger.LogInfo($"Navigated to TikTok video details page for keyword: '{searchDto.Keyword}'");


                // Store the base URL for constructing absolute URLs later
                string tiktokVideoUrl = page.Url; // This will be the URL of the currently playing video

                var processedVideoIdentifiers = new HashSet<string>();
                int maxScrollAttempts = searchDto.PageNumber;
                _logger.LogInfo($"Starting to scrape TikTok videos. Will attempt up to {maxScrollAttempts} scroll cycles.");

                for (int i = 0; i < maxScrollAttempts; i++)
                {
                    _logger.LogInfo($"Processing TikTok video, scroll attempt {i + 1}/{maxScrollAttempts}.");
                    try
                    {
                        string? username = null;
                        string? profileUrl = null;
                        string? videoDescription = null;

                        // Wait until the profile link is visible
                        await page.WaitForSelectorAsync("a.css-tppnop-5e6d46e3--StyledLink");
                        _logger.LogInfo("Profile link element found.");

                        // Select the <a> element
                        var profileLink = await page.QuerySelectorAsync("a.css-tppnop-5e6d46e3--StyledLink");

                        if (profileLink == null)
                        {
                            _logger.LogWarning("Profile link element not found after waiting. Skipping current video.");
                            await page.Keyboard.PressAsync("ArrowDown"); // Try to move to the next video
                            await Task.Delay(2000);
                            continue;
                        }

                        // Extract href (profile URL)
                        profileUrl = await profileLink.GetAttributeAsync("href");
                        profileUrl = profileUrl != null && profileUrl.StartsWith("http") ? profileUrl : $"https://www.tiktok.com{profileUrl}";
                        _logger.LogInfo($"Extracted profile URL: '{profileUrl}'");

                        // Extract username
                        var usernameElement = await profileLink.QuerySelectorAsync("span[data-e2e='browse-username'] span.css-6tu85p-5e6d46e3--SpanEllipsis");
                        if (usernameElement != null)
                        {
                            username = await usernameElement.InnerTextAsync();
                            _logger.LogInfo($"Extracted username: '{username}'");
                        }
                        else
                        {
                            _logger.LogWarning("Username element not found for the current video.");
                            username = "Unknown TikTok User";
                        }

                        // Video Description
                        videoDescription = await GetVideoDescription(page);
                        _logger.LogInfo($"Extracted video description (first 100 chars): {videoDescription?.Substring(0, Math.Min(videoDescription.Length, 100)) ?? "N/A"}");


                        // Only process and save if core data exists
                        if (!string.IsNullOrWhiteSpace(username) || !string.IsNullOrWhiteSpace(videoDescription))
                        {
                            _logger.LogInfo($"Checking relevance for TikTok video by '{username}'");
                            // Use AI service to check if the content is relevant
                            var isRelevant = await _aiService.CheckIfContentIsRelevantAsync(videoDescription, searchDto.Query);

                            if (isRelevant)
                            {
                                try
                                {
                                    var lead = new Leads()
                                    {
                                        Name = username,
                                        ProfileUrl = profileUrl,
                                        Status = "New",
                                        Platform = "TikTok",
                                        PostDescription = videoDescription,
                                        PostUrl = tiktokVideoUrl, // This URL might need to be specific to the video, not just the search page.
                                        Keywords = searchDto.Keyword,
                                        Query = searchDto.Query,
                                        PostDate = DateTime.UtcNow // Using UtcNow as no specific post date is extracted
                                    };

                                    await _context.Leads.AddAsync(lead);
                                    await _context.SaveChangesAsync();
                                    _logger.LogInfo($"Successfully added new relevant lead: '{username}' from TikTok video.");
                                }
                                catch (Exception ex)
                                {
                                    _logger.LogError($"Error saving lead '{username}' from TikTok video. Exception: {ex.Message}");
                                }
                            }
                            else
                            {
                                _logger.LogInfo($"TikTok video by '{username}' deemed not relevant by AI for query: '{searchDto.Query}'");
                            }
                        }
                        else
                        {
                            _logger.LogWarning($"Skipping TikTok video due to missing username or video description.");
                        }

                        await page.Keyboard.PressAsync("ArrowDown");
                        await Task.Delay(2000); // Small delay to allow the next video to load
                        _logger.LogInfo("Pressed ArrowDown to navigate to the next video, waiting 2 seconds.");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"Error during TikTok video processing in scroll attempt {i + 1}. Attempting to move to next video. Exception: {ex.Message}");
                        await page.Keyboard.PressAsync("ArrowDown");
                        await Task.Delay(2000); // Add a delay after error to stabilize
                        continue; // Skip to the next iteration
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"An unhandled error occurred during TikTok search for keyword: '{searchDto.Keyword}'. Exception: {ex.Message}");
                throw; // Re-throw the exception after logging
            }
            finally
            {
                if (browser != null)
                {
                    await browser.CloseAsync();
                    _logger.LogInfo("Browser closed in finally block after TikTok search.");
                }
            }
        }
    }
}