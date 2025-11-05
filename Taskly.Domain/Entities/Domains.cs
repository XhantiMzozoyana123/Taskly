using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Taskly.Domain.Entities
{
    public class Domains : BaseEntity
    {
        public string Url { get; set; } = string.Empty;
    }
}
