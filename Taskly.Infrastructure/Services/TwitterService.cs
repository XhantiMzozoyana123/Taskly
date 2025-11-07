using Microsoft.EntityFrameworkCore;
using Microsoft.Playwright;
using System;
using System.Collections.Generic;
using System.Linq;
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
        private readonly IUiLogger _logger; // Injecting IUiLogger

        public TwitterService(ApplicationDbContext context, IAiService aiService, ICookieService cookieService, IUiLogger logger)
        {
            _context = context;
            _aiService = aiService;
            _cookieService = cookieService;
            _logger = logger; // Initializing the logger
        }

        public async Task SearchAsync(SearchDto searchDto)
        {
            if (string.IsNullOrWhiteSpace(searchDto.Keyword))
            {
                _logger.LogWarning("Search keyword is empty or null, skipping Twitter search operation.");
                return;
            }

            IPage page = null;
            IBrowser browser = null;

            try
            {
                _logger.LogInfo($"Attempting to log in and navigate to Twitter for keyword: '{searchDto.Keyword}'");
                // Login to Twitter
                (page, browser) = await _cookieService.LoadCookieOnPageAsync(searchDto.CookiePath, searchDto.PrivateMode);
                _logger.LogInfo("Successfully loaded cookie and initialized browser page for Twitter.");

                // Navigate to the specified URL (e.g., search results or user timeline)
                page = await GoToTweetsPageAsync(page, searchDto);
                _logger.LogInfo($"Navigated to Twitter search page for keyword: '{searchDto.Keyword}'");

                // Wait for the first tweet element to be visible using the actual main div class
                var mainTweetContainerSelector = ".css-175oi2r";
                var tweetContainerLocator = page.Locator(mainTweetContainerSelector).First;
                await tweetContainerLocator.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible, Timeout = 15000 });
                _logger.LogInfo("First tweet container found on the page.");

                var scrapedTweetPostUrls = new HashSet<string>(); // Track already scraped tweet post URLs to avoid duplicates

                // Loop for scrolling and loading multiple pages of tweets
                for (int i = 0; i < searchDto.PageNumber; i++)
                {
                    _logger.LogInfo($"Scraping page {i + 1} of Twitter posts for keyword: '{searchDto.Keyword}'");
                    // Scrolling strategy: Scroll down to load more content
                    await page.EvaluateAsync(@"window.scrollBy(1, window.innerHeight);");
                    _logger.LogInfo("Scrolled down to load more tweets.");


                    // Get all tweet containers currently loaded on the page
                    var tweetLocators = await page.Locator(mainTweetContainerSelector).AllAsync();
                    _logger.LogInfo($"Found {tweetLocators.Count} tweets on the current view.");


                    foreach (var currentTweetLocator in tweetLocators)
                    {
                        // --- 1. Extract Post URL (Permalink) first for unique tracking ---
                        string? postUrl = null;
                        var postLinkLocator = currentTweetLocator.Locator("a[href*='/status/']").First;
                        if (await postLinkLocator.IsVisibleAsync())
                        {
                            postUrl = await postLinkLocator.GetAttributeAsync("href");
                            postUrl = !string.IsNullOrWhiteSpace(postUrl) ? $"https://x.com{postUrl}" : null;
                        }

                        // Use the absolute post URL as the unique identifier
                        if (string.IsNullOrWhiteSpace(postUrl) || scrapedTweetPostUrls.Contains(postUrl))
                        {
                            if (string.IsNullOrWhiteSpace(postUrl))
                            {
                                _logger.LogWarning("Tweet post URL is null or empty, skipping tweet.");
                            }
                            else
                            {
                                _logger.LogInfo($"Tweet post URL '{postUrl}' already scraped, skipping duplicate.");
                            }
                            continue; // Skip if URL is invalid or already scraped
                        }

                        scrapedTweetPostUrls.Add(postUrl); // Add to set
                        _logger.LogInfo($"Processing new tweet: {postUrl}");


                        // Initialize other data points for the current tweet
                        string? username = null;
                        string? profileUrl = null;
                        string? tweetText = null;
                        DateTime? publishedDate = null;

                        // --- 2. Extract Username (Full Display Name) and Profile URL ---
                        var userDisplayNameLocator = currentTweetLocator.Locator("div[data-testid='User-Name'] a[role='link'] div[dir='ltr'] span.r-poiln3 span.r-poiln3").First;
                        if (await userDisplayNameLocator.IsVisibleAsync())
                        {
                            username = await userDisplayNameLocator.InnerTextAsync();
                            _logger.LogInfo($"Extracted author name: '{username}'");
                        }

                        var userProfileLinkLocator = currentTweetLocator.Locator("div[data-testid='User-Name'] a[role='link']").First;
                        if (await userProfileLinkLocator.IsVisibleAsync())
                        {
                            string? relativeProfileUrl = await userProfileLinkLocator.GetAttributeAsync("href");
                            profileUrl = !string.IsNullOrWhiteSpace(relativeProfileUrl) ? $"https://x.com{relativeProfileUrl}" : null;
                            _logger.LogInfo($"Extracted author profile URL: '{profileUrl}'");
                        }

                        // --- 3. Extract Tweet Text ---
                        var tweetTextLocator = currentTweetLocator.Locator("div[data-testid='tweetText']").First;
                        if (await tweetTextLocator.IsVisibleAsync())
                        {
                            tweetText = await tweetTextLocator.InnerTextAsync();
                            tweetText = tweetText?.Trim();
                            _logger.LogInfo($"Extracted tweet text (first 100 chars): {tweetText?.Substring(0, Math.Min(tweetText.Length, 100)) ?? "N/A"}");
                        }

                        // --- 4. Extract Published Datetime ---
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
                        _logger.LogInfo($"Extracted published date: {publishedDate?.ToString() ?? "N/A"}");


                        // Only process and save if core data exists
                        if (!string.IsNullOrWhiteSpace(username) || !string.IsNullOrWhiteSpace(tweetText))
                        {
                            _logger.LogInfo($"Checking relevance for potential new lead: '{username}'");
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
                                        PostDate = publishedDate ?? DateTime.UtcNow // Use UtcNow if publishedDate is null
                                    };

                                    await _context.Leads.AddAsync(lead);
                                    await _context.SaveChangesAsync();
                                    _logger.LogInfo($"Successfully added new relevant lead: '{username}' from tweet: {postUrl}");
                                }
                                catch (Exception ex)
                                {
                                    // As per your IUiLogger, only a string message is passed to LogError
                                    _logger.LogError($"Error saving lead '{username}' from tweet {postUrl}. Exception: {ex.Message}");
                                }
                            }
                            else
                            {
                                _logger.LogInfo($"Tweet from '{username}' deemed not relevant by AI for query: '{searchDto.Query}'");
                            }
                        }
                        else
                        {
                            _logger.LogWarning($"Skipping tweet due to missing username or tweet text. Post URL: {postUrl}");
                        }
                    }

                    await Task.Delay(2000); // Wait for new tweets to load after scrolling.
                    _logger.LogInfo("Waiting 2 seconds for new tweets to load after scroll.");
                }
            }
            catch (Exception ex)
            {
                // As per your IUiLogger, only a string message is passed to LogError
                _logger.LogError($"An unhandled error occurred during Twitter search for keyword: '{searchDto.Keyword}'. Exception: {ex.Message}");
                throw; // Re-throw the exception after logging
            }
            finally
            {
                if (browser != null)
                {
                    await browser.CloseAsync();
                    _logger.LogInfo("Browser closed in finally block after Twitter search.");
                }
            }
        }

        public async Task<IPage> GoToTweetsPageAsync(IPage page, SearchDto searchDto)
        {
            _logger.LogInfo($"Navigating to X search page for keyword: '{searchDto.Keyword}'");
            // Go to X Explore page
            await page.GotoAsync($"https://x.com/search?q={searchDto.Keyword.Replace(" ", "+")}", new PageGotoOptions
            {
                WaitUntil = WaitUntilState.DOMContentLoaded
            });
            _logger.LogInfo("Successfully navigated to X search page.");

            return page;
        }

        public async Task<IPage> DirectMessagingAsync(IPage page, MessengerDto messengerDto)
        {
            _logger.LogInfo($"Navigating to user's X profile page for direct messaging: {messengerDto.Lead.ProfileUrl}");
            // Go to the user's X profile page
            await page.GotoAsync(messengerDto.Lead.ProfileUrl, new PageGotoOptions
            {
                WaitUntil = WaitUntilState.DOMContentLoaded,
                Timeout = 30000
            });
            _logger.LogInfo("Navigated to X profile page.");

            // Wait for the Message button to appear
            var messageButton = page.Locator("button[data-testid='sendDMFromProfile']");
            await messageButton.WaitForAsync(new LocatorWaitForOptions
            {
                State = WaitForSelectorState.Visible,
                Timeout = 10000
            });
            _logger.LogInfo("Message button located.");

            // Click the Message button
            await messageButton.ClickAsync();
            _logger.LogInfo("Clicked 'Message' button.");


            // Wait for the DM input to appear (Draft.js editor)
            var dmInput = page.Locator("div.public-DraftStyleDefault-block");
            await dmInput.WaitForAsync(new LocatorWaitForOptions
            {
                State = WaitForSelectorState.Visible,
                Timeout = 10000
            });
            _logger.LogInfo("DM input field located.");

            // Focus the Draft.js input and type the message
            await dmInput.ClickAsync();
            await page.Keyboard.TypeAsync(messengerDto.Text);
            _logger.LogInfo($"Typed message into DM input: '{messengerDto.Text.Substring(0, Math.Min(messengerDto.Text.Length, 50))}...'");

            // Send the message (Enter sends DM)
            await page.Keyboard.PressAsync("Enter");
            _logger.LogInfo("Pressed Enter to send message.");

            await UpdateLead(messengerDto.Lead);

            // Wait for network idle to ensure send completes
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
            _logger.LogInfo("Direct message sent, waiting for network idle.");

            return page;
        }


        private async Task UpdateLead(Leads lead)
        {
            var query = await _context.Leads.Where(x => x.Name == lead.Name).ToListAsync();

            foreach (var item in query)
            {
                item.Status = "Contacted";
                _context.Leads.Update(item);
                await _context.SaveChangesAsync();
                _logger.LogInfo($"Lead '{item.Name}' status updated to 'Contacted' in the database.");
            }
        }

        public async Task<IPage> ExtractSelectedProfileAsync(IPage page)
        {
            var authorContainer = await page.QuerySelectorAsync("div[dir='ltr']"); // the div containing the author name
            var authorText = await authorContainer.TextContentAsync();

            Leads leads = new Leads
            {
                Name = authorText,
                ProfileUrl = page.Url,
                Status = "New",
                Platform = "Twitter",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Leads.Add(leads);
            await _context.SaveChangesAsync();

            return page;
        }
    }
}