using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Taskly.Application.Constants;
using Taskly.Application.Dtos;
using Taskly.Application.Interfaces;
using Taskly.Domain;

namespace Taskly.Infrastructure.Services
{
    /// <summary>
    /// ExtractService coordinates scraping/extraction of content from multiple social media platforms.
    /// Supports Facebook, Instagram, Twitter, Reddit, and TikTok.
    /// Leverages platform-specific services for the actual scraping logic.
    /// </summary>
    public class ExtractService : IExtractService
    {
        private readonly IFacebookService _facebookService;
        private readonly IInstagramService _instagramService;
        private readonly ITwitterService _twitterService;
        private readonly IRedditService _redditService;
        private readonly ITikTokService _tikTokService;

        private readonly ICookieService _cookieService;

        /// <summary>
        /// Constructor for dependency injection.
        /// </summary>
        public ExtractService(
            IFacebookService facebookService,
            IInstagramService instagramService,
            ITwitterService twitterService,
            IRedditService redditService,
            ITikTokService tikTokService,
            IHttpContextAccessor httpContextAccessor,
            ICookieService cookieService)
        {
            _facebookService = facebookService;
            _instagramService = instagramService;
            _twitterService = twitterService;
            _redditService = redditService;
            _tikTokService = tikTokService;
            _cookieService = cookieService;
        }

        /// <summary>
        /// Extracts content/posts from specified platforms based on SearchDto.
        /// </summary>
        /// <param name="searchDto">DTO containing search parameters, platform, and query details.</param>
        /// <returns>Task representing the asynchronous extraction operation.</returns>
        /// <exception cref="ArgumentException">Thrown if an unsupported platform is specified.</exception>
        public async Task ExtractAsync(SearchDto searchDto)
        {
            // ✅ 3. Determine extraction scope

            if (searchDto.MultiPlatform)
            {
                // If user selects "All Platforms", initiate extraction for all services simultaneously.
                // Uses Task.WhenAll to run all searches concurrently for speed and efficiency.

                var tasks = new List<Task>();

                tasks.Add(_facebookService.SearchAsync(searchDto));  // Scrape Facebook
                tasks.Add(_instagramService.SearchAsync(searchDto)); // Scrape Instagram
                tasks.Add(_twitterService.SearchAsync(searchDto));   // Scrape Twitter
                tasks.Add(_redditService.SearchAsync(searchDto));    // Scrape Reddit
                tasks.Add(_tikTokService.SearchAsync(searchDto));    // Scrape TikTok

                // Await all platform tasks to finish
                await Task.WhenAll(tasks);
            }
            else
            {
                // If a specific platform is selected, call only that service.
                var domain = await _cookieService.IdentifyCookieSiteAsync(searchDto.CookiePath);

                // This ensures faster, platform-targeted scraping.
                switch (domain)
                {
                    case "facebook.com":
                        await _facebookService.SearchAsync(searchDto);
                        break;
                    case "instagram.com":
                        await _instagramService.SearchAsync(searchDto);
                        break;
                    case "x.com":
                        await _twitterService.SearchAsync(searchDto);
                        break;
                    case "reddit.com":
                        await _redditService.SearchAsync(searchDto);
                        break;
                    case "tiktok.com":
                        await _tikTokService.SearchAsync(searchDto);
                        break;
                    default:
                        // Throw error if the platform is not supported
                        throw new ArgumentException("Unsupported platform specified.");
                }
            }
        }
    }
}
