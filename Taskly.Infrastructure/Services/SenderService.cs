using Microsoft.EntityFrameworkCore;
using Microsoft.Playwright;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using Taskly.Application.Dtos;
using Taskly.Application.Interfaces;
using Taskly.Domain;
using Taskly.Domain.Entities;

namespace Taskly.Infrastructure.Services
{
    public class SenderService : ISenderService
    {
        private List<MessengerDto> messengerDtos = new List<MessengerDto>();
        private readonly ApplicationDbContext _context;

        private readonly IFacebookService _facebookService;
        private readonly IInstagramService _instagramService;
        private readonly ITwitterService _twitterService;
        private readonly ITikTokService _tikTokService;

        private readonly IAiService _aiService;

        public SenderService(
            ApplicationDbContext context,
            IFacebookService facebookService,
            IInstagramService instagramService,
            ITwitterService twitterService,
            ITikTokService tikTokService,
            IAiService aiService)
        {
            _context = context;
            _facebookService = facebookService;
            _instagramService = instagramService;
            _twitterService = twitterService;
            _tikTokService = tikTokService;
            _aiService = aiService;
        }

        public async Task AutomatedMessagingAsync(MessengerDto messengerDto)
        {
            if (string.IsNullOrWhiteSpace(messengerDto.Text))
            {
                return;
            }

            using var playwright = await Playwright.CreateAsync();
            await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Headless = messengerDto.PrivateMode });
            var page = await browser.NewPageAsync();

            try
            {
                int abTestsNo = 0;

                if (messengerDto.AbTestRotation == true)
                {
                    var emailer = _context.Leads.Where(x => x.Status == "New" &&
                        x.UserId == messengerDto.UserId).ToList();

                    await AbTestRotation(emailer, page);
                }
                else
                {
                    var text = messengerDto.Text;
                    var query = _context.Leads.Where(x => x.Status == "New" &&
                        x.UserId == messengerDto.UserId).ToList();

                    foreach (var item in query)
                    {
                        var iceBreaker = await _context.Icebreakers.FirstAsync(x => x.LeadId == item.Id 
                                && x.UserId == messengerDto.UserId);

                        if (iceBreaker == null)
                        {
                            iceBreaker = new Icebreakers()
                            {
                                Text = "Hello!",
                                LeadId = item.Id
                            };
                        }

                        string newText = text.Replace("[name]", item.Name)
                                                        .Replace("[descr]", item.PostDescription)
                                                        .Replace("[url]", item.PostUrl)
                                                        .Replace("[date]", item.PostDate.ToString())
                                                        .Replace("[icebreak]", iceBreaker.Text);

                        MessengerDto messenger = new MessengerDto();

                        messenger.Text = newText;
                        messenger.Lead = item;

                        await SendDM(messenger, page);
                    }
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


        private async Task AbTestRotation(List<Leads> lead, IPage page) 
        {
            for (int i = 0; i <= lead.Count; i++)
            {
                Random random = new Random();
                int r = random.Next(messengerDtos.Count);

                var text = messengerDtos[r].Text;


                var iceBreaker = await _context.Icebreakers.FirstAsync(x => x.LeadId == messengerDtos[r].Lead.Id
                                && x.UserId == messengerDtos[r].UserId);

                if (iceBreaker == null)
                {
                    iceBreaker = new Icebreakers()
                    {
                        Text = "Hello!",
                        LeadId = lead[i].Id
                    };
                }

                string content = text.Replace("[name]", lead[i].Name)
                                                        .Replace("[descr]", lead[i].PostDescription)
                                                        .Replace("[url]", lead[i].PostUrl)
                                                        .Replace("[date]", lead[i].PostDate.ToString())
                                                        .Replace("[icebreak]", iceBreaker.Text);

                MessengerDto messenger = new MessengerDto();

                messenger.Text = content;
                messenger.Lead = lead[i];

                await SendDM(messenger, page);
            }
        }

        private async Task<bool> SendDM(MessengerDto messengerDto, IPage page)
        {
            try
            {
                if (messengerDto.Lead.Status == "New")
                {
                    switch (messengerDto.Lead.Platform)
                    {
                        case "Facebook":
                            await _facebookService.DirectMessagingAsync(page, messengerDto);
                            break;
                        case "Instagram":
                            await _instagramService.DirectMessagingAsync(page, messengerDto);
                            break;
                        case "Twitter":
                            await _instagramService.DirectMessagingAsync(page, messengerDto);
                            break;
                        case "Tik-Tok":
                            await _tikTokService.DirectMessagingAsync(page, messengerDto);
                            break;
                    }

                    await UpdateLead(messengerDto.Lead);
                }
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        private async Task UpdateLead(Leads lead)
        {
            lead.Status = "Contacted"; 
            _context.Leads.Update(lead);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> ManualMessagingAsync(MessengerDto messengerDto)
        {
            try
            {
                var leads = await _context.Leads.Where(x => x.UserId == messengerDto.UserId
                       && x.Status == "New").ToListAsync();

                foreach (var lead in leads)
                {
                    var content = await _aiService.GenerateDirectMessageAsync(new SearchDto() 
                    {
                        Keyword = lead.Keywords,
                        Query = lead.Query
                    });

                    var message = new Messages()
                    {
                        UserId = messengerDto.UserId,
                        LeadId = lead.Id,
                        iceBreakerId = await _context.Icebreakers
                                    .Select(i => (int?)i.Id)
                                    .FirstOrDefaultAsync() ?? 0,
                        Text = content,
                        Status = "New"
                    };

                    await _context.Messages.AddAsync(message);
                    await _context.SaveChangesAsync();
                }
            }
            catch(Exception)
            {
                return false;
            }

            return true;    
        }
    }
}
