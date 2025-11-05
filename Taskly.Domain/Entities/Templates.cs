using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Taskly.Domain.Entities
{
    public class Templates : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
    }
}
