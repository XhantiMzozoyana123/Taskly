using HtmlAgilityPack;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.Extensions.Hosting;
using Microsoft.Playwright;
using Newtonsoft.Json;
using System;
using System.Buffers.Text;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Taskly.Application.Constants; // Assuming TokenEncryptor is here
using Taskly.Application.Dtos;
using Taskly.Application.Interfaces;
using Taskly.Domain; // Assuming ApplicationDbContext is here
using Taskly.Domain.Entities; // Assuming Posts, Leads, SocialLogins are here

namespace Taskly.Infrastructure.Services
{
    public class FacebookService : IFacebookService
    {
        private readonly ApplicationDbContext _context;
        private readonly IAiService _aiService;
        private readonly ICookieService _cookieService;
        private readonly IUiLogger _logger; // Correctly referencing IUiLogger

        public FacebookService(ApplicationDbContext context, IAiService aiService, ICookieService cookieService, IUiLogger logger)
        {
            _context = context;
            _aiService = aiService;
            _cookieService = cookieService;
            _logger = logger;
        }

        public async Task SearchAsync(SearchDto searchDto)
        {
            if (string.IsNullOrWhiteSpace(searchDto.Keyword))
            {
                _logger.LogWarning("Search keyword is empty or null, skipping search operation.");
                return;
            }

            IPage page = null;
            IBrowser browser = null;

            try
            {
                _logger.LogInfo($"Attempting to load cookie and navigate for keyword: '{searchDto.Keyword}'");
                // Login to Facebook
                (page, browser) = await _cookieService.LoadCookieOnPageAsync(searchDto.CookiePath, searchDto.PrivateMode);
                _logger.LogInfo("Successfully loaded cookie and initialized browser page.");

                // Navigate to Facebook Groups search page
                page = await GoToFacebookGroupPage(page, searchDto);
                _logger.LogInfo($"Navigated to Facebook group search page for keyword: '{searchDto.Keyword}'");

                // Get all Facebook groups from the search results
                var facebookGroupList = await SelectAllFacebookFacebookGroups(page, searchDto);
                _logger.LogInfo($"Found {facebookGroupList.Count} Facebook groups related to keyword: '{searchDto.Keyword}'.");

                foreach (var groupUrl in facebookGroupList)
                {
                    try
                    {
                        _logger.LogInfo($"Navigating to group URL: {groupUrl}");
                        await page.GotoAsync(groupUrl, new PageGotoOptions
                        {
                            WaitUntil = WaitUntilState.DOMContentLoaded
                        });
                        _logger.LogInfo($"Successfully navigated to group: {groupUrl}");

                        // Wait for the first post container to appear using the specific classes provided
                        string mainPostContainerSelector = "div.html-div.xdj266r.x14z9mp.xat24cr.x1lziwak.xexx8yu.xyri2b.x18d9i69.x1c1uobl:has(h3[id*='_r_'])";
                        // The original code had a commented-out WaitForAsync, keeping it commented as per original
                        // var firstPostLocator = page.Locator(mainPostContainerSelector).First;
                        // await firstPostLocator.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible, Timeout = 15000 });
                        _logger.LogInfo("Waiting for first post container to appear (implicit wait based on page load).");


                        var scrapedPostPermalinks = new HashSet<string>(); // Track already scraped post URLs to avoid duplicates

                        // Loop for scrolling and loading multiple pages of posts
                        for (int i = 0; i < searchDto.PageNumber; i++)
                        {
                            _logger.LogInfo($"Scraping page {i + 1} for group: {groupUrl}");
                            // Get all post containers currently loaded on the page
                            var postLocators = await page.Locator(mainPostContainerSelector).AllAsync();
                            _logger.LogInfo($"Found {postLocators.Count} posts on the current view for group: {groupUrl}.");

                            foreach (var currentPostLocator in postLocators)
                            {
                                // --- 1. Extract Post URL (Permalink) first for unique tracking ---
                                string? postPermalink = null;
                                var postContentLinkLocator = currentPostLocator.Locator("a[attributionsrc][href*='fbid']").First;
                                if (await postContentLinkLocator.IsVisibleAsync())
                                {
                                    postPermalink = await postContentLinkLocator.GetAttributeAsync("href");
                                }

                                if (string.IsNullOrWhiteSpace(postPermalink) || scrapedPostPermalinks.Contains(postPermalink))
                                {
                                    if (string.IsNullOrWhiteSpace(postPermalink))
                                    {
                                        _logger.LogWarning("Post permalink is null or empty, skipping post.");
                                    }
                                    else if (scrapedPostPermalinks.Contains(postPermalink))
                                    {
                                        _logger.LogInfo($"Post permalink '{postPermalink}' already scraped, skipping duplicate.");
                                    }
                                    continue;
                                }

                                scrapedPostPermalinks.Add(postPermalink);
                                _logger.LogInfo($"Scraping new post with permalink: {postPermalink}");

                                // Initialize other data points
                                string? authorName = null;
                                string? authorProfileUrl = null;
                                string? postText = null;
                                DateTime? publishedDate = DateTime.UtcNow; // Fallback

                                // --- 2. Extract Author's Name and Profile URL ---
                                var authorNameLocator = currentPostLocator.Locator("h3[id*='_r_'] a span.html-span").First;
                                if (await authorNameLocator.IsVisibleAsync())
                                {
                                    authorName = await authorNameLocator.InnerTextAsync();
                                    _logger.LogInfo($"Extracted author name: '{authorName}'");
                                }

                                var authorProfileLinkLocator = currentPostLocator.Locator("h3[id*='_r_'] a[attributionsrc]").First;
                                if (await authorProfileLinkLocator.IsVisibleAsync())
                                {
                                    string rawProfileUrl = await authorProfileLinkLocator.GetAttributeAsync("href");
                                    authorProfileUrl = await AuthorProfileUrlExchangedUrlAsync(page, rawProfileUrl);
                                    _logger.LogInfo($"Extracted author profile URL: '{authorProfileUrl}'");
                                }

                                // --- 3. Extract Post Text ---
                                postText = await GetPostDescriptionAsync(page, authorProfileUrl);
                                _logger.LogInfo($"Extracted post text (first 100 chars): {postText?.Substring(0, Math.Min(postText.Length, 100)) ?? "N/A"}");

                                // Only process and save if core data exists
                                if (!string.IsNullOrWhiteSpace(authorName) || !string.IsNullOrWhiteSpace(postText))
                                {
                                    var existingLead = await _context.Leads.FirstOrDefaultAsync(l =>
                                               l.Name == authorName);

                                    if (existingLead == null)
                                    {
                                        _logger.LogInfo($"Checking relevance for potential new lead: '{authorName}'");
                                        var isRelevant = await _aiService.CheckIfContentIsRelevantAsync(postText, searchDto.Query);

                                        if (isRelevant)
                                        {
                                            try
                                            {
                                                var lead = new Leads()
                                                {
                                                    Name = authorName,
                                                    ProfileUrl = authorProfileUrl,
                                                    Status = "New",
                                                    Platform = "Facebook",
                                                    PostDescription = postText,
                                                    PostUrl = postPermalink,
                                                    Keywords = searchDto.Keyword,
                                                    Query = searchDto.Query,
                                                    PostDate = publishedDate.Value
                                                };

                                                await _context.Leads.AddAsync(lead);
                                                await _context.SaveChangesAsync();
                                                _logger.LogInfo($"Successfully added new relevant lead: '{authorName}' from {groupUrl}");
                                            }
                                            catch (Exception ex)
                                            {
                                                // Log the exception message in LogError, as the interface doesn't support passing Exception objects
                                                _logger.LogError($"Error saving lead '{authorName}' from {groupUrl}. Exception: {ex.Message}");
                                                continue; // Skip saving this lead on error
                                            }
                                        }
                                        else
                                        {
                                            _logger.LogInfo($"Post from '{authorName}' deemed not relevant by AI for query: '{searchDto.Query}'");
                                        }
                                    }
                                    else
                                    {
                                        _logger.LogInfo($"Lead with name '{authorName}' already exists, skipping.");
                                    }
                                }
                                else
                                {
                                    _logger.LogWarning($"Skipping post due to missing author name or post text. Post Permalink: {postPermalink}");
                                }
                            }

                            _logger.LogInfo("Scrolling down to load more posts.");
                            await page.EvaluateAsync(@"window.scrollBy(0, window.innerHeight);");
                            await Task.Delay(2000); // Wait for new posts to load after scrolling.
                            _logger.LogInfo("Scroll complete, waiting for content to load.");

                            // Optional: Add a check here to see if scrolling actually loaded new content
                            // (e.g., compare scrapedPostPermalinks.Count before and after scroll. If no new posts, break.)
                        }
                    }
                    catch (Exception ex)
                    {
                        // Log the exception message in LogError
                        _logger.LogError($"Error processing group URL: {groupUrl}. Skipping to the next group. Exception: {ex.Message}");
                        continue; // Skip to the next group URL on error
                    }
                }
            }
            catch (Exception ex)
            {
                // Log the exception message in LogError
                _logger.LogError($"An unhandled error occurred during Facebook search for keyword: '{searchDto.Keyword}'. Exception: {ex.Message}");
                throw; // Re-throw the exception after logging
            }
            finally
            {
                if (browser != null)
                {
                    await browser.CloseAsync();
                    _logger.LogInfo("Browser closed in finally block.");
                }
            }
        }

        public async Task<IPage> GoToFacebookGroupPage(IPage page, SearchDto searchDto)
        {
            _logger.LogInfo("Navigating to Facebook groups feed.");
            await page.GotoAsync("https://www.facebook.com/groups/feed", new PageGotoOptions
            {
                WaitUntil = WaitUntilState.DOMContentLoaded
            });
            _logger.LogInfo("Successfully navigated to groups feed.");

            var searchInput = page.Locator("input[aria-label='Search groups'], input[placeholder='Search groups']");
            await searchInput.WaitForAsync();
            _logger.LogInfo("Search input field located.");

            await searchInput.FillAsync(searchDto.Keyword);
            _logger.LogInfo($"Filled search input with keyword: '{searchDto.Keyword}'");

            await searchInput.PressAsync("Enter");
            _logger.LogInfo("Pressed Enter to initiate search.");

            await Task.Delay(5000); // Wait for results to load
            _logger.LogInfo("Waited 5 seconds for search results to load.");

            return page;
        }

        public async Task<IPage> DirectMessagingAsync(IPage page, MessengerDto messengerDto)
        {
            _logger.LogInfo($"Navigating to profile URL for direct messaging: {messengerDto.Lead.ProfileUrl}");
            await page.GotoAsync(messengerDto.Lead.ProfileUrl, new PageGotoOptions
            {
                WaitUntil = WaitUntilState.DOMContentLoaded,
            });

            await Task.Delay(5000);
            _logger.LogInfo("Navigated to profile, waiting 5 seconds.");

            var messageButton = page.Locator("span:has-text('Message')");
            if (await messageButton.CountAsync() == 0)
            {
                _logger.LogWarning($"Message button not found for profile: {messengerDto.Lead.ProfileUrl}. DMs might be disabled or selector changed.");
                return page;
            }

            await Task.Delay(5000);

            await messageButton.First.ClickAsync();
            _logger.LogInfo("Clicked 'Message' button.");

            var dmInput = page.Locator("div[contenteditable='true'][role='textbox'][data-lexical-editor='true'][aria-placeholder='Aa']");
            await dmInput.WaitForAsync(new LocatorWaitForOptions
            {
                State = WaitForSelectorState.Visible,
            });
            _logger.LogInfo("DM input field located.");

            await Task.Delay(5000);

            await dmInput.ClickAsync();
            await page.Keyboard.TypeAsync(messengerDto.Text);
            _logger.LogInfo($"Typed message into DM input: '{messengerDto.Text.Substring(0, Math.Min(messengerDto.Text.Length, 50))}...'");

            await page.Keyboard.PressAsync("Enter");
            _logger.LogInfo("Pressed Enter to send message.");

            await Task.Delay(5000);
            _logger.LogInfo("Direct message sent, waiting 5 seconds for network idle (if applicable).");

            return page;
        }

        public async Task<IPage> SelectRandomFacebookGroup(IPage page, SearchDto searchDto)
        {
            _logger.LogInfo("Attempting to select a random Facebook group.");
            await page.WaitForSelectorAsync("a[href*='facebook.com/groups']");
            _logger.LogInfo("Group links selector found.");

            var anchors = await page.QuerySelectorAllAsync("a[href*='facebook.com/groups']");
            _logger.LogInfo($"Found {anchors.Count} group links.");

            // Log all found links for visibility
            foreach (var anchor in anchors)
            {
                var href = await anchor.GetAttributeAsync("href");
                var text = await anchor.InnerTextAsync();
                if (!string.IsNullOrEmpty(href))
                {
                    _logger.LogInfo($"Found Group Link: '{text}' -> '{href}'");
                }
            }

            if (anchors.Count == 0)
            {
                _logger.LogWarning("No Facebook group links found to select a random one.");
                return page;
            }

            var random = new Random();
            var randomAnchor = anchors[random.Next(anchors.Count)];
            var randomHref = await randomAnchor.GetAttributeAsync("href");

            if (!string.IsNullOrWhiteSpace(randomHref))
            {
                _logger.LogInfo($"Navigating to randomly selected group: {randomHref}");
                await page.GotoAsync(randomHref, new PageGotoOptions
                {
                    WaitUntil = WaitUntilState.DOMContentLoaded,
                    Timeout = 30000
                });
                _logger.LogInfo($"Successfully navigated to random group: {randomHref}");
            }
            else
            {
                _logger.LogWarning("Randomly selected group's href was empty or null.");
            }

            return page;
        }

        public async Task<List<string>> SelectAllFacebookFacebookGroups(IPage page, SearchDto searchDto)
        {
            _logger.LogInfo("Attempting to select all Facebook groups from search results.");
            await page.WaitForSelectorAsync("a[href*='facebook.com/groups']");
            _logger.LogInfo("Group links selector found for selecting all groups.");

            var anchors = await page.QuerySelectorAllAsync("a[href*='facebook.com/groups']");
            _logger.LogInfo($"Found {anchors.Count} group links in total.");

            List<string> allhrefs = new List<string>();

            foreach (var anchor in anchors)
            {
                var href = await anchor.GetAttributeAsync("href");
                var text = await anchor.InnerTextAsync();

                if (!string.IsNullOrEmpty(href))
                {
                    allhrefs.Add(href);
                    _logger.LogInfo($"Added group link: '{text}' -> '{href}'");
                }
            }
            _logger.LogInfo($"Collected {allhrefs.Count} unique group URLs.");
            return allhrefs;
        }

        public async Task<string> AuthorProfileUrlExchangedUrlAsync(IPage page, string partialUrl)
        {
            _logger.LogInfo($"Resolving author profile URL for partial URL: '{partialUrl}'");
            var newPage = await page.Context.NewPageAsync();

            try
            {
                string baseUrl = "https://www.facebook.com";
                await newPage.GotoAsync(baseUrl + partialUrl, new PageGotoOptions
                {
                    WaitUntil = WaitUntilState.DOMContentLoaded,
                    Timeout = 15000
                });
                _logger.LogInfo($"Navigated to temporary page for URL resolution: '{baseUrl + partialUrl}'");

                await newPage.WaitForLoadStateAsync(LoadState.DOMContentLoaded);

                string finalUrl = newPage.Url;
                _logger.LogInfo($"Resolved profile URL from '{baseUrl + partialUrl}' to: '{finalUrl}'");
                return finalUrl;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in AuthorProfileUrlExchangedUrlAsync while resolving partial URL: '{partialUrl}'. Exception: {ex.Message}");
                return null;
            }
            finally
            {
                await newPage.CloseAsync();
                _logger.LogInfo("Temporary page for URL resolution closed.");
            }
        }

        public async Task<string> GetPostDescriptionAsync(IPage page, string profileUrl)
        {
            if (string.IsNullOrWhiteSpace(profileUrl))
            {
                _logger.LogWarning("Profile URL is null or empty, cannot get post description.");
                return string.Empty;
            }

            _logger.LogInfo($"Attempting to get post description from profile URL: '{profileUrl}'");
            var newPage = await page.Context.NewPageAsync();

            try
            {
                await newPage.GotoAsync(profileUrl, new PageGotoOptions
                {
                    WaitUntil = WaitUntilState.DOMContentLoaded,
                    Timeout = 30000
                });
                _logger.LogInfo($"Navigated to profile/group page to extract post description: '{profileUrl}'");

                string html = await newPage.ContentAsync();

                var doc = new HtmlAgilityPack.HtmlDocument();
                doc.LoadHtml(html);
                string text = doc.DocumentNode.InnerText;

                text = Regex.Replace(text, @"\r\n|\r|\n", "\n");
                text = Regex.Replace(text, @"\n+", "\n").Trim();

                text = text.Replace("FacebookFacebook", "");
                _logger.LogInfo($"Successfully extracted and normalized post description from {profileUrl} (first 100 chars): {text.Substring(0, Math.Min(text.Length, 100))}");
                return text;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in GetPostDescriptionAsync while processing profile URL: '{profileUrl}'. Exception: {ex.Message}");
                return string.Empty;
            }
            finally
            {
                await newPage.CloseAsync();
                _logger.LogInfo("Temporary page for post description extraction closed.");
            }
        }
    }
}