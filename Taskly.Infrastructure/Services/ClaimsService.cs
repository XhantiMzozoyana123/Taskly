using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Taskly.Application.Constants;
using Taskly.Application.Interfaces;

namespace Taskly.Infrastructure.Services
{
    public class ClaimsService : IClaimsService
    {
        private const string SubscriptionClaimType = SubscriptionConstants.Type;

        // Update or add subscription claim for a logged-in user (Cookie-based)
        public async Task SetSubscriptionAsync(ClaimsPrincipal user, string subscriptionTier)
        {
            if (user.Identity is not ClaimsIdentity identity) return;

            // Remove existing subscription claim
            var existingClaim = identity.FindFirst(SubscriptionClaimType);
            if (existingClaim != null)
                identity.RemoveClaim(existingClaim);

            // Add new subscription claim
            identity.AddClaim(new Claim(SubscriptionClaimType, subscriptionTier));

            // Update cookie
            await user.AuthenticationUpdateAsync(identity);
        }

        // Retrieve subscription from claims
        public string? GetSubscription(ClaimsPrincipal user)
        {
            return user.Claims.FirstOrDefault(c => c.Type == SubscriptionClaimType)?.Value;
        }
    }

    // Extension to update the cookie after changing claims
    public static class ClaimsPrincipalExtensions
    {
        public static async Task AuthenticationUpdateAsync(this ClaimsPrincipal user, ClaimsIdentity newIdentity)
        {
            var httpContext = new HttpContextAccessor().HttpContext;
            if (httpContext != null)
            {
                var principal = new ClaimsPrincipal(newIdentity);
                await httpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);
            }
        }
    }
}
