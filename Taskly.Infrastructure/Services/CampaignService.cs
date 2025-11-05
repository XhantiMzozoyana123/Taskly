using Hangfire;
using Microsoft.EntityFrameworkCore;
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
    public class CampaignService : ICampaignService
    {
        private readonly ApplicationDbContext _context;
        private readonly ISenderService _senderService;

        public CampaignService(ApplicationDbContext context, ISenderService senderService)
        {
            _context = context;
            _senderService = senderService;
        }

        public Task PauseCampaignAsync(Campaigns campaign)
        {
            campaign.Status = "Inactive";
            _context.Campaigns.Update(campaign);
            return _context.SaveChangesAsync();
        }

        public Task ResumeCampaignAsync(Campaigns campaign)
        {
            campaign.Status = "Active";
            _context.Campaigns.Update(campaign);
            return _context.SaveChangesAsync();
        }

        /// <summary>
        /// Schedule all sequences and messages of a campaign
        /// </summary>
        public async Task RunCampaignsAsync(Campaigns campaign)
        {
            var sequences = await _context.CampaignSequences
                .Where(s => s.CampaignId == campaign.Id && !s.Completed)
                .OrderBy(s => s.Id)
                .ToListAsync();

            foreach (var sequence in sequences)
            {
                var messages = await _context.CampaignMessages
                    .Where(m => m.CampaignSequenceId == sequence.Id)
                    .OrderBy(m => m.Id)
                    .ToListAsync();

                foreach (var message in messages)
                {
                    // Schedule message processing via Hangfire
                    BackgroundJob.Schedule<CampaignService>(
                        service => service.ProcessCampaignMessageAsync(sequence.Id, message.Id),
                        TimeSpan.FromHours(sequence.WaitTimeInHours)
                    );
                }
            }
        }

        /// <summary>
        /// Process a single campaign message (executed by Hangfire)
        /// </summary>
        private async Task ProcessCampaignMessageAsync(int sequenceId, int messageId)
        {
            try
            {
                var sequence = await _context.CampaignSequences.FindAsync(sequenceId);
                var message = await _context.CampaignMessages.FindAsync(messageId);

                if (sequence == null || message == null) return;

                // Fetch active leads at runtime
                var leads = await _context.Leads
                    .Where(l => l.CampaignId == sequence.CampaignId)
                    .ToListAsync();

                var contents = await _context.CampaignContents
                    .Where(c => c.CampaignMessageId == message.Id)
                    .ToListAsync();

                if (!contents.Any()) return;

                // Build MessengerDto
                var messengerDto = new MessengerDto
                {
                    Text = contents.First().MessageText,
                    TextList = contents.Select(c => c.MessageText).ToList(),
                    MessegeRotation = message.MessageRotation,
                    AccountRotation = sequence.AccountRotation,
                    PrivateMode = true,
                    MessageDelay = (int)(message.WaitTimeInMinutes * 60 * 1000)
                };

                // Send message to all leads
                await _senderService.StartCampaignMessages(messengerDto, leads);

                // After all messages in sequence are processed, mark sequence completed
                sequence.Completed = true;
                _context.CampaignSequences.Update(sequence);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                // Replace with proper logging
                Console.WriteLine($"Error processing sequence {sequenceId}, message {messageId}: {ex.Message}");
                throw; // Let Hangfire retry if enabled
            }
        }
    }
}
