using Microsoft.EntityFrameworkCore;
using Microsoft.Playwright;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Taskly.Application.Constants;
using Taskly.Application.Dtos;
using Taskly.Application.Interfaces;
using Taskly.Domain;
using Taskly.Domain.Entities;
using static System.Runtime.InteropServices.JavaScript.JSType;

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
            ITikTokService tikTokService)
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
            if (searchDto.Platform == null)
            {
                var facebookTask = _facebookService.SearchAsync(searchDto);
                var instagramTask = _instagramService.SearchAsync(searchDto);
                var twitterTask = _twitterService.SearchAsync(searchDto);
                var redditTask = _redditService.SearchAsync(searchDto);
                var titkokTask = _tikTokService.SearchAsync(searchDto); // Placeholder for TikTok service if needed in the future

                await Task.WhenAll(facebookTask, instagramTask, twitterTask, redditTask, titkokTask);
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
