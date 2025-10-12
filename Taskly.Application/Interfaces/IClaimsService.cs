using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Taskly.Application.Interfaces
{
    public interface IClaimsService
    {
        Task SetSubscriptionAsync(ClaimsPrincipal user, string subscriptionTier);
        string? GetSubscription(ClaimsPrincipal user);
    }
}
