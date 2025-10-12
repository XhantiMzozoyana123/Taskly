using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Taskly.Domain
{
    public class ApplicationUser : IdentityUser
    {
        public string SubscriptionTier { get; set; } = "Free";
    }
}
