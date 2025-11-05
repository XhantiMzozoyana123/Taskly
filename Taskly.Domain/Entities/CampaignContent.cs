using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Taskly.Domain.Entities
{
    public class CampaignContent 
    {
        [Key]
        public int Id { get; set; }

        public int CampaignMessageId { get; set; }

        public string MessageText { get; set; } = string.Empty;

        public bool Sent { get; set; }

        public bool Replied { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }
}
