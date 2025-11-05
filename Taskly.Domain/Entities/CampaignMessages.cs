using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Taskly.Domain.Entities
{
    public class CampaignMessages 
    {
        [Key]
        public int Id { get; set; }

        public int CampaignSequenceId { get; set; }

        public int WaitTimeInMinutes { get; set; }

        public bool MessageRotation { get; set; } = false; // Indicates if message rotation is enabled

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }
}
