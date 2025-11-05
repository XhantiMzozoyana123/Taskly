using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Taskly.Domain.Entities
{
    public class CustomMessages : BaseEntity
    {
        public int LeadId { get; set; }

        public string Text { get; set; } = string.Empty;
    }
}
