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
        private readonly ICookieService _cookieService;
        private readonly IUiLogger _logger; // Injecting IUiLogger

        public InstagramService(ApplicationDbContext context, IAiService aiService, ICookieService cookieService, IUiLogger logger)
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
                _logger.LogWarning("Search keyword is empty or null, skipping Instagram search operation.");
                return;
            }

            IPage page = null;
            IBrowser browser = null;

            try
            {
                _logger.LogInfo($"Attempting to log in and navigate to Instagram for keyword: '{searchDto.Keyword}'");
                // Login to Instagram
                (page, browser) = await _cookieService.LoadCookieOnPageAsync(searchDto.CookiePath, searchDto.PrivateMode);
                _logger.LogInfo("Successfully loaded cookie and initialized browser page for Instagram.");

                // Go to Explore Page to search for hashtags or keywords
                page = await GoToExplorePageAsync(page, searchDto);
                _logger.LogInfo($"Navigated to Instagram explore page for keyword: '{searchDto.Keyword}'");

                // Wait for at least one Instagram post link to be visible
                var postLinkLocator = page.Locator("a._a6hd").First;
                await postLinkLocator.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible, Timeout = 15000 });
                _logger.LogInfo("First Instagram post link found on the explore page.");

                var scrapedPostUrls = new HashSet<string>(); // Track already scraped post URLs

                // Loop for scrolling and loading multiple pages of Instagram posts
                for (int i = 0; i < searchDto.PageNumber; i++)
                {
                    _logger.LogInfo($"Scraping page {i + 1} of Instagram posts for keyword: '{searchDto.Keyword}'");
                    var instagramPostLocators = await page.Locator("a._a6hd").AllAsync();
                    _logger.LogInfo($"Found {instagramPostLocators.Count} posts on the current view.");


                    foreach (var currentPostLocator in instagramPostLocators)
                    {
                        string? postUrl = await currentPostLocator.GetAttributeAsync("href");
                        string? absolutePostUrl = !string.IsNullOrWhiteSpace(postUrl)
                            ? $"https://www.instagram.com{postUrl}"
                            : null;

                        if (string.IsNullOrWhiteSpace(absolutePostUrl) || scrapedPostUrls.Contains(absolutePostUrl))
                        {
                            if (string.IsNullOrWhiteSpace(absolutePostUrl))
                            {
                                _logger.LogWarning("Post URL is null or empty, skipping post.");
                            }
                            else
                            {
                                _logger.LogInfo($"Post URL '{absolutePostUrl}' already scraped, skipping duplicate.");
                            }
                            continue;
                        }

                        scrapedPostUrls.Add(absolutePostUrl);
                        _logger.LogInfo($"Processing new Instagram post: {absolutePostUrl}");

                        string? imageUrl = null;
                        string? altText = null;

                        var imgLocator = currentPostLocator.Locator("img.x5yr21d").First;

                        if (await imgLocator.IsVisibleAsync())
                        {
                            imageUrl = await imgLocator.GetAttributeAsync("src");
                            altText = await imgLocator.GetAttributeAsync("alt");
                            _logger.LogInfo($"Extracted image URL: {imageUrl?.Substring(0, Math.Min(imageUrl.Length, 50))}... and alt text (first 50 chars): {altText?.Substring(0, Math.Min(altText.Length, 50)) ?? "N/A"}");
                        }
                        else
                        {
                            _logger.LogWarning($"Image locator not visible for post URL: {absolutePostUrl}. Skipping image extraction.");
                        }

                        if (!string.IsNullOrWhiteSpace(imageUrl))
                        {
                            var postDto = await GetAuthorPost(page, absolutePostUrl);

                            if (postDto != null)
                            {
                                _logger.LogInfo($"Checking relevance for Instagram post by '{postDto.Author}' with content (first 100 chars): {postDto.Text?.Substring(0, Math.Min(postDto.Text.Length, 100)) ?? "N/A"}");
                                var isRelevant = await _aiService.CheckIfContentIsRelevantAsync(altText, searchDto.Query);

                                if (isRelevant)
                                {
                                    try
                                    {
                                        var lead = new Leads
                                        {
                                            Name = postDto.Author,
                                            ProfileUrl = postDto.ProfileUrl,
                                            Status = "New",
                                            Platform = "Instagram",
                                            PostDescription = postDto.Text,
                                            PostUrl = absolutePostUrl,
                                            Keywords = searchDto.Keyword,
                                            Query = searchDto.Query,
                                            PostDate = postDto.PublishedDate
                                        };

                                        await _context.Leads.AddAsync(lead);
                                        await _context.SaveChangesAsync();
                                        _logger.LogInfo($"Successfully added new relevant lead: '{postDto.Author}' from Instagram post: {absolutePostUrl}");
                                    }
                                    catch (Exception ex)
                                    {
                                        _logger.LogError($"Error saving lead '{postDto.Author}' from Instagram post: {absolutePostUrl}. Exception: {ex.Message}");
                                    }
                                }
                                else
                                {
                                    _logger.LogInfo($"Instagram post by '{postDto.Author}' deemed not relevant by AI for query: '{searchDto.Query}'");
                                }
                            }
                            else
                            {
                                _logger.LogWarning($"Could not retrieve PostContentDto for URL: {absolutePostUrl}. Skipping lead creation.");
                            }
                        }
                        else
                        {
                            _logger.LogWarning($"Skipping post {absolutePostUrl} due to missing image URL.");
                        }
                    }

                    _logger.LogInfo("Scrolling down to load more Instagram posts.");
                    await page.EvaluateAsync(@"window.scrollBy(0, window.innerHeight);");
                    await Task.Delay(2000); // Wait for new posts to load
                    _logger.LogInfo("Scroll complete, waiting for new content to load.");
                }

            }
            catch (Exception ex)
            {
                _logger.LogError($"An unhandled error occurred during Instagram search for keyword: '{searchDto.Keyword}'. Exception: {ex.Message}");
                throw; // Re-throw the exception after logging
            }
            finally
            {
                if (browser != null)
                {
                    await browser.CloseAsync();
                    _logger.LogInfo("Browser closed in finally block after Instagram search.");
                }
            }
        }

        public async Task<IPage> GoToExplorePageAsync(IPage page, SearchDto searchDto)
        {
            _logger.LogInfo($"Navigating to Instagram Explore page for keyword: '{searchDto.Keyword}'");
            await page.GotoAsync($"https://www.instagram.com/explore/search/keyword/?q={searchDto.Keyword.Replace(" ", "+")}", new PageGotoOptions
            {
                WaitUntil = WaitUntilState.DOMContentLoaded
            });
            _logger.LogInfo("Successfully navigated to Instagram Explore page.");
            return page;
        }

        public async Task<PostContentDto> GetAuthorPost(IPage page, string postUrl)
        {
            if (string.IsNullOrWhiteSpace(postUrl))
            {
                _logger.LogWarning("Post URL is null or empty, cannot get author post details.");
                return null;
            }

            _logger.LogInfo($"Attempting to get author and post details for URL: {postUrl}");
            var newPage = await page.Context.NewPageAsync();
            var postContentDto = new PostContentDto();

            try
            {
                await newPage.GotoAsync(postUrl, new PageGotoOptions
                {
                    WaitUntil = WaitUntilState.DOMContentLoaded,
                    Timeout = 30000
                });
                _logger.LogInfo($"Navigated to individual Instagram post page: {postUrl}");

                // Wait for essential elements
                await newPage.Locator("time.xdwrcjd").WaitForAsync(new LocatorWaitForOptions
                {
                    State = WaitForSelectorState.Visible,
                    Timeout = 10000
                });
                _logger.LogInfo("Time element for post date located.");


                // Wait for the link element to load (assuming it's for the username)
                await newPage.WaitForSelectorAsync("a.x1i10hfl.xjbqb8w.x1ejq31n");
                _logger.LogInfo("Username link element located.");


                // Username
                string usernameRaw = await newPage.GetAttributeAsync("a.x1i10hfl.xjbqb8w.x1ejq31n", "href");
                string username = usernameRaw?.Replace("/", "").Trim() ?? string.Empty;
                _logger.LogInfo($"Extracted username: '{username}'");

                // Extract the caption text
                string caption = await newPage.InnerHTMLAsync("div.html-div");
                caption = AppConstants.ConvertHtmlToPlainText(caption);
                _logger.LogInfo($"Extracted caption (first 100 chars): {caption?.Substring(0, Math.Min(caption.Length, 100)) ?? "N/A"}");

                // Datetime
                DateTime? postDate = null;
                var timeLocator = newPage.Locator("time.xdwrcjd");
                if (await timeLocator.IsVisibleAsync())
                {
                    var datetimeAttr = await timeLocator.GetAttributeAsync("datetime");
                    if (!string.IsNullOrWhiteSpace(datetimeAttr) &&
                        DateTime.TryParse(datetimeAttr, out DateTime parsedDate))
                    {
                        postDate = parsedDate;
                    }
                }
                _logger.LogInfo($"Extracted published date: {postDate?.ToString() ?? "N/A"}");

                postContentDto.Author = username;
                postContentDto.Text = caption;
                postContentDto.PostUrl = postUrl;
                postContentDto.ProfileUrl = !string.IsNullOrWhiteSpace(username)
                    ? $"https://www.instagram.com/{username}/"
                    : string.Empty;
                postContentDto.PublishedDate = postDate ?? DateTime.UtcNow;
                _logger.LogInfo($"Successfully compiled PostContentDto for post: {postUrl}");

                return postContentDto;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in GetAuthorPost for URL: {postUrl}. Exception: {ex.Message}");
                return null;
            }
            finally
            {
                await newPage.CloseAsync();
                _logger.LogInfo("Temporary page for author post details closed.");
            }
        }


        public async Task<IPage> DirectMessagingAsync(IPage page, MessengerDto messengerDto)
        {
            _logger.LogInfo($"Navigating to user's Instagram profile for direct messaging: {messengerDto.Lead.ProfileUrl}");
            // Navigate to the user's Instagram profile
            await page.GotoAsync(messengerDto.Lead.ProfileUrl, new PageGotoOptions
            {
                WaitUntil = WaitUntilState.DOMContentLoaded,
                Timeout = 30000
            });
            _logger.LogInfo("Navigated to Instagram profile.");

            // Wait for and click the "Message" button
            var messageButton = page.Locator("div[role='button']:has-text('Message')");
            if (await messageButton.CountAsync() == 0)
            {
                _logger.LogWarning($"Message button not found for profile: {messengerDto.Lead.ProfileUrl}. DMs might be disabled or selector changed.");
                return page; // Skip users with closed DMs
            }

            await messageButton.First.ClickAsync();
            _logger.LogInfo("Clicked 'Message' button.");

            // Wait for the Lexical Editor input to appear
            var messageInput = page.Locator("div[contenteditable='true'][role='textbox']");
            await messageInput.WaitForAsync(new LocatorWaitForOptions
            {
                State = WaitForSelectorState.Visible,
                Timeout = 10000
            });
            _logger.LogInfo("Direct message input field located.");

            // Focus and type the message
            await messageInput.ClickAsync();
            await page.Keyboard.TypeAsync(messengerDto.Text);
            _logger.LogInfo($"Typed message into DM input: '{messengerDto.Text.Substring(0, Math.Min(messengerDto.Text.Length, 50))}...'");

            // Press Enter to send the DM
            await page.Keyboard.PressAsync("Enter");
            _logger.LogInfo("Pressed Enter to send message.");

            // Wait for idle network to ensure message is sent
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
            _logger.LogInfo("Direct message sent, waiting for network idle.");

            return page;
        }
    }
}