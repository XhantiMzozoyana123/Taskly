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

        public FacebookService(ApplicationDbContext context, IAiService aiService, ICookieService cookieService)
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
                // Login to Facebook
                (page, browser) = await _cookieService.LoadCookieOnPageAsync(searchDto.CookiePath, searchDto.PrivateMode);

                // Navigate to Facebook Groups search page
                page = await GoToFacebookGroupPage(page, searchDto);

                // Get all Facebook groups from the search results (for logging/debugging)
                var facebookGroupList = await SelectAllFacebookFacebookGroups(page, searchDto);

                foreach (var groupUrl in facebookGroupList)
                {
                    try
                    {
                        // Navigate to each group URL one by one
                        await page.GotoAsync(groupUrl, new PageGotoOptions
                        {
                            WaitUntil = WaitUntilState.DOMContentLoaded
                        });
                        // Wait for the main content to load
                        //await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

                        // Wait for the first post container to appear using the specific classes provided
                        string mainPostContainerSelector = "div.html-div.xdj266r.x14z9mp.xat24cr.x1lziwak.xexx8yu.xyri2b.x18d9i69.x1c1uobl:has(h3[id*='_r_'])";
                        var firstPostLocator = page.Locator(mainPostContainerSelector).First;
                        //await firstPostLocator.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible, Timeout = 15000 });

                        var scrapedPostPermalinks = new HashSet<string>(); // Track already scraped post URLs to avoid duplicates

                        // Loop for scrolling and loading multiple pages of posts
                        for (int i = 0; i < searchDto.PageNumber; i++)
                        {
                            // Get all post containers currently loaded on the page
                            var postLocators = await page.Locator(mainPostContainerSelector).AllAsync();

                            foreach (var currentPostLocator in postLocators)
                            {
                                // --- 1. Extract Post URL (Permalink) first for unique tracking ---
                                string? postPermalink = null;
                                // The primary link for the post content itself, often containing 'fbid'
                                var postContentLinkLocator = currentPostLocator.Locator("a[attributionsrc][href*='fbid']").First;
                                if (await postContentLinkLocator.IsVisibleAsync())
                                {
                                    postPermalink = await postContentLinkLocator.GetAttributeAsync("href");
                                }

                                // Use the post permalink as the unique identifier
                                if (string.IsNullOrWhiteSpace(postPermalink) || scrapedPostPermalinks.Contains(postPermalink))
                                    continue; // Skip if URL is invalid or already scraped

                                scrapedPostPermalinks.Add(postPermalink); // Add to set

                                // Initialize other data points for the current post
                                string? authorName = null;
                                string? authorProfileUrl = null;
                                string? postText = null;
                                DateTime? publishedDate = null; // Will fallback to UtcNow if not found

                                // --- 2. Extract Author's Name and Profile URL ---
                                // The author's name is inside a <b> tag within a span that is a child of the main author link.
                                var authorNameLocator = currentPostLocator.Locator("h3[id*='_r_'] a span.html-span").First;

                                if (await authorNameLocator.IsVisibleAsync())
                                {
                                    authorName = await authorNameLocator.InnerTextAsync();
                                }

                                // The profile URL is the href of the main 'a' tag in the author's h3 block.
                                var authorProfileLinkLocator = currentPostLocator.Locator("h3[id*='_r_'] a[attributionsrc]").First;
                                if (await authorProfileLinkLocator.IsVisibleAsync())
                                {
                                    string rawProfileUrl = await authorProfileLinkLocator.GetAttributeAsync("href");
                                    authorProfileUrl = await AuthorProfileUrlExchangedUrlAsync(page, rawProfileUrl);
                                }

                                // --- 3. Extract Post Text ---
                                // This is typically within div[data-ad-rendering-role="story_message"] or similar, potentially with nested spans.
                                postText = await GetPostDescriptionAsync(page, authorProfileUrl);// Normalize line breaks

                                // --- 4. Extract Published Datetime ---
                                // Based on the provided HTML, there isn't a direct <time datetime="..."> for the main post.
                                // Facebook dynamically renders relative times. For this implementation, we will use UtcNow
                                // as a fallback for PublishedAt. A more advanced solution would parse relative times
                                // from aria-label attributes if they contained full timestamps.
                                publishedDate = DateTime.UtcNow; // Fallback

                                // Only process and save if core data exists
                                if (!string.IsNullOrWhiteSpace(authorName) || !string.IsNullOrWhiteSpace(postText))
                                {
                                    var existingLead = await _context.Leads.FirstOrDefaultAsync(l =>
                                               l.Name == authorName);

                                    if (existingLead == null)
                                    {
                                        // Use AI service to check if the content is relevant
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
                                            }
                                            catch (Exception ex)
                                            {
                                                continue; // Skip saving this lead on error
                                            }
                                        }
                                    }
                                }
                            }

                            // Scrolling strategy: Scroll down to load more content
                            await page.EvaluateAsync(@"window.scrollBy(0, window.innerHeight);");
                            await Task.Delay(2000); // Wait for new posts to load after scrolling.

                            // Optional: Add a check here to see if scrolling actually loaded new content
                            // (e.g., compare scrapedPostPermalinks.Count before and after scroll. If no new posts, break.)
                        }
                    }
                    catch (Exception ex)
                    {
                        continue; // Skip to the next group URL on error
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

        public async Task<IPage> GoToFacebookGroupPage(IPage page, SearchDto searchDto)
        {
            // Go to the groups explore page
            await page.GotoAsync("https://www.facebook.com/groups/feed", new PageGotoOptions
            {
                WaitUntil = WaitUntilState.DOMContentLoaded
            });

            // Locate the input by aria-label or placeholder (more stable)
            var searchInput = page.Locator("input[aria-label='Search groups'], input[placeholder='Search groups']");
            await searchInput.WaitForAsync();

            // Fill the search text
            await searchInput.FillAsync(searchDto.Keyword);

            // Press Enter
            await searchInput.PressAsync("Enter");

            // Wait for search results to appear
            //await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            await Task.Delay(5000); // Wait for results to load

            return page;
        }

        public async Task<IPage> DirectMessagingAsync(IPage page, MessengerDto messengerDto)
        {
            // Navigate to the user's Facebook profile
            await page.GotoAsync(messengerDto.Lead.ProfileUrl, new PageGotoOptions
            {
                WaitUntil = WaitUntilState.DOMContentLoaded,
            });

            await Task.Delay(5000);

            // Wait for and click the "Message" button
            var messageButton = page.Locator("span:has-text('Message')");
            if (await messageButton.CountAsync() == 0)
            {
                return page; // Skip profiles with DMs disabled
            }

            await Task.Delay(5000);

            await messageButton.First.ClickAsync();

            // Wait for the Lexical DM input to appear
            var dmInput = page.Locator("div[contenteditable='true'][role='textbox'][data-lexical-editor='true'][aria-placeholder='Aa']");
            await dmInput.WaitForAsync(new LocatorWaitForOptions
            {
                State = WaitForSelectorState.Visible,
            });

            await Task.Delay(5000);

            // Focus and type the message
            await dmInput.ClickAsync();
            await page.Keyboard.TypeAsync(messengerDto.Text);

            // Press Enter to send
            await page.Keyboard.PressAsync("Enter");

            await Task.Delay(5000);

            // Wait for network idle to ensure message is sent
            // await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            return page;
        }

        public async Task<IPage> SelectRandomFacebookGroup(IPage page, SearchDto searchDto)
        {
            // Wait for anchor tags to appear (adjust selector if needed)
            await page.WaitForSelectorAsync("a[href*='facebook.com/groups']");

            // Get all matching anchors
            var anchors = await page.QuerySelectorAllAsync("a[href*='facebook.com/groups']");

            Console.WriteLine($"Found {anchors.Count} group links:\n");

            // Loop through and extract hrefs
            foreach (var anchor in anchors)
            {
                var href = await anchor.GetAttributeAsync("href");
                var text = await anchor.InnerTextAsync();

                // Filter or print
                if (!string.IsNullOrEmpty(href))
                {
                    Console.WriteLine($"{text} -> {href}");
                }
            }
            // Select a random anchor from the filtered list
            var random = new Random();
            var randomAnchor = anchors[random.Next(anchors.Count)];
            var randomHref = await randomAnchor.GetAttributeAsync("href");

            if (!string.IsNullOrWhiteSpace(randomHref))
            {
                // Navigate to the selected group's URL
                await page.GotoAsync(randomHref, new PageGotoOptions
                {
                    WaitUntil = WaitUntilState.DOMContentLoaded,
                    Timeout = 30000
                });
            }

            return page;
        }

        public async Task<List<string>> SelectAllFacebookFacebookGroups(IPage page, SearchDto searchDto)
        {
            // Wait for anchor tags to appear (adjust selector if needed)
            await page.WaitForSelectorAsync("a[href*='facebook.com/groups']");

            // Get all matching anchors
            var anchors = await page.QuerySelectorAllAsync("a[href*='facebook.com/groups']");

            Console.WriteLine($"Found {anchors.Count} group links:\n");

            List<string> allhrefs = new List<string>();

            // Loop through and extract hrefs
            foreach (var anchor in anchors)
            {
                var href = await anchor.GetAttributeAsync("href");
                var text = await anchor.InnerTextAsync();

                // Filter or print
                if (!string.IsNullOrEmpty(href))
                {
                    allhrefs.Add(href);
                }
            }

            return allhrefs;
        }

        public async Task<string> AuthorProfileUrlExchangedUrlAsync(IPage page, string partialUrl)
        {
            // Create a new tab (page)
            var newPage = await page.Context.NewPageAsync();

            try
            {
                string baseUrl = "https://www.facebook.com";
                // Go to the provided partial or relative URL
                await newPage.GotoAsync(baseUrl + partialUrl, new PageGotoOptions
                {
                    WaitUntil = WaitUntilState.DOMContentLoaded,
                    Timeout = 15000
                });

                // Wait for navigation to settle
                await newPage.WaitForLoadStateAsync(LoadState.DOMContentLoaded);

                // Grab the final, resolved URL (Facebook may redirect it)
                string finalUrl = newPage.Url;

                return finalUrl;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[AuthorProfileUrlExchangedUrlAsync] Error: {ex.Message}");
                return null;
            }
            finally
            {
                // Close the tab to free resources
                await newPage.CloseAsync();
            }
        }

        public async Task<string> GetPostDescriptionAsync(IPage page, string profileUrl)
        {
            // Open a new tab
            var newPage = await page.Context.NewPageAsync();

            try
            {
                // Navigate to the profile/group page
                await newPage.GotoAsync(profileUrl, new PageGotoOptions
                {
                    WaitUntil = WaitUntilState.DOMContentLoaded,
                    Timeout = 30000
                });

                // Get the full page HTML
                string html = await newPage.ContentAsync();

                // Convert HTML to text
                var doc = new HtmlAgilityPack.HtmlDocument();
                doc.LoadHtml(html);
                string text = doc.DocumentNode.InnerText;

                // Normalize whitespace and line breaks
                text = Regex.Replace(text, @"\r\n|\r|\n", "\n");
                text = Regex.Replace(text, @"\n+", "\n").Trim();

                text = text.Replace("FacebookFacebook", "");

                return text;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[GetPostDescriptionAsync] Error: {ex.Message}");
                return string.Empty;
            }
            finally
            {
                await newPage.CloseAsync();
            }
        }
    }
} 