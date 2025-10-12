// File: Taskly.Infrastructure.Services/RedditService.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.Playwright;
using System;
using System.Collections.Generic; // Added for List
using System.Linq;
using System.Threading.Tasks;
using Taskly.Application.Constants;
using Taskly.Application.Dtos;
using Taskly.Application.Interfaces;
using Taskly.Domain;
using Taskly.Domain.Entities; 

namespace Taskly.Infrastructure.Services
{
    public class RedditService : IRedditService
    {
        private readonly ApplicationDbContext _context;
        private readonly IAiService _aiService;

        public RedditService(ApplicationDbContext context, IAiService aiService) // Constructor for dependency injection
        {
            _context = context;
            _aiService = aiService;
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
                page = await LoginAsync(page, searchDto);
                page = await FindSubredditsUrl(page, searchDto);

                // Wait for first comment to appear
                await page.Locator("shreddit-comment").First.WaitForAsync(new LocatorWaitForOptions
                {
                    State = WaitForSelectorState.Visible,
                    Timeout = 10000
                });

                var scrapedCommentIds = new HashSet<string>(); // Track already scraped comments
                
                for (int i = 0; i < searchDto.PageNumber; i++)
                {
                    // Get all comments currently loaded
                    var commentLocators = await page.Locator("shreddit-comment").AllAsync();

                    foreach (var commentLocator in commentLocators)
                    {
                        // Use a unique identifier to avoid duplicates
                        var commentId = await commentLocator.GetAttributeAsync("id");
                        if (!string.IsNullOrWhiteSpace(commentId) && scrapedCommentIds.Contains(commentId))
                            continue; // Already scraped

                        scrapedCommentIds.Add(commentId);

                        string? username = await commentLocator.GetAttributeAsync("author");
                        string? userProfileUrl = null;
                        string? authorsPostingText = null;
                        string? shareUrl = null;
                        DateTime? postedDateTime = null;

                        // Extract user profile URL
                        var userProfileLinkLocator = commentLocator.Locator("a[href^='/user/']").First;
                        if (await userProfileLinkLocator.IsVisibleAsync())
                        {
                            var userProfileRelativeUrl = await userProfileLinkLocator.GetAttributeAsync("href");
                            if (!string.IsNullOrWhiteSpace(userProfileRelativeUrl))
                                userProfileUrl = new Uri(new Uri(searchDto.Url), userProfileRelativeUrl).AbsoluteUri;
                        }

                        // Extract comment text
                        var authorsPostingTextLocator = commentLocator.Locator("div[slot='comment'] p").First;
                        if (await authorsPostingTextLocator.IsVisibleAsync())
                            authorsPostingText = await authorsPostingTextLocator.InnerTextAsync();

                        // Extract comment permalink
                        shareUrl = await commentLocator.GetAttributeAsync("permalink");
                        if (!string.IsNullOrWhiteSpace(shareUrl))
                            shareUrl = new Uri(new Uri(searchDto.Url), shareUrl).AbsoluteUri;

                        // Extract posted datetime
                        var timeElementLocator = commentLocator.Locator("time").First;
                        if (await timeElementLocator.IsVisibleAsync())
                        {
                            var dateTimeAttribute = await timeElementLocator.GetAttributeAsync("datetime");
                            if (!string.IsNullOrWhiteSpace(dateTimeAttribute) &&
                                DateTime.TryParse(dateTimeAttribute, out DateTime parsedDateTime))
                                postedDateTime = parsedDateTime;
                        }

                        // Only process if core data exists
                        if (!string.IsNullOrWhiteSpace(username) || !string.IsNullOrWhiteSpace(authorsPostingText))
                        {
                            var isRelevant = await _aiService.CheckIfContentIsRelevantAsync(authorsPostingText, searchDto.Query);
                            if (isRelevant)
                            {
                                var lead = new Leads()
                                {
                                    Name = username,
                                    ProfileUrl = userProfileUrl,
                                    Status = "New",
                                    Platform = "Reddit",
                                    PostDescription = authorsPostingText,
                                    PostUrl = shareUrl,
                                    Keywords = searchDto.Keyword,
                                    Query = searchDto.Query,
                                    PostDate = postedDateTime.Value
                                };

                                await _context.Leads.AddAsync(lead);
                                await _context.SaveChangesAsync();
                            }
                        }
                    }

                    // Scroll to load more comments
                    await page.EvaluateAsync(@"window.scrollBy(0, window.innerHeight * 3);");
                    await Task.Delay(1500); // Wait for new posts to load
                }
            }
            catch (Exception ex)
            {
                throw;
            }
            finally
            {
                await browser.CloseAsync();
            }
        }

        public async Task<IPage> LoginAsync(IPage page, SearchDto searchDto)
        {
            var socialLogin = await _context.SocialLogins.FirstAsync(x =>
                   x.UserId == searchDto.UserId &&
                   x.Platform == "Reddit");

            var userName = TokenEncryptor.Decrypt(socialLogin.UsernameHash);
            var passWord = TokenEncryptor.Decrypt(socialLogin.PasswordHash);

            // --- START LOGIN SEQUENCE ---
            await page.GotoAsync("https://www.reddit.com/login/", new PageGotoOptions { WaitUntil = WaitUntilState.DOMContentLoaded });

            // Fill username
            await page.Locator("input[name='username']").FillAsync(userName);

            // Fill password
            await page.Locator("input[name='password']").FillAsync(passWord);

            // Click login button
            await page.Locator("button.login.oidc").ClickAsync();

            // Wait for navigation after login. This could be waiting for network idle,
            // or waiting for a specific element that appears on the authenticated homepage.
            // For simplicity, we'll wait for network to be idle.
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
            // --- END LOGIN SEQUENCE ---
            
            await page.GotoAsync(searchDto.Url, new PageGotoOptions { WaitUntil = WaitUntilState.DOMContentLoaded });

            return page;
        }

        public async Task ScrapeSocialLinks(IPage page, Leads lead, string profileUrl)
        {
            // Extract the Reddit username from the URL for the SocialMediaLink entity
            var uri = new Uri(profileUrl);
            var redditUsername = uri.Segments.LastOrDefault(s => !string.IsNullOrWhiteSpace(s) && !s.EndsWith("/"))?.TrimEnd('/');

            // Wait for the container of social links to be visible
            var socialLinksContainerLocator = page.Locator("div.flex.gap-xs.flex-wrap").First;
            await socialLinksContainerLocator.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible, Timeout = 10000 });

            // Find all anchor tags for social links within the container
            var socialLinkLocators = await socialLinksContainerLocator.Locator("a[rpl][aria-label^='Visit ']").AllAsync();

            foreach (var linkLocator in socialLinkLocators)
            {
                string? href = await linkLocator.GetAttributeAsync("href");
                string? ariaLabel = await linkLocator.GetAttributeAsync("aria-label");
                string? displayName = await linkLocator.Locator("span.flex.items-center.gap-x-xs").First.InnerTextAsync(); // Text next to the image

                string? platform = null;
                if (!string.IsNullOrWhiteSpace(ariaLabel))
                {
                    // Extract platform from aria-label, e.g., "Visit MLB on TWITTER" -> "TWITTER"
                    var parts = ariaLabel.Split(new[] { " on " }, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length > 1)
                    {
                        platform = parts.Last().ToUpper();
                    }
                }

                if (!string.IsNullOrWhiteSpace(href) && !string.IsNullOrWhiteSpace(platform))
                {
                    var externalLinks = new ExternalLinks
                    {
                        UserId = lead.UserId,
                        LeadId = lead.Id,
                        Url = href,
                    };

                    await _context.ExternalLinks.AddAsync(externalLinks);
                    await _context.SaveChangesAsync();
                }
            }
        }

        public async Task<IPage> FindSubredditsUrl(IPage page, SearchDto searchDto)
        {
            // Wait for the shadow host to appear
            await page.WaitForSelectorAsync("faceplate-search-input");

            // Locate the inner input element inside the shadow root and fill text
            var searchInput = page.Locator("faceplate-search-input").Locator("input[type='text']");
            await searchInput.FillAsync(searchDto.Keyword);

            // Optional: press Enter to search
            await searchInput.PressAsync("Enter");

            // Wait for search results to load
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            return page;
        }
    }
}