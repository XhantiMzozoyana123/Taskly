using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Taskly.Domain.Entities
{
    public class CampaignSequences : BaseEntity
    {
        public int CampaignId { get; set; }

        public string SequenceName { get; set; } = string.Empty; // Name of the sequence

        public string SequenceDescription { get; set; } = string.Empty; // Description of the sequence

        public int WaitTimeInHours { get; set; } // Wait time before executing this sequence

        public bool AccountRotation { get; set; } = false; // Indicates if account rotation is enabled

        public bool Completed { get; set; } = false; // Indicates if the sequence has been completed
    }
}
