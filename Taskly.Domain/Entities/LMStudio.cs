using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Taskly.Domain.Entities
{
    public class LMStudio : BaseEntity
    {
        public string Model { get; set; } = string.Empty;

        public string Url { get; set; } = string.Empty;
    }
}
