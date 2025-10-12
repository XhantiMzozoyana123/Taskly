using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Taskly.Domain.Entities
{
    public class Leads : BaseEntity
    {
        public int CampaignId { get; set; }

        public string Name { get; set; } = string.Empty;
        
        public string ProfileUrl { get; set; } = string.Empty;
        
        public string PostDescription { get; set; } = string.Empty;

        public string PostUrl { get; set; } = string.Empty;

        public string Platform { get; set; } = string.Empty;
        
        public string Keywords { get; set; } = string.Empty;

        public string Query { get; set; } = string.Empty;

        public string Status { get; set; } = string.Empty; // e.g., "New", "Contacted", "Qualified", "Converted"

        public DateTime PostDate { get; set; }
    }
}
