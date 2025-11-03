using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Taskly.Domain.Entities
{
    public class CookieFiles : BaseEntity
    {
        public string FileName { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
    }
}
