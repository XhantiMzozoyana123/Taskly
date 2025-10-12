using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Taskly.Application.Dtos;
using Taskly.Application.Interfaces;
using Taskly.Domain;

namespace Taskly.Controllers
{
    /// <summary>
    /// Controller for handling authentication and user management operations.
    /// </summary>
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
        /// <summary>
        /// Registers a new user.
        /// </summary>
        /// <param name="model">
        /// Used model: RegisterDto
        /// Properties:
        /// - UserName (string): The desired username.
        /// - Email (string): The user's email address.
        /// - Password (string): The user's password.
        /// </param>
        /// <returns>A message indicating the result of the registration.</returns>
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto model)
        {
            var result = await _authService.RegisterAsync(model);
            return Ok(new { message = result });
        }

        // ------------------ LOGIN ------------------
        /// <summary>
        /// Logs in an existing user.
        /// </summary>
        /// <param name="model">
        /// Used model: LoginDto
        /// Properties:
        /// - Email (string): The user's email address.
        /// - Password (string): The user's password.
        /// </param>
        /// <returns>A JWT token, user ID, and a flag indicating if 2FA is required.</returns>
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
        /// <summary>
        /// Verifies a two-factor authentication code.
        /// </summary>
        /// <param name="model">
        /// Used model: TwoFactorDto
        /// Properties:
        /// - UserId (string): The ID of the user.
        /// - Code (string): The 2FA code entered by the user.
        /// </param>
        /// <returns>A JWT token if 2FA is successfully verified.</returns>
        [HttpPost("verify-2fa")]
        public async Task<IActionResult> VerifyTwoFactor([FromBody] TwoFactorDto model)
        {
            var token = await _authService.VerifyTwoFactorCodeAsync(model.UserId, model.Code);
            return Ok(new { token });
        }

        // ------------------ SEND 2FA ------------------
        /// <summary>
        /// Sends a two-factor authentication code to the user.
        /// </summary>
        /// <param name="model">
        /// Used model: SendTwoFactorDto
        /// Properties:
        /// - UserId (string): The ID of the user to send the 2FA code to.
        /// </param>
        /// <returns>A message indicating that the 2FA code has been sent.</returns>
        [HttpPost("send-2fa")]
        public async Task<IActionResult> SendTwoFactor([FromBody] SendTwoFactorDto model)
        {
            var user = await _authService.GetUserByIdAsync(model.UserId);
            if (user == null) return NotFound("User not found.");

            // Used model: ApplicationUser
            // Properties:
            // - Id (string): The user's ID.
            // - Email (string): The user's email address.
            // - SubscriptionTier (string): The user's subscription tier (default "Free").
            await _authService.SendTwoFactorCodeAsync(new ApplicationUser { Id = user.Id, Email = user.NewEmail });
            return Ok(new { message = "2FA code sent." });
        }

        // ------------------ GET USER ------------------
        /// <summary>
        /// Retrieves user details by ID.
        /// </summary>
        /// <param name="userId">The ID of the user to retrieve.</param>
        /// <returns>The user's details.</returns>
        [Authorize]
        [HttpGet("{userId}")]
        public async Task<IActionResult> GetUser(string userId)
        {
            var user = await _authService.GetUserByIdAsync(userId);
            if (user == null) return NotFound("User not found.");
            return Ok(user);
        }

        // ------------------ UPDATE USER ------------------
        /// <summary>
        /// Updates an existing user's information.
        /// </summary>
        /// <param name="model">
        /// Used model: UserDto
        /// Properties:
        /// - Id (string, optional): The user's ID.
        /// - UserName (string): The user's new username.
        /// - NewEmail (string): The user's new email address.
        /// - CurrentPassword (string): The user's current password for verification.
        /// - NewPassword (string): The user's new password.
        /// - SubscriptionTier (string): The user's subscription tier (default "Free").
        /// </param>
        /// <returns>A message indicating the result of the update operation.</returns>
        [Authorize]
        [HttpPut("update")]
        public async Task<IActionResult> UpdateUser([FromBody] UserDto model)
        {
            var result = await _authService.UpdateUserAsync(model);
            return Ok(new { message = result });
        }

        // ------------------ SET SUBSCRIPTION ------------------
        /// <summary>
        /// Sets the subscription tier for a user.
        /// </summary>
        /// <param name="userId">The ID of the user.</param>
        /// <param name="model">
        /// Used model: SubscriptionDto
        /// Properties:
        /// - SubscriptionTier (string): The new subscription tier.
        /// </param>
        /// <returns>A message indicating that the subscription has been updated.</returns>
        [Authorize]
        [HttpPut("{userId}/subscription")]
        public async Task<IActionResult> SetSubscription(string userId, [FromBody] SubscriptionDto model)
        {
            await _authService.SetUserSubscriptionAsync(userId, model.SubscriptionTier);
            return Ok(new { message = "Subscription updated." });
        }

        // ------------------ GET SUBSCRIPTION ------------------
        /// <summary>
        /// Retrieves the subscription tier for a user.
        /// </summary>
        /// <param name="userId">The ID of the user.</param>
        /// <returns>The user's subscription tier.</returns>
        [Authorize]
        [HttpGet("{userId}/subscription")]
        public async Task<IActionResult> GetSubscription(string userId)
        {
            var subscription = await _authService.GetUserSubscriptionAsync(userId);
            return Ok(new { subscription });
        }

        // ------------------ FORGOT PASSWORD ------------------
        /// <summary>
        /// Initiates the forgot password process.
        /// </summary>
        /// <param name="model">
        /// Used model: ForgotPasswordDto
        /// Properties:
        /// - Email (string): The email address of the user who forgot their password.
        /// </param>
        /// <returns>A message indicating the result of the forgot password request.</returns>
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto model)
        {
            var result = await _authService.ForgotPasswordAsync(model.Email);
            return Ok(new { message = result });
        }

        // ------------------ RESET PASSWORD ------------------
        /// <summary>
        /// Resets the user's password using a token.
        /// </summary>
        /// <param name="model">
        /// Used model: ResetPasswordDto
        /// Properties:
        /// - Id (string): The user's ID.
        /// - Token (string): The password reset token.
        /// - NewPassword (string): The new password.
        /// </param>
        /// <returns>A message indicating the result of the password reset operation.</returns>
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto model)
        {
            var result = await _authService.ResetPasswordAsync(model);
            return Ok(new { message = result });
        }

        // ------------------ DELETE USER ------------------
        /// <summary>
        /// Deletes a user account.
        /// </summary>
        /// <param name="userId">The ID of the user to delete.</param>
        /// <returns>A message indicating the result of the deletion operation.</returns>
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
