using Microsoft.EntityFrameworkCore;
using Microsoft.Playwright;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using Taskly.Application.Dtos;
using Taskly.Application.Interfaces;
using Taskly.Domain;

namespace Taskly.Infrastructure.Services
{
    /// <summary>
    /// Handles browser cookie operations using Playwright.
    /// Supports loading cookies into pre-authenticated sessions for automation,
    /// identifying cookie domains, and uploading files for automation purposes.
    /// </summary>
    public class CookieService : ICookieService
    {
        private readonly ApplicationDbContext _context;

        public CookieService(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Gets all cookie file paths stored in the database.
        /// </summary>
        /// <returns>List of file names representing cookie files.</returns>
        public async Task<List<string>> GetCookieFilePathsAsync()
        {
            var settings = await _context.Settings.FirstOrDefaultAsync();
            var httpMode = settings.ProcessDataRemotely;

            if (httpMode)
            {
                var cookieFiles = await _context.CookieFiles.Where(x => x.Remote == true).ToListAsync();
                return cookieFiles.Select(c => c.FileName).ToList();
            }
            else
            {
                var cookieFiles = await _context.CookieFiles.Where(x => x.Remote == false).ToListAsync();
                return cookieFiles.Select(c => c.FileName).ToList();
            }
        }

        /// <summary>
        /// Identifies the primary domain associated with a cookie file.
        /// </summary>
        /// <param name="cookiePath">Path to the cookie JSON file.</param>
        /// <returns>The domain name of the cookie file.</returns>
        public async Task<string> IdentifyCookieSiteAsync(string cookiePath)
        {
            if (!File.Exists(cookiePath))
                throw new FileNotFoundException($"Cookie file not found at: {cookiePath}");

            try
            {
                var cookieJson = await File.ReadAllTextAsync(cookiePath);
                var cookies = JsonConvert.DeserializeObject<List<CookieDto>>(cookieJson)!;

                // Return the first cookie's domain (trim leading dot)
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
        /// </summary>
        /// <param name="cookiePath">Path to the cookie JSON file.</param>
        /// <param name="hideBrowser">Whether to run browser in headless mode.</param>
        /// <returns>A tuple containing the Playwright page and browser instances.</returns>
        public async Task<(IPage page, IBrowser browser)> LoadCookieOnPageAsync(string cookiePath, bool hideBrowser)
        {
            if (!File.Exists(cookiePath))
                throw new FileNotFoundException($"Cookie file not found at: {cookiePath}");

            try
            {
                // Initialize Playwright and launch browser
                var playwright = await Playwright.CreateAsync();
                var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
                {
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

                // Create isolated browser context
                var context = await browser.NewContextAsync(new BrowserNewContextOptions
                {
                    IgnoreHTTPSErrors = true
                });

                // Read cookies and apply to context
                var cookieJson = await File.ReadAllTextAsync(cookiePath);
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
        /// Uploads a file to the configured API endpoint.
        /// </summary>
        /// <param name="filePath">Path to the file to upload.</param>
        /// <returns>Response DTO from the upload API.</returns>
        public async Task<UploadResponseDto> UploadFileRemotelyAsync(string filePath)
        {
            try
            {
                if (!File.Exists(filePath))
                    throw new FileNotFoundException($"File not found: {filePath}");

                using var httpClient = new HttpClient();
                using var form = new MultipartFormDataContent();
                using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read);

                var fileContent = new StreamContent(stream);
                fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                form.Add(fileContent, "file", Path.GetFileName(filePath));

                string domain = _context.Settings.FirstOrDefault()?.MasterDomainUrl
                                ?? throw new Exception("MasterDomainUrl not configured in settings.");
                string apiUrl = $"{domain}api/fileupload/upload";

                var response = await httpClient.PostAsync(apiUrl, form);
                response.EnsureSuccessStatusCode();

                string json = await response.Content.ReadAsStringAsync();
                var uploadResult = System.Text.Json.JsonSerializer.Deserialize<UploadResponseDto>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }) ?? throw new Exception("Failed to deserialize upload response");

                return uploadResult;
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }
    }
}
