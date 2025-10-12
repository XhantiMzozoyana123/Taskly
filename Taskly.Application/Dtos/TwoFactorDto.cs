using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Taskly.Application.Dtos
{
    public class TwoFactorDto
    {
        public string UserId { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
    }

    public class SendTwoFactorDto
    {
        public string UserId { get; set; } = string.Empty;
    }

    public class SubscriptionDto
    {
        public string SubscriptionTier { get; set; } = string.Empty;
    }

    public class ForgotPasswordDto
    {
        public string Email { get; set; } = string.Empty;
    }

}
