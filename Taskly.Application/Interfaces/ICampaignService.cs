using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Taskly.Domain.Entities;

namespace Taskly.Application.Interfaces
{
    public interface ICampaignService
    {
        // --- Campaign Lifecycle Methods ---

        /// <summary>
        /// Initializes a campaign, sets its status to "Active", and schedules its first execution
        /// at the designated StartDate using Hangfire.
        /// </summary>
        Task StartCampaignAsync(Campaigns campaign);

        /// <summary>
        /// Pauses a campaign by setting its status to "Inactive".
        /// </summary>
        Task PauseCampaignAsync(Campaigns campaign);

        /// <summary>
        /// Resumes a campaign by setting its status to "Active".
        /// </summary>
        Task ResumeCampaignAsync(Campaigns campaign);

        // --- Scheduling and Execution Methods ---

        /// <summary>
        /// Retrieves a campaign and schedules all sequences and messages for execution via Hangfire.
        /// (Designed to be called by Hangfire).
        /// </summary>
        /// <param name="campaignId">The ID of the campaign to run.</param>
        Task RunCampaignsAsync(int campaignId);

        /// <summary>
        /// Marks a campaign as "Completed" at its designated EndDate. (Designed to be called by Hangfire).
        /// </summary>
        /// <param name="campaignId">The ID of the campaign to complete.</param>
        Task CompleteCampaignAsync(int campaignId);
    }
}
