using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Taskly.Domain.Entities
{
    public class Messages : BaseEntity
    {
        public int LeadId { get; set; }
        
        public int iceBreakerId { get; set; }
        
        public string Text { get; set; } = string.Empty; 

        public string Status { get; set; } = string.Empty; // e.g., "New", "Sent", "Delivered", "Read"
    }
}
