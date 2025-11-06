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

        // --- NEW FUNCTIONALITY: START AND END ---

        /// <summary>
        /// Initializes a campaign, sets its status to "Active", and schedules its first execution
        /// at the designated StartDate using Hangfire.
        /// </summary>
        public async Task StartCampaignAsync(Campaigns campaign)
        {
            // 1. Update campaign status and save. Use the defined StartDate.
            campaign.Status = "Active";
            campaign.UpdatedAt = DateTime.Now;
            _context.Campaigns.Update(campaign);
            await _context.SaveChangesAsync();

            // 2. Schedule the job to run at the campaign's defined StartDate.
            BackgroundJob.Schedule<CampaignService>(
                service => service.RunCampaignsAsync(campaign.Id),
                campaign.StartDate
            );

            // 3. Schedule a job to automatically complete the campaign at the EndDate.
            if (campaign.EndDate > campaign.StartDate)
            {
                BackgroundJob.Schedule<CampaignService>(
                    service => service.CompleteCampaignAsync(campaign.Id),
                    campaign.EndDate
                );
            }
        }

        /// <summary>
        /// Marks a campaign as "Completed" at its designated EndDate. Called by Hangfire.
        /// </summary>
        public async Task CompleteCampaignAsync(int campaignId)
        {
            var campaign = await _context.Campaigns.FindAsync(campaignId);
            if (campaign == null) return;

            // Only complete if currently active
            if (campaign.Status == "Active")
            {
                campaign.Status = "Completed";
                campaign.UpdatedAt = DateTime.Now;
                _context.Campaigns.Update(campaign);
                await _context.SaveChangesAsync();

                // Note: Complex logic for stopping scheduled Hangfire jobs is not included here.
            }
        }

        // --- EXISTING FUNCTIONALITY (with RunCampaignsAsync updated) ---

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
        /// Retrieves a campaign and schedules all sequences and messages for execution via Hangfire.
        /// Called by Hangfire in the background.
        /// </summary>
        public async Task RunCampaignsAsync(int campaignId)
        {
            var campaign = await _context.Campaigns.FindAsync(campaignId);
            // Check if campaign exists and is still active before processing
            if (campaign == null || campaign.Status != "Active") return;

            var sequences = await _context.CampaignSequences
                .Where(s => s.CampaignId == campaignId && !s.Completed)
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
                    // The job is scheduled relative to when RunCampaignsAsync executes.
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

                // Check campaign status again to ensure processing hasn't been paused/completed
                var campaign = await _context.Campaigns.FindAsync(sequence?.CampaignId);

                if (sequence == null || message == null || campaign?.Status != "Active") return;

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
                    // Convert minutes to milliseconds for the sender service
                    MessageDelay = (int)(message.WaitTimeInMinutes * 60 * 1000)
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