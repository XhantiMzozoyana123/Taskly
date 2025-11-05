using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Taskly.Domain.Entities
{
    public class GoogleAI : BaseEntity
    {
        public string ApiKey { get; set; } = string.Empty;
    }
}
