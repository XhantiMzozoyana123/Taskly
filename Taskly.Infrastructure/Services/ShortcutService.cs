using Microsoft.EntityFrameworkCore;
using Microsoft.Playwright;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Taskly.Application.Interfaces;
using Taskly.Domain;

namespace Taskly.Application.Services
{
    public class ShortcutService : IShortcutService
    {
        private readonly Dictionary<string, ShortcutAction> _shortcutMap = new();

        // Dependencies (inject your services)
        private readonly IExtractService _extractService;
        private readonly ICookieService _cookieService;
        private readonly IFacebookService _facebookService;
        private readonly ITwitterService _twitterService;

        IPage _page;
        IBrowser _browser;
        int _navigator = 0;

        private readonly ApplicationDbContext _context;

        public ShortcutService(
            IExtractService extractService,
            ICookieService cookieService,
            IFacebookService facebookService,
            ITwitterService twitterService,
            ApplicationDbContext context)
        {
            _extractService = extractService;
            _cookieService = cookieService;
            _facebookService = facebookService;
            _twitterService = twitterService;
            _context = context;
        }

        public List<ShortcutAction> GetAllActions()
        {
            return new List<ShortcutAction>((ShortcutAction[])Enum.GetValues(typeof(ShortcutAction)));
        }

        public Dictionary<string, ShortcutAction> GetRegisteredShortcuts()
        {
            return new Dictionary<string, ShortcutAction>(_shortcutMap);
        }

        public void RegisterShortcut(string keyCombination, ShortcutAction action)
        {
            if (!_shortcutMap.ContainsKey(keyCombination))
            {
                _shortcutMap.Add(keyCombination, action);
            }
            else
            {
                _shortcutMap[keyCombination] = action; // override existing
            }
        }

        public async Task ExecuteShortcutAsync(string keyCombination)
        {
            if (!_shortcutMap.ContainsKey(keyCombination)) return;

            var action = _shortcutMap[keyCombination];

            switch (action)
            {
                // Hybrid Search Actions
                case ShortcutAction.LaunchHybridSearchBrowser:
                    await LaunchBrowserAsync();
                    break;
                case ShortcutAction.AddToLeadList:
                    // Example: extract leads via extract service
                    await SaveContactAsLeadAsync();
                    break;
                case ShortcutAction.NavigatePreviousLead:
                    await NavigatePreviousLeadAsync();
                    break;
                case ShortcutAction.NavigateNextLead:
                    await NavigateNextLeadAsync();
                    break;

                // Messaging Actions
                case ShortcutAction.RotateTemplates:
                    await PopulateMessengerAsync("template");
                    break;
                case ShortcutAction.RotateIcebreakers:
                    await PopulateMessengerAsync("icebreaker");
                    break;
                case ShortcutAction.RotateCustomMessages:
                    await PopulateMessengerAsync("custom");
                    break;

                // Cookie Actions
                case ShortcutAction.RotateCookies:
                    await RotateCookieSessionAsync();
                    break;
            }
        }

        private async Task LaunchBrowserAsync() 
        {
            var cookiePath = await _cookieService.GetCookieFilePathsAsync();
            (_page, _browser) = await _cookieService.LoadCookieOnPageAsync(cookiePath.First(), false);
        }

        private async Task NavigatePreviousLeadAsync()
        {
            if (_navigator > 0) _navigator--;
            var lead = await _context.Leads.Skip(_navigator).FirstOrDefaultAsync();
            if (lead != null)
                await _page.GotoAsync(lead.ProfileUrl);
        }

        private async Task NavigateNextLeadAsync()
        {
            var totalLeads = await _context.Leads.CountAsync();
            if (_navigator < totalLeads - 1) _navigator++;
            var lead = await _context.Leads.Skip(_navigator).FirstOrDefaultAsync();
            if (lead != null)
                await _page.GotoAsync(lead.ProfileUrl);
        }

        private async Task SaveContactAsLeadAsync()
        {
            var profile = _page.Url;

            switch(profile)
            {
                case var u when u.Contains("facebook.com"):
                    // Facebook specific action
                    _page = await _facebookService.ExtractSelectedProfileAsync(_page);
                    break;
                case var u when u.Contains("instagram.com"):
                    // Instagram specific action
                    break;
                case var u when u.Contains("x.com"):
                    // X specific action
                    _page = await _twitterService.ExtractSelectedProfileAsync(_page);
                    break;
                case var u when u.Contains("reddit.com"):
                    // Reddit specific action
                    break;
                case var u when u.Contains("tiktok.com"):
                    // TikTok specific action
                    break;
            }
        }

        private async Task PopulateMessengerAsync(string action) 
        {
            var profile = _page.Url;

            switch (profile)
            {
                case var u when u.Contains("facebook.com"):
                    // Facebook specific action
                    _page = await _facebookService.InjectMessenger(_page, action);
                    break;
                case var u when u.Contains("instagram.com"):
                    // Instagram specific action
                    break;
                case var u when u.Contains("x.com"):
                    // X specific action
                    _page = await _twitterService.ExtractSelectedProfileAsync(_page);
                    break;
                case var u when u.Contains("reddit.com"):
                    // Reddit specific action
                    break;
                case var u when u.Contains("tiktok.com"):
                    // TikTok specific action
                    break;
            }
        }

        private async Task RotateCookieSessionAsync()
        {
            var paths = await _cookieService.GetCookieFilePathsAsync();

            Random rand = new Random();
            int index = rand.Next(paths.Count);

            (_page, _browser) = await _cookieService.LoadCookieOnPageAsync(paths[index], false);
        }
    }
}
