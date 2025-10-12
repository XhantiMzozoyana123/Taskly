using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Taskly.Application.Dtos;
using Taskly.Domain;

namespace Taskly.Application.Interfaces
{
    public interface IAuthService
    {
        // ------------------ USER AUTH ------------------
        Task<string> RegisterAsync(RegisterDto model);
        Task<(string token, string userId, bool requires2FA)> LoginAsync(LoginDto model);
        Task<string> UpdateUserAsync(UserDto userUpdateDto);
        Task<UserDto> GetUserByIdAsync(string userId);

        // ------------------ TWO-FACTOR ------------------
        Task<string> GenerateTwoFactorTokenAsync(ApplicationUser user);
        Task<bool> VerifyTwoFactorTokenAsync(ApplicationUser user, string token);
        Task SendTwoFactorCodeAsync(ApplicationUser user);
        Task<string> VerifyTwoFactorCodeAsync(string userId, string token);

        // ------------------ EMAIL ------------------
        Task SendEmailAsync(string email, string subject, string message);

        // ------------------ PASSWORD ------------------
        Task<string> ForgotPasswordAsync(string email);
        Task<string> ResetPasswordAsync(ResetPasswordDto model);

        // ------------------ USER DELETION ------------------
        Task<bool> DeleteUserAsync(string userId);

        // ------------------ SUBSCRIPTION ------------------
        Task SetUserSubscriptionAsync(string userId, string subscriptionTier);
        Task<string?> GetUserSubscriptionAsync(string userId);
    }
}
