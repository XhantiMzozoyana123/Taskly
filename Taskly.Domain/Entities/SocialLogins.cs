using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Taskly.Domain.Entities
{
    public class SocialLogins : BaseEntity
    {
        public string Username { get; set; } = string.Empty;

        public string Password { get; set; } = string.Empty;

        public string Platform { get; set; } = string.Empty; // e.g., "Reddit", "Twitter", "Facebook"
    }
}
