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
        Task RunCampaignsAsync(Campaigns campaigns);

        Task ResumeCampaignAsync(Campaigns campaign);

        Task PauseCampaignAsync(Campaigns campaign);
    }
}
