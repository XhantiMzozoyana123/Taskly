using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Mail;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Taskly.Application.Constants;
using Taskly.Application.Dtos;
using Taskly.Application.Interfaces;
using Taskly.Domain;

namespace ApplySmart.Infrastructure.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IConfiguration _configuration;
        private readonly ApplicationDbContext _context;
        private readonly EmailSettingDto _emailSettings;

        private const string SubscriptionClaimType = "SubscriptionLevel";

        public AuthService(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IConfiguration configuration,
            ApplicationDbContext context,
            EmailSettingDto emailSettings)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
            _context = context;
            _emailSettings = emailSettings;
        }

        // ------------------ REGISTER ------------------
        public async Task<string> RegisterAsync(RegisterDto model)
        {
            // Inside RegisterAsync
            var user = new ApplicationUser
            {
                UserName = model.UserName.Replace(" ", "_"),
                Email = model.Email,
                SubscriptionTier = SubscriptionConstants.Free,
                TwoFactorEnabled = true // Explicitly enable 2FA
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
                throw new Exception(string.Join(", ", result.Errors.Select(e => e.Description)));

            // Add subscription claim
            await _userManager.AddClaimAsync(user, new Claim(SubscriptionClaimType, user.SubscriptionTier));

            return "User registered successfully";
        }

        // ------------------ LOGIN ------------------
        public async Task<(string token, string userId, bool requires2FA)> LoginAsync(LoginDto model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null) throw new Exception("Invalid login attempt.");

            // Check password
            var result = await _signInManager.CheckPasswordSignInAsync(user, model.Password, false);

            if (!result.Succeeded)
                throw new Exception("Invalid login attempt.");

            // If 2FA is enabled, require verification before issuing JWT
            if (user.TwoFactorEnabled)
            {
                return (null, user.Id, true); // Indicate 2FA is required
            }

            // Generate JWT token
            var token = await GenerateJwtTokenAsync(user);
            return (token, user.Id, false);
        }


        // ------------------ GENERATE JWT TOKEN ------------------
        private async Task<string> GenerateJwtTokenAsync(ApplicationUser user)
        {
            var baseClaims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(JwtRegisteredClaimNames.Sub, user.UserName),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var identityClaims = await _userManager.GetClaimsAsync(user);
            baseClaims.AddRange(identityClaims);

            // Ensure subscription claim exists
            if (!identityClaims.Any(c => c.Type == SubscriptionClaimType))
                baseClaims.Add(new Claim(SubscriptionClaimType, user.SubscriptionTier ?? SubscriptionConstants.Free));

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: baseClaims,
                expires: DateTime.Now.AddMinutes(30),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        // ------------------ SET USER SUBSCRIPTION ------------------
        public async Task SetUserSubscriptionAsync(string userId, string subscriptionTier)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) throw new Exception("User not found.");

            user.SubscriptionTier = subscriptionTier;
            await _userManager.UpdateAsync(user);

            var claims = await _userManager.GetClaimsAsync(user);
            var existingClaim = claims.FirstOrDefault(c => c.Type == SubscriptionClaimType);
            if (existingClaim != null)
            {
                await _userManager.RemoveClaimAsync(user, existingClaim);
            }

            await _userManager.AddClaimAsync(user, new Claim(SubscriptionClaimType, subscriptionTier));
        }

        // ------------------ GET USER SUBSCRIPTION ------------------
        public async Task<string?> GetUserSubscriptionAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            return user?.SubscriptionTier;
        }

        // ------------------ UPDATE USER ------------------
        public async Task<string> UpdateUserAsync(UserDto userUpdateDto)
        {
            var user = await _userManager.FindByIdAsync(userUpdateDto.Id);
            if (user == null) return "User not found.";

            var passwordCheck = await _signInManager.CheckPasswordSignInAsync(user, userUpdateDto.CurrentPassword, false);
            if (!passwordCheck.Succeeded) return "Current password is incorrect.";

            if (user.Email != userUpdateDto.NewEmail)
            {
                var emailUpdateResult = await _userManager.SetEmailAsync(user, userUpdateDto.NewEmail);
                if (!emailUpdateResult.Succeeded) return string.Join(", ", emailUpdateResult.Errors.Select(e => e.Description));
            }

            if (!string.IsNullOrWhiteSpace(userUpdateDto.NewPassword))
            {
                var passwordChangeResult = await _userManager.ChangePasswordAsync(user, userUpdateDto.CurrentPassword, userUpdateDto.NewPassword);
                if (!passwordChangeResult.Succeeded) return string.Join(", ", passwordChangeResult.Errors.Select(e => e.Description));
            }

            await _signInManager.RefreshSignInAsync(user);
            return "User details updated successfully.";
        }

        // ------------------ GET USER BY ID ------------------
        public async Task<UserDto> GetUserByIdAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return null;

            return new UserDto
            {
                Id = user.Id,
                UserName = user.UserName,
                NewEmail = user.Email,
                SubscriptionTier = user.SubscriptionTier
            };
        }

        // ------------------ TWO FACTOR ------------------
        public async Task<string> GenerateTwoFactorTokenAsync(ApplicationUser user)
            => await _userManager.GenerateTwoFactorTokenAsync(user, TokenOptions.DefaultEmailProvider);

        public async Task<bool> VerifyTwoFactorTokenAsync(ApplicationUser user, string token)
            => await _userManager.VerifyTwoFactorTokenAsync(user, TokenOptions.DefaultEmailProvider, token);

        public async Task SendTwoFactorCodeAsync(ApplicationUser user)
        {
            var token = await _userManager.GenerateTwoFactorTokenAsync(user, TokenOptions.DefaultEmailProvider);
            await SendEmailAsync(user.Email, "Your 2FA Code", $"Your 2FA code is: {token}");
        }

        public async Task<string> VerifyTwoFactorCodeAsync(string userId, string token)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) throw new Exception("User not found.");

            var isValid = await _userManager.VerifyTwoFactorTokenAsync(user, TokenOptions.DefaultEmailProvider, token);
            if (!isValid) throw new Exception("Invalid 2FA token.");

            // 2FA passed, generate JWT
            return await GenerateJwtTokenAsync(user);
        }

        // ------------------ PASSWORD RESET ------------------
        public async Task<string> ForgotPasswordAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null) throw new Exception("User not found.");

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var resetLink = $"{_configuration["App:Url"]}/reset-password?token={token}&id={user.Id}";

            await SendEmailAsync(user.Email, "Password Reset", $"Click here to reset your password: {resetLink}");
            return "Password reset email sent.";
        }

        public async Task<string> ResetPasswordAsync(ResetPasswordDto model)
        {
            var user = await _userManager.FindByIdAsync(model.Id);
            if (user == null) throw new Exception("User not found.");

            var result = await _userManager.ResetPasswordAsync(user, model.Token, model.NewPassword);
            if (result.Succeeded) return "Password reset successfully.";

            throw new Exception(string.Join(", ", result.Errors.Select(e => e.Description)));
        }

        // ------------------ DELETE USER ------------------
        public async Task<bool> DeleteUserAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return false;

            var result = await _userManager.DeleteAsync(user);
            await _context.SaveChangesAsync();

            return result.Succeeded;
        }

        // ------------------ EMAIL SENDING ------------------
        public async Task SendEmailAsync(string email, string subject, string message)
        {
            using (MailMessage mail = new MailMessage())
            {
                mail.From = new MailAddress(_emailSettings.FromEmail, _emailSettings.FromName);
                mail.To.Add(email);
                mail.Subject = subject;
                mail.Body = message; // you can still use AppConstants.ConvertStringToHtml(message, 0, false);
                mail.IsBodyHtml = true;

                using (SmtpClient smtpServer = new SmtpClient(_emailSettings.SmtpHost, _emailSettings.SmtpPort))
                {
                    smtpServer.Credentials = new System.Net.NetworkCredential(_emailSettings.FromEmail, _emailSettings.Password);
                    smtpServer.EnableSsl = _emailSettings.EnableSsl;
                    smtpServer.Send(mail);
                }
            }
        }
    }
}
