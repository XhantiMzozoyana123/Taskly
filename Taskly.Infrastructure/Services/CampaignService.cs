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
    /// <summary>
    /// Handles campaign operations such as starting, pausing, resuming, and processing messages.
    /// Integrates with Hangfire for background scheduling and ISenderService for message delivery.
    /// </summary>
    public class CampaignService : ICampaignService
    {
        private readonly ApplicationDbContext _context;
        private readonly ISenderService _senderService;

        public CampaignService(ApplicationDbContext context, ISenderService senderService)
        {
            _context = context;
            _senderService = senderService;
        }

        /// <summary>
        /// Pauses a campaign by setting its status to "Inactive".
        /// </summary>
        public Task PauseCampaignAsync(Campaigns campaign)
        {
            campaign.Status = "Inactive";
            _context.Campaigns.Update(campaign);
            return _context.SaveChangesAsync();
        }

        /// <summary>
        /// Resumes a campaign by setting its status to "Active".
        /// </summary>
        public Task ResumeCampaignAsync(Campaigns campaign)
        {
            campaign.Status = "Active";
            _context.Campaigns.Update(campaign);
            return _context.SaveChangesAsync();
        }

        /// <summary>
        /// Schedules all sequences and messages of a campaign for execution via Hangfire.
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
                    // Schedule message processing with delay based on sequence's wait time
                    BackgroundJob.Schedule<CampaignService>(
                        service => service.ProcessCampaignMessageAsync(sequence.Id, message.Id),
                        TimeSpan.FromHours(sequence.WaitTimeInHours)
                    );
                }
            }
        }

        /// <summary>
        /// Processes a single campaign message. Called by Hangfire in the background.
        /// </summary>
        private async Task ProcessCampaignMessageAsync(int sequenceId, int messageId)
        {
            try
            {
                var sequence = await _context.CampaignSequences.FindAsync(sequenceId);
                var message = await _context.CampaignMessages.FindAsync(messageId);

                if (sequence == null || message == null) return;

                // Get all active leads for this campaign sequence
                var leads = await _context.Leads
                    .Where(l => l.CampaignId == sequence.CampaignId)
                    .ToListAsync();

                // Get all message contents associated with this message
                var contents = await _context.CampaignContents
                    .Where(c => c.CampaignMessageId == message.Id)
                    .ToListAsync();

                if (!contents.Any()) return;

                // Build DTO for sending messages
                var messengerDto = new MessengerDto
                {
                    Text = contents.First().MessageText,
                    TextList = contents.Select(c => c.MessageText).ToList(),
                    MessegeRotation = message.MessageRotation,
                    AccountRotation = sequence.AccountRotation,
                    PrivateMode = true,
                    MessageDelay = (int)(message.WaitTimeInMinutes * 60 * 1000) // Convert minutes to milliseconds
                };

                // Send the message to all leads
                await _senderService.StartCampaignMessages(messengerDto, leads);

                // Mark sequence as completed after processing
                sequence.Completed = true;
                _context.CampaignSequences.Update(sequence);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                // Log exception and allow Hangfire to handle retries
                Console.WriteLine($"Error processing sequence {sequenceId}, message {messageId}: {ex.Message}");
                throw;
            }
        }
    }
}
