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
    public class AirbnbService : IAirbnbService
    {
        private const string AirbnbBaseUrl = "https://www.airbnb.com";
        private static readonly Regex RoomIdRegex = new Regex(@"/rooms/(\d+)", RegexOptions.Compiled);

        private readonly ApplicationDbContext _context;
        private readonly ICookieService _cookieService;
        private readonly IUiLogger _logger;

        public AirbnbService(ApplicationDbContext context, ICookieService cookieService, IUiLogger logger)
        {
            _context = context;
            _cookieService = cookieService;
            _logger = logger;
        }

        public async Task SearchAsync(SearchDto searchDto)
        {
            if (string.IsNullOrWhiteSpace(searchDto.Keyword))
            {
                _logger.LogWarning("Search keyword is empty or null, skipping Airbnb search operation.");
                return;
            }

            IPage page = null;
            IBrowser browser = null;

            try
            {
                _logger.LogInfo($"Attempting to launch browser and navigate to Airbnb for keyword: '{searchDto.Keyword}'");

                // 1) Launch browser page (no proxy — user provides their own VPN / connection)
                (page, browser) = await _cookieService.LaunchPageAsync(searchDto.PrivateMode);
                _logger.LogInfo($"Successfully launched browser page for Airbnb (no proxy).");

                // 2) Source URL: airbnb.com
                await page.GotoAsync(AirbnbBaseUrl + "/", new PageGotoOptions
                {
                    WaitUntil = WaitUntilState.DOMContentLoaded,
                    Timeout = 90000
                });
                _logger.LogInfo($"Navigated to source URL: {AirbnbBaseUrl}");

                // 3) + 4) Fill the search input and submit
                page = await GoToSearchPageAsync(page, searchDto);
                _logger.LogInfo($"Submitted Airbnb search for keyword: '{searchDto.Keyword}'");

                // 5) Wait for 10 seconds for results to load
                _logger.LogInfo("Waiting 10 seconds for Airbnb search results to load.");
                await Task.Delay(TimeSpan.FromSeconds(10));

                // Shared set to de-duplicate properties across the whole run (all pages)
                var seenRoomIds = new HashSet<string>();

                // 6) Per-page extraction:
                //    1) extract exactly 10 properties on the page
                //    2) auto-scroll to 100% (the very bottom) to reveal the "Next" button
                //    3) wait for the "Next" link to appear
                //    4) click it
                //    repeat for the number of requested pages (PageNumber)
                int pagesToScrape = Math.Max(1, searchDto.PageNumber);
                int extractedLeads = 0;

                await Task.Delay(TimeSpan.FromSeconds(60));

                for (int pageIndex = 0; pageIndex < pagesToScrape; pageIndex++)
                {
                    _logger.LogInfo($"Processing page {pageIndex + 1} of {pagesToScrape}.");

                    // Ensure the results for this page have rendered before extracting
                    try
                    {
                        await page.Locator("a[href*='/rooms/']").First.WaitForAsync(new LocatorWaitForOptions
                        {
                            State = WaitForSelectorState.Visible,
                            Timeout = 30000
                        });
                    }
                    catch (TimeoutException)
                    {
                        _logger.LogWarning($"No property links found on page {pageIndex + 1}. Stopping.");
                        break;
                    }

                    // 1) Extract exactly 10 properties on this page (up to 10 unique new leads)
                    int pageLeads = await ExtractTenPropertiesAsync(page, searchDto, seenRoomIds);
                    extractedLeads += pageLeads;
                    _logger.LogInfo($"Page {pageIndex + 1} yielded {pageLeads} property lead(s). Running total: {extractedLeads}");

                    // If this is the last requested page, stop (nothing left to advance to)
                    if (pageIndex >= pagesToScrape - 1)
                        break;

                    // 2) Auto-scroll down through the page to reveal the "Next" button.
                    //    Airbnb lazily loads more listings as you scroll, so we keep scrolling
                    //    to the bottom until either the "Next" link appears or the page height
                    //    stops growing (meaning no more content is being loaded).
                    _logger.LogInfo($"Auto-scrolling page {pageIndex + 1} to reveal the 'Next' button.");

                    bool nextFound = false;
                    var nextButton = page.Locator("a[aria-label='Next']").First;

                    // Check upfront in case the link is already present in the DOM
                    if (await nextButton.CountAsync() > 0)
                    {
                        nextFound = true;
                    }

                    if (!nextFound)
                    {
                        for (int scrollStep = 0; scrollStep < 60 && !nextFound; scrollStep++)
                        {
                            // Re-check for the "Next" link after each scroll
                            if (await nextButton.CountAsync() > 0)
                            {
                                nextFound = true;
                                break;
                            }

                            // Record the page height before scrolling
                            var previousHeight = await page.EvaluateAsync<long>(
                                "document.body.scrollHeight");

                            // Scroll straight to the very bottom of the page
                            await page.EvaluateAsync(
                                "window.scrollTo(0, document.body.scrollHeight)");

                            // Give the page time to lazy-load more listings
                            await page.WaitForTimeoutAsync(1500);

                            // Read the height again after scrolling
                            var newHeight = await page.EvaluateAsync<long>(
                                "document.body.scrollHeight");

                            // If the page height didn't grow, there's no more content to load
                            if (newHeight <= previousHeight)
                            {
                                _logger.LogInfo("Page height stopped growing; no more content to load.");
                                break;
                            }
                        }
                    }

                    if (!nextFound)
                    {
                        _logger.LogWarning("'Next' pagination link not found after scrolling. Stopping pagination.");
                        break;
                    }

                    // 3) Wait for the "Next" pagination link to become visible
                    await nextButton.WaitForAsync(new LocatorWaitForOptions
                    {
                        State = WaitForSelectorState.Visible,
                        Timeout = 15000
                    });

                    // 4) Click the "Next" link to load the next page of results
                    _logger.LogInfo($"Clicking 'Next' to advance to page {pageIndex + 2}.");
                    await nextButton.ClickAsync();

                    // Wait for the next page of results to be fetched and rendered
                    _logger.LogInfo($"Waiting 10 seconds for page {pageIndex + 2} results to load.");
                    await Task.Delay(TimeSpan.FromSeconds(10));
                }

                _logger.LogInfo($"Extraction complete. Total {extractedLeads} property lead(s) processed for keyword: '{searchDto.Keyword}'");
            }
            catch (Exception ex)
            {
                _logger.LogError($"An unhandled error occurred during Airbnb search for keyword: '{searchDto.Keyword}'. Exception: {ex.Message}");
                throw;
            }
            finally
            {
                if (browser != null)
                {
                    await browser.CloseAsync();
                    _logger.LogInfo("Browser closed in finally block after Airbnb search.");
                }
            }
        }

        public async Task<IPage> GoToSearchPageAsync(IPage page, SearchDto searchDto)
        {
            // Dismiss any modal/overlay (cookie banner, region/login popup, etc.) that could
            // cover the search UI and intercept clicks.
            try
            {
                await page.Keyboard.PressAsync("Escape");
                await page.WaitForTimeoutAsync(300);
            }
            catch { }

            try
            {
                var modalClose = page.Locator(
                    "[data-testid='modal-container'] button[aria-label='Close'], " +
                    "[data-testid='modal-header'] button, " +
                    "button[aria-label='Close']").First;
                if (await modalClose.CountAsync() > 0 && await modalClose.IsVisibleAsync())
                {
                    await modalClose.ClickAsync(new LocatorClickOptions { Timeout = 3000 });
                    _logger.LogInfo("Dismissed a modal/overlay on the Airbnb page.");
                    await page.WaitForTimeoutAsync(500);
                }
            }
            catch
            {
                // Best-effort modal dismissal — continue even if none exists.
            }

            // 3) Input field for searching
            var searchInput = page.Locator("#bigsearch-query-location-input, [data-testid='structured-search-input-field-query']").First;
            await searchInput.WaitForAsync(new LocatorWaitForOptions
            {
                State = WaitForSelectorState.Visible,
                Timeout = 30000
            });
            _logger.LogInfo("Airbnb search input field located.");

            // Force the click so any lingering overlay doesn't block interacting with the field.
            await searchInput.ClickAsync(new LocatorClickOptions { Force = true });
            await searchInput.FillAsync(searchDto.Keyword);
            _logger.LogInfo($"Filled Airbnb search input with: '{searchDto.Keyword}'");

            // Small delay to let the autocomplete dropdown settle before submitting
            await page.WaitForTimeoutAsync(500);

            // 4) Click the real "Search" submit button — this is what actually triggers
            //    the search navigation to the results page (pressing Enter alone is unreliable).
            var searchButton = page.Locator("button[data-testid='structured-search-input-search-button']").First;
            await searchButton.WaitForAsync(new LocatorWaitForOptions
            {
                State = WaitForSelectorState.Visible,
                Timeout = 10000
            });
            await searchButton.ClickAsync(new LocatorClickOptions { Force = true });
            _logger.LogInfo("Clicked the 'Search' submit button.");

            // Wait for navigation to the search results page
            try
            {
                await page.WaitForLoadStateAsync(LoadState.DOMContentLoaded, new PageWaitForLoadStateOptions { Timeout = 30000 });
            }
            catch (TimeoutException)
            {
                _logger.LogWarning("Timeout waiting for search navigation; continuing.");
            }

            return page;
        }

        private async Task<int> ExtractTenPropertiesAsync(IPage page, SearchDto searchDto, HashSet<string> seenRoomIds)
        {
            const int propertiesPerPage = 10;

            var roomAnchors = await page.Locator("a[href*='/rooms/']").AllAsync();
            _logger.LogInfo($"Found {roomAnchors.Count} room link element(s) loaded on this page.");

            int savedCount = 0;

            foreach (var anchor in roomAnchors)
            {
                if (savedCount >= propertiesPerPage)
                    break; // Exactly 10 properties per page

                try
                {
                    string? href = await anchor.GetAttributeAsync("href");
                    if (string.IsNullOrWhiteSpace(href))
                        continue;

                    var match = RoomIdRegex.Match(href);
                    if (!match.Success)
                        continue;

                    string roomId = match.Groups[1].Value;
                    if (!seenRoomIds.Add(roomId))
                        continue; // Already captured this property on a previous page

                    string absoluteUrl = href.StartsWith("http", StringComparison.OrdinalIgnoreCase)
                        ? href
                        : AirbnbBaseUrl + href;

                    // Open the listing in a new tab and extract the host's name (not the property title)
                    string hostName = await GetHostNameAsync(page, absoluteUrl, roomId);

                    var lead = new Leads
                    {
                        Name = hostName,
                        ProfileUrl = absoluteUrl,
                        PostUrl = absoluteUrl,
                        PostDescription = searchDto.Keyword,
                        Platform = "Airbnb",
                        Keywords = searchDto.Keyword,
                        Query = searchDto.Query,
                        Status = "New",
                        PostDate = DateTime.UtcNow,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now
                    };

                    await _context.Leads.AddAsync(lead);
                    await _context.SaveChangesAsync();
                    savedCount++;

                    _logger.LogInfo($"Saved Airbnb lead (host '{hostName}', room {roomId}): {savedCount}/{propertiesPerPage}");
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error saving Airbnb property lead. Exception: {ex.Message}");
                }
            }

            _logger.LogInfo($"Extracted {savedCount} property lead(s) on this page.");
            return savedCount;
        }

        /// <summary>
        /// Opens a single Airbnb listing in a new tab, reads the host's name from the
        /// "Hosted by &lt;Name&gt;" element, strips the "Hosted by" prefix, then closes the tab.
        /// </summary>
        private async Task<string> GetHostNameAsync(IPage sourcePage, string listingUrl, string roomId)
        {
            IPage tab = null;
            try
            {
                // Open the listing in a new tab (same browser context/session)
                tab = await sourcePage.Context.NewPageAsync();

                await tab.GotoAsync(listingUrl, new PageGotoOptions
                {
                    WaitUntil = WaitUntilState.DOMContentLoaded,
                    Timeout = 90000
                });

                await Task.Delay(TimeSpan.FromSeconds(30));

                // Locate the "Hosted by <Name>" element on the listing page
                var hostElement = tab.Locator("div.t1avz7ro").First;
                await hostElement.WaitForAsync(new LocatorWaitForOptions
                {
                    State = WaitForSelectorState.Visible,
                    Timeout = 20000
                });

                string? hostText = await hostElement.TextContentAsync();
                if (!string.IsNullOrWhiteSpace(hostText))
                {
                    string cleaned = hostText.Trim();
                    // Remove the leading "Hosted by" (case-insensitive)
                    if (cleaned.StartsWith("Hosted by", StringComparison.OrdinalIgnoreCase))
                        cleaned = cleaned.Substring("Hosted by".Length).Trim();

                    if (!string.IsNullOrWhiteSpace(cleaned))
                        return cleaned;
                }

                _logger.LogWarning($"Could not extract host name for room {roomId}. Using fallback.");
                return $"Airbnb Host {roomId}";
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Error opening listing {roomId} to extract host name: {ex.Message}");
                return $"Airbnb Host {roomId}";
            }
            finally
            {
                if (tab != null)
                {
                    await tab.CloseAsync();
                    _logger.LogInfo($"Closed tab for listing {roomId}.");
                }
            }
        }

        public async Task<IPage> DirectMessagingAsync(IPage page, MessengerDto messengerDto)
        {
            _logger.LogInfo($"Navigating to Airbnb property for messaging: {messengerDto.Lead.ProfileUrl}");

            // Navigate to the property listing page
            await page.GotoAsync(messengerDto.Lead.ProfileUrl, new PageGotoOptions
            {
                WaitUntil = WaitUntilState.DOMContentLoaded,
                Timeout = 30000
            });
            _logger.LogInfo("Navigated to Airbnb property listing.");
           
            await Task.Delay(TimeSpan.FromSeconds(30));

            // 7) Message Host Button - click the "Message host" link
            var messageHostButton = page.Locator("span[data-button-content='true']", new() { HasText = "Message host" }).First;
           
            await messageHostButton.WaitForAsync(new LocatorWaitForOptions
            {
                State = WaitForSelectorState.Visible,
                Timeout = 15000
            });
            await messageHostButton.ClickAsync();
            _logger.LogInfo("Clicked 'Message host' button.");

            // 8) Wait 10 seconds for the messaging form to load
            _logger.LogInfo("Waiting 10 seconds for Airbnb contact-host messaging form to load.");
            await Task.Delay(TimeSpan.FromSeconds(10));

            // 9) Textarea for the message
            var messageTextarea = page.Locator("#contactHostMessage");
            await messageTextarea.WaitForAsync(new LocatorWaitForOptions
            {
                State = WaitForSelectorState.Visible,
                Timeout = 15000
            });
            _logger.LogInfo("Airbnb message textarea located.");

            await messageTextarea.ClickAsync();
            await messageTextarea.FillAsync(messengerDto.Text);
            _logger.LogInfo($"Filled Airbnb message textarea (first 50 chars): '{messengerDto.Text.Substring(0, Math.Min(messengerDto.Text.Length, 50))}...'");

            // 10) Submit button to submit the message
            var sendButton = page.Locator("button[data-testid='send-message-button']");
            if (await sendButton.CountAsync() == 0)
            {
                _logger.LogWarning($"Send message button not found for property: {messengerDto.Lead.ProfileUrl}");
                return page;
            }

            await sendButton.ClickAsync();
            _logger.LogInfo("Clicked 'Send message' button.");

            // Wait for the message send to complete
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle, new PageWaitForLoadStateOptions { Timeout = 10000 });
            _logger.LogInfo("Airbnb message sent, waiting for network idle.");

            await UpdateLead(messengerDto.Lead);

            return page;
        }

        private async Task UpdateLead(Leads lead)
        {
            var query = await _context.Leads.Where(x => x.Name == lead.Name && x.Platform == "Airbnb").ToListAsync();

            foreach (var item in query)
            {
                item.Status = "Contacted";
                _context.Leads.Update(item);
                await _context.SaveChangesAsync();
                _logger.LogInfo($"Airbnb lead '{item.Name}' status updated to 'Contacted' in the database.");
            }
        }
    }
}


