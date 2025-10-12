using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Taskly.Application.Dtos;
using Taskly.Application.Interfaces;
using Taskly.Domain;

namespace Taskly.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        // ------------------ REGISTER ------------------
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto model)
        {
            var result = await _authService.RegisterAsync(model);
            return Ok(new { message = result });
        }

        // ------------------ LOGIN ------------------
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto model)
        {
            var (token, userId, requires2FA) = await _authService.LoginAsync(model);

            return Ok(new
            {
                token,
                userId,
                requires2FA
            });
        }

        // ------------------ VERIFY 2FA ------------------
        [HttpPost("verify-2fa")]
        public async Task<IActionResult> VerifyTwoFactor([FromBody] TwoFactorDto model)
        {
            var token = await _authService.VerifyTwoFactorCodeAsync(model.UserId, model.Code);
            return Ok(new { token });
        }

        // ------------------ SEND 2FA ------------------
        [HttpPost("send-2fa")]
        public async Task<IActionResult> SendTwoFactor([FromBody] SendTwoFactorDto model)
        {
            var user = await _authService.GetUserByIdAsync(model.UserId);
            if (user == null) return NotFound("User not found.");

            await _authService.SendTwoFactorCodeAsync(new ApplicationUser { Id = user.Id, Email = user.NewEmail });
            return Ok(new { message = "2FA code sent." });
        }

        // ------------------ GET USER ------------------
        [Authorize]
        [HttpGet("{userId}")]
        public async Task<IActionResult> GetUser(string userId)
        {
            var user = await _authService.GetUserByIdAsync(userId);
            if (user == null) return NotFound("User not found.");
            return Ok(user);
        }

        // ------------------ UPDATE USER ------------------
        [Authorize]
        [HttpPut("update")]
        public async Task<IActionResult> UpdateUser([FromBody] UserDto model)
        {
            var result = await _authService.UpdateUserAsync(model);
            return Ok(new { message = result });
        }

        // ------------------ SET SUBSCRIPTION ------------------
        [Authorize]
        [HttpPut("{userId}/subscription")]
        public async Task<IActionResult> SetSubscription(string userId, [FromBody] SubscriptionDto model)
        {
            await _authService.SetUserSubscriptionAsync(userId, model.SubscriptionTier);
            return Ok(new { message = "Subscription updated." });
        }

        // ------------------ GET SUBSCRIPTION ------------------
        [Authorize]
        [HttpGet("{userId}/subscription")]
        public async Task<IActionResult> GetSubscription(string userId)
        {
            var subscription = await _authService.GetUserSubscriptionAsync(userId);
            return Ok(new { subscription });
        }

        // ------------------ FORGOT PASSWORD ------------------
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto model)
        {
            var result = await _authService.ForgotPasswordAsync(model.Email);
            return Ok(new { message = result });
        }

        // ------------------ RESET PASSWORD ------------------
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto model)
        {
            var result = await _authService.ResetPasswordAsync(model);
            return Ok(new { message = result });
        }

        // ------------------ DELETE USER ------------------
        [Authorize]
        [HttpDelete("{userId}")]
        public async Task<IActionResult> DeleteUser(string userId)
        {
            var result = await _authService.DeleteUserAsync(userId);
            if (!result) return NotFound("User not found.");
            return Ok(new { message = "User deleted successfully." });
        }
    }
}
