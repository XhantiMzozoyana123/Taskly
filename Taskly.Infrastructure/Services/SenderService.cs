using Microsoft.EntityFrameworkCore;
using Microsoft.Playwright;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Taskly.Application.Dtos;
using Taskly.Application.Interfaces;
using Taskly.Domain;
using Taskly.Domain.Entities;

namespace Taskly.Infrastructure.Services
{
    public class SenderService : ISenderService
    {
        private readonly ApplicationDbContext _context;
        private readonly IFacebookService _facebookService;
        private readonly IInstagramService _instagramService;
        private readonly ITwitterService _twitterService;
        private readonly ITikTokService _tikTokService;
        private readonly IAiService _aiService;
        private readonly ICookieService _cookieService;

        private readonly Random _random = new Random();

        public SenderService(
            ApplicationDbContext context,
            IFacebookService facebookService,
            IInstagramService instagramService,
            ITwitterService twitterService,
            ITikTokService tikTokService,
            IAiService aiService,
            ICookieService cookieService)
        {
            _context = context;
            _facebookService = facebookService;
            _instagramService = instagramService;
            _twitterService = twitterService;
            _tikTokService = tikTokService;
            _aiService = aiService;
            _cookieService = cookieService;
        }

        public async Task MessagingSequenceAsync(MessengerDto messengerDto)
        {
            if (string.IsNullOrWhiteSpace(messengerDto.Text) && !messengerDto.TextList.Any())
                return;

            IBrowser browser = null;
            IPage page = null;

            try
            {
                // 🔹 Load cookie accounts
                var cookieAccounts = await _context.CookieFiles
                    .OrderBy(x => x.Id)
                    .ToListAsync();

                if (!cookieAccounts.Any())
                    throw new Exception("No active cookie accounts found for rotation.");

                // 🔹 Load leads
                var leads = await _context.Leads
                    .Where(x => x.Status == "New")
                    .ToListAsync();

                if (!leads.Any())
                    return;

                int accountIndex = 0;
                var random = new Random();

                for (int i = 0; i < leads.Count; i++)
                {
                    // 🔁 Account Rotation
                    CookieFiles currentAccount;
                    if (messengerDto.AccountRotation)
                    {
                        currentAccount = cookieAccounts[accountIndex];
                        accountIndex = (accountIndex + 1) % cookieAccounts.Count;
                    }
                    else
                    {
                        // Use single account
                        currentAccount = cookieAccounts.First();
                    }

                    (page, browser) = await _cookieService.LoadCookieOnPageAsync(currentAccount.FileName, messengerDto.PrivateMode);
                    var lead = leads[i];

                    // 🧠 Message Rotation
                    string messageText;
                    if (messengerDto.MessegeRotation && messengerDto.TextList.Any())
                    {
                        messageText = messengerDto.TextList[random.Next(messengerDto.TextList.Count)];
                    }
                    else
                    {
                        messageText = messengerDto.Text;
                    }

                    // Replace placeholders dynamically
                    var iceBreaker = await _context.Icebreakers.FirstOrDefaultAsync(x => x.LeadId == lead.Id)
                        ?? new Icebreakers { Text = "Hey!", LeadId = lead.Id };

                    messageText = messageText
                        .Replace("[name]", lead.Name)
                        .Replace("[descr]", lead.PostDescription)
                        .Replace("[url]", lead.PostUrl)
                        .Replace("[date]", lead.PostDate.ToString())
                        .Replace("[icebreak]", iceBreaker.Text);

                    // Build new message
                    var message = new MessengerDto
                    {
                        Text = messageText,
                        Lead = lead
                    };

                    bool sent = await SendDM(message, page);
                    await browser.CloseAsync();

                    // Simulate human delay
                    await Task.Delay(TimeSpan.FromSeconds(random.Next(3, 6)));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Error] {ex.Message}");
                throw;
            }
            finally
            {
                if (browser != null)
                    await browser.CloseAsync();
            }
        }

        private async Task<bool> SendDM(MessengerDto messengerDto, IPage page)
        {
            try
            {
                // Random delay between messages
                var delay = new Random().Next(15_000, 45_000); // 15–45 seconds
                await Task.Delay(delay);

                if (messengerDto.Lead.Status == "New")
                {
                    switch (messengerDto.Lead.Platform.ToLower())
                    {
                        case "facebook":
                            await _facebookService.DirectMessagingAsync(page, messengerDto);
                            break;
                        case "instagram":
                            await _instagramService.DirectMessagingAsync(page, messengerDto);
                            break;
                        case "twitter":
                            await _twitterService.DirectMessagingAsync(page, messengerDto);
                            break;
                        case "tiktok":
                            await _tikTokService.DirectMessagingAsync(page, messengerDto);
                            break;
                        default:
                            throw new ArgumentException("Unsupported platform specified.");
                    }

                    await UpdateLead(messengerDto.Lead);
                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SendDM Error] {ex.Message}");
            }

            return false;
        }

        private async Task UpdateLead(Leads lead)
        {
            lead.Status = "Contacted";
            _context.Leads.Update(lead);
            await _context.SaveChangesAsync();
        }
    }
}
