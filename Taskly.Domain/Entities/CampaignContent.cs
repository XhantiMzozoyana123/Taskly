using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Taskly.Domain.Entities
{
    public class CampaignContent : BaseEntity
    {
        public int CampaignMessageId { get; set; }

        public string MessageText { get; set; } = string.Empty;

        public bool Sent { get; set; }

        public bool Replied { get; set; }
    }
}
