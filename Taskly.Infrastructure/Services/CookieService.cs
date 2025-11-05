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
    /// Handles loading browser cookies into a Playwright page.
    /// Supports pre-authenticated sessions for automation and account rotation.
    /// </summary>
    public class CookieService : ICookieService
    {
        private readonly ApplicationDbContext _context;
        
        public CookieService(ApplicationDbContext context) 
        {
            _context = context;
        }

        public async Task<List<string>> GetCookieFilePathsAsync()
        {
            var query = await _context.CookieFiles.ToListAsync();
            return query.Select(c => c.FileName).ToList();
        }

        public async Task<string> IdentifyCookieSiteAsync(string cookiePath)
        {
            if (!File.Exists(cookiePath))
                throw new FileNotFoundException($"Cookie file not found at: {cookiePath}");

            try
            {
                // Read and apply cookies
                var cookieJson = await File.ReadAllTextAsync(cookiePath);
                var cookies = JsonConvert.DeserializeObject<List<CookieDto>>(cookieJson)!;
                var domain = cookies.FirstOrDefault()?.Domain?.TrimStart('.') ?? string.Empty;

                return domain;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ CookieService error: {ex.Message}");
                throw;
            }
        }

        public async Task<(IPage page, IBrowser browser)> LoadCookieOnPageAsync(string cookiePath, bool hideBrowser)
        {
            if (!File.Exists(cookiePath))
                throw new FileNotFoundException($"Cookie file not found at: {cookiePath}");

            try
            {
                // Initialize Playwright
                var playwright = await Playwright.CreateAsync();

                // Launch browser
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

                // Create isolated context
                var context = await browser.NewContextAsync(new BrowserNewContextOptions
                {
                    IgnoreHTTPSErrors = true
                });

                // Read and apply cookies
                var cookieJson = await File.ReadAllTextAsync(cookiePath);
                var cookies = JsonConvert.DeserializeObject<List<CookieDto>>(cookieJson)!;

                // Map CookieDto -> Playwright.Cookie
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

                // Open page
                var page = await context.NewPageAsync();

                // --- FIXED: domain handling and safer navigation ---
                var firstDomain = cookies.FirstOrDefault()?.Domain?.TrimStart('.');
                if (!firstDomain.StartsWith("www.", StringComparison.OrdinalIgnoreCase))
                    firstDomain = "www." + firstDomain;

                var targetUrl = $"https://{firstDomain}";

                Console.WriteLine($"🌐 Navigating to {targetUrl}...");

                try
                {
                    await page.GotoAsync(targetUrl, new PageGotoOptions
                    {
                        WaitUntil = WaitUntilState.DOMContentLoaded, // faster, avoids full page load wait
                        Timeout = 90000 // 90 seconds
                    });
                }
                catch (TimeoutException)
                {
                    Console.WriteLine("⚠️ Navigation timeout — continuing with partially loaded page...");
                }

                // Optional: Take screenshot for debugging
                await page.ScreenshotAsync(new() { Path = "cookie_debug.png" });

                return (page, browser);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ CookieService error: {ex.Message}");
                throw;
            }
        }

        public async Task<UploadResponseDto> UploadFileAsync(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException($"File not found: {filePath}");

            using var httpClient = new HttpClient();
            using var form = new MultipartFormDataContent();
            using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read);

            var fileContent = new StreamContent(stream);
            fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
            form.Add(fileContent, "file", Path.GetFileName(filePath));

            string domain = _context.Settings.FirstOrDefault().MasterDomainUrl;
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
    }
}
