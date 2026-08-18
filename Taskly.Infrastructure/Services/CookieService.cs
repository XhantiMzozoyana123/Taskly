using Microsoft.EntityFrameworkCore;
using Microsoft.Playwright;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Taskly.Application.Dtos;
using Taskly.Application.Interfaces;
using Taskly.Domain;

namespace Taskly.Infrastructure.Services
{
    /// <summary>
    /// Handles browser cookie operations using Playwright.
    /// Supports loading cookies into pre-authenticated sessions for automation,
    /// and identifying cookie domains. All operations run strictly on the local
    /// machine — no remote requests are made.
    /// </summary>
    public class CookieService : ICookieService
    {
        private readonly ApplicationDbContext _context;

        public CookieService(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Gets all local cookie file paths stored in the database.
        /// </summary>
        /// <returns>List of file names representing cookie files.</returns>
        public async Task<List<string>> GetCookieFilePathsAsync()
        {
            var cookieFiles = await _context.CookieFiles.Where(x => x.Remote == false).ToListAsync();
            return cookieFiles.Select(c => c.FileName).ToList();
        }

        /// <summary>
        /// Identifies the primary domain associated with a local cookie file.
        /// </summary>
        /// <param name="cookiePath">Path to the cookie JSON file.</param>
        /// <returns>The domain name of the cookie file.</returns>
        public async Task<string> IdentifyCookieSiteAsync(string cookiePath)
        {
            try
            {
                string cookieJson = await File.ReadAllTextAsync(cookiePath);
                var cookies = JsonConvert.DeserializeObject<List<CookieDto>>(cookieJson)!;
                return cookies.FirstOrDefault()?.Domain?.TrimStart('.') ?? string.Empty;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ CookieService error (IdentifyCookieSiteAsync): {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Loads cookies from a file into a Playwright page for automated browsing.
        /// All operations run on the local machine — no remote requests or proxies are applied.
        /// </summary>
        /// <param name="cookiePath">Path to the cookie JSON file.</param>
        /// <param name="hideBrowser">Whether to run browser in headless mode.</param>
        /// <returns>A tuple containing the Playwright page and browser instances.</returns>
        public async Task<(IPage page, IBrowser browser)> LoadCookieOnPageAsync(string cookiePath, bool hideBrowser)
        {
            try
            {
                var playwright = await Playwright.CreateAsync();
                var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
                {
                    Channel = "msedge",
                    Headless = hideBrowser,
                    Args = new[]
                    {
                        "--disable-gpu",
                        "--disable-dev-shm-usage",
                        "--no-sandbox",
                        "--disable-setuid-sandbox",
                        "--disable-accelerated-2d-canvas",
                        "--disable-background-timer-throttling",
                        "--disable-renderer-backgrounding",
                        "--disable-extensions",
                        "--disable-features=site-per-process,SiteIsolationTrial"
                    }
                });

                // Create isolated browser context (no proxy)
                var context = await browser.NewContextAsync(new BrowserNewContextOptions
                {
                    IgnoreHTTPSErrors = true
                });

                // Read the local cookie file and apply to context
                string cookieJson = await File.ReadAllTextAsync(cookiePath);
                var cookies = JsonConvert.DeserializeObject<List<CookieDto>>(cookieJson)!;

                var validCookies = cookies.Select(c => new Cookie
                {
                    Name = c.Name,
                    Value = c.Value,
                    Domain = c.Domain,
                    Path = c.Path ?? "/",
                    HttpOnly = c.HttpOnly,
                    Secure = c.Secure
                }).ToArray();

                await context.AddCookiesAsync(validCookies);

                // Open a new page
                var page = await context.NewPageAsync();

                // Determine target URL from first cookie domain
                var firstDomain = cookies.FirstOrDefault()?.Domain?.TrimStart('.') ?? string.Empty;
                if (!firstDomain.StartsWith("www.", StringComparison.OrdinalIgnoreCase))
                    firstDomain = "www." + firstDomain;

                var targetUrl = $"https://{firstDomain}";
                Console.WriteLine($"🌐 Navigating to {targetUrl}...");

                try
                {
                    await page.GotoAsync(targetUrl, new PageGotoOptions
                    {
                        WaitUntil = WaitUntilState.DOMContentLoaded,
                        Timeout = 90000 // 90 seconds
                    });
                }
                catch (TimeoutException)
                {
                    Console.WriteLine("⚠️ Navigation timeout — continuing with partially loaded page...");
                }

                // Optional debugging screenshot
                await page.ScreenshotAsync(new() { Path = "cookie_debug.png" });

                return (page, browser);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ CookieService error (LoadCookieOnPageAsync): {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Launches a Playwright browser page without loading any cookies, without applying any proxy.
        /// Useful for platforms like Airbnb where browsing does not require an authenticated session
        /// and a regional proxy is not required since the user provides their own VPN.
        /// </summary>
        /// <param name="hideBrowser">Whether to run browser in headless mode.</param>
        /// <returns>A tuple containing the Playwright page and browser instances.</returns>
        public async Task<(IPage page, IBrowser browser)> LaunchPageAsync(bool hideBrowser)
        {
            try
            {
                // Initialize Playwright and launch browser
                var playwright = await Playwright.CreateAsync();
                var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
                {
                    Channel = "msedge",
                    Headless = hideBrowser,
                    Args = new[]
                    {
                        "--disable-gpu",
                        "--disable-dev-shm-usage",
                        "--no-sandbox",
                        "--disable-setuid-sandbox",
                        "--disable-accelerated-2d-canvas",
                        "--disable-background-timer-throttling",
                        "--disable-renderer-backgrounding",
                        "--disable-extensions",
                        "--disable-features=site-per-process,SiteIsolationTrial"
                    }
                });

                // Create isolated browser context (no proxy — user provides their own VPN)
                var contextOptions = new BrowserNewContextOptions
                {
                    IgnoreHTTPSErrors = true
                };

                var context = await browser.NewContextAsync(contextOptions);

                // Open a new page
                var page = await context.NewPageAsync();

                return (page, browser);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ CookieService error (LaunchPageAsync): {ex.Message}");
                throw;
            }
        }
    }
}
