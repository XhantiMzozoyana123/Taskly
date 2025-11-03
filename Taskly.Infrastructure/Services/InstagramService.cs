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

        public InstagramService(ApplicationDbContext context, IAiService aiService, ICookieService cookieService)
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
                // Login to Instagram
                (page, browser) = await _cookieService.LoadCookieOnPageAsync(searchDto.CookiePath, searchDto.PrivateMode);

                // Go to Expore Page to search for hashtags or keywords
                page = await GoToExplorePageAsync(page, searchDto);

                // Wait for at least one Instagram post link to be visible
                // The class `_a6hd` seems consistent in your HTML snippets for post links
                var postLinkLocator = page.Locator("a._a6hd").First;
                await postLinkLocator.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible, Timeout = 15000 });

                var scrapedPostUrls = new HashSet<string>(); // Track already scraped post URLs

                // Loop for scrolling and loading multiple pages of Instagram posts
                for (int i = 0; i < searchDto.PageNumber; i++)
                {
                    var instagramPostLocators = await page.Locator("a._a6hd").AllAsync();

                    foreach (var currentPostLocator in instagramPostLocators)
                    {
                        // Get the href (relative link) for the post
                        string? postUrl = await currentPostLocator.GetAttributeAsync("href");

                        // Convert it to an absolute URL
                        string? absolutePostUrl = !string.IsNullOrWhiteSpace(postUrl)
                            ? $"https://www.instagram.com{postUrl}"
                            : null;

                        // Skip if this post has already been scraped
                        if (string.IsNullOrWhiteSpace(absolutePostUrl) || scrapedPostUrls.Contains(absolutePostUrl))
                            continue;

                        // Add to the set to prevent duplicates
                        scrapedPostUrls.Add(absolutePostUrl);

                        string? imageUrl = null;
                        string? altText = null;

                        // Locate the image within the post
                        var imgLocator = currentPostLocator.Locator("img.x5yr21d").First;

                        if (await imgLocator.IsVisibleAsync())
                        {
                            imageUrl = await imgLocator.GetAttributeAsync("src");
                            altText = await imgLocator.GetAttributeAsync("alt");
                        }

                        // Only process and save if core data exists
                        if (!string.IsNullOrWhiteSpace(imageUrl))
                        {
                            // Use the absolute URL to get more info
                            var postDto = await GetAuthorPost(page, absolutePostUrl);

                            if (postDto != null)
                            {
                                // Use AI to check relevance
                                var isRelevant = await _aiService.CheckIfContentIsRelevantAsync(altText, searchDto.Query);

                                if (isRelevant)
                                {
                                    var lead = new Leads
                                    {
                                        Name = postDto.Author,
                                        ProfileUrl = postDto.ProfileUrl,
                                        Status = "New",
                                        Platform = "Instagram",
                                        PostDescription = postDto.Text,
                                        PostUrl = absolutePostUrl, // ✅ now using the full URL
                                        Keywords = searchDto.Keyword,
                                        Query = searchDto.Query,
                                        PostDate = postDto.PublishedDate
                                    };

                                    await _context.Leads.AddAsync(lead);
                                    await _context.SaveChangesAsync();
                                }
                            }
                        }
                    }

                    // Scroll down to load more posts
                    await page.EvaluateAsync(@"window.scrollBy(0, window.innerHeight);");
                    await Task.Delay(2000);
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

        public async Task<IPage> GoToExplorePageAsync(IPage page, SearchDto searchDto)
        {
            // Go to Instagram Explore page
            await page.GotoAsync($"https://www.instagram.com/explore/search/keyword/?q={searchDto.Keyword.Replace(" ", "+")}", new PageGotoOptions
            {
                WaitUntil = WaitUntilState.DOMContentLoaded
            });

            return page;
        }

        public async Task<PostContentDto> GetAuthorPost(IPage page, string postUrl)
        {
            if (string.IsNullOrWhiteSpace(postUrl))
                return null;

            var newPage = await page.Context.NewPageAsync();
            var postContentDto = new PostContentDto();

            try
            {
                await newPage.GotoAsync(postUrl, new PageGotoOptions
                {
                    WaitUntil = WaitUntilState.DOMContentLoaded,
                    Timeout = 30000
                });

                // Wait for essential elements
                await newPage.Locator("time.xdwrcjd").WaitForAsync(new LocatorWaitForOptions
                {
                    State = WaitForSelectorState.Visible,
                    Timeout = 10000
                });

                // Wait for the link element to load
                await page.WaitForSelectorAsync("a.x1i10hfl.xjbqb8w.x1ejq31n");

                // Username
                string username = await newPage.GetAttributeAsync("a.x1i10hfl.xjbqb8w.x1ejq31n", "href");
                username = username.Replace("/", "").Trim();

                // Extract the caption text
                string caption = await newPage.InnerHTMLAsync("div.html-div");
                caption = AppConstants.ConvertHtmlToPlainText(caption);

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

                postContentDto.Author = username ?? string.Empty;
                postContentDto.Text = caption ?? string.Empty;
                postContentDto.PostUrl = postUrl;
                postContentDto.ProfileUrl = !string.IsNullOrWhiteSpace(username)
                    ? $"https://www.instagram.com/{username}/"
                    : string.Empty;
                postContentDto.PublishedDate = postDate ?? DateTime.UtcNow;

                return postContentDto;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[InstagramService.GetAuthorPost] Error: {ex.Message}");
                return null;
            }
            finally
            {
                await newPage.CloseAsync();
            }
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