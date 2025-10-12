using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Taskly.Application.Constants;

namespace Taskly.Application.Dtos
{
    public class UserDto
    {
        public string? Id { get; set; }

        [Required]
        public string UserName { get; set; }

        [Required]
        [EmailAddress]
        public string NewEmail { get; set; }

        [Required]
        public string CurrentPassword { get; set; }

        [Required]
        public string NewPassword { get; set; }

        public string SubscriptionTier { get; set; } = SubscriptionConstants.Free;
    }
}
