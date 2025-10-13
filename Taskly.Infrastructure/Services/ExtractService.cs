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
    public class ExtractService : IExtractService
    {
        private readonly ApplicationDbContext _context;
        private readonly IFacebookService _facebookService;
        private readonly IInstagramService _instagramService;
        private readonly ITwitterService _twitterService;
        private readonly IRedditService _redditService;
        private readonly ITikTokService _tikTokService;

        public ExtractService(
            ApplicationDbContext context,
            IFacebookService facebookService,
            IInstagramService instagramService,
            ITwitterService twitterService,
            IRedditService redditService,
            ITikTokService tikTokService,
            IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _facebookService = facebookService;
            _instagramService = instagramService;
            _twitterService = twitterService;
            _redditService = redditService;
            _tikTokService = tikTokService;
        }

        public async Task ExtractAsync(SearchDto searchDto)
        {
            // ✅ 3. Continue with extraction logic
            if (searchDto.Platform == "All Platforms")
            {
                // When no platform is specified, extract based on user’s subscription tier
                var tasks = new List<Task>();

                tasks.Add(_facebookService.SearchAsync(searchDto));
                tasks.Add(_instagramService.SearchAsync(searchDto));
                tasks.Add(_twitterService.SearchAsync(searchDto));
                tasks.Add(_redditService.SearchAsync(searchDto));
                tasks.Add(_tikTokService.SearchAsync(searchDto));

                await Task.WhenAll(tasks);
            }
            else
            {
                switch (searchDto.Platform.ToLower())
                {
                    case "facebook":
                        await _facebookService.SearchAsync(searchDto);
                        break;
                    case "instagram":
                        await _instagramService.SearchAsync(searchDto);
                        break;
                    case "twitter":
                        await _twitterService.SearchAsync(searchDto);
                        break;
                    case "reddit":
                        await _redditService.SearchAsync(searchDto);
                        break;
                    case "tiktok":
                        await _tikTokService.SearchAsync(searchDto);
                        break;
                    default:
                        throw new ArgumentException("Unsupported platform specified.");
                }
            }
        }
    }
}
