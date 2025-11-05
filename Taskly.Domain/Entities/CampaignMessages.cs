using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Taskly.Domain.Entities
{
    public class CampaignMessages : BaseEntity
    {
        public int CampaignSequenceId { get; set; }

        public int WaitTimeInMinutes { get; set; }

        public bool MessageRotation { get; set; } = false; // Indicates if message rotation is enabled
    }
}
