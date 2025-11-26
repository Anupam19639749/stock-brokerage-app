using Microsoft.AspNetCore.Mvc;
using StockAlertTracker.API.DTOs.Auth;
using StockAlertTracker.API.Interfaces.Services;

namespace StockAlertTracker.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromForm] RegisterUserDto registerDto)
        {
            var response = await _authService.RegisterAsync(registerDto);
            if (!response.Success)
            {
                return BadRequest(response);
            }
            return Ok(response);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromForm] LoginDto loginDto)
        {
            var response = await _authService.LoginAsync(loginDto);
            if (!response.Success)
            {
                return Unauthorized(response);
            }

            // We set the secure HttpOnly cookie for our React app
            Response.Cookies.Append("token", response.Data.Token, new CookieOptions
            {
                HttpOnly = true, // JavaScript can't read it
                Secure = true,   // Only sent over HTTPS
                SameSite = SameSiteMode.None, // Protects against CSRF
                Expires = DateTime.UtcNow.AddDays(7)
            });
            return Ok(response);
        }

        [HttpPost("logout")]
        public IActionResult Logout()
        {
            // Clear the HttpOnly cookie
            Response.Cookies.Delete("token", new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None
            });
            return Ok(new { Success = true, Message = "Logged out successfully." });
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromForm] ForgotPasswordDto forgotPasswordDto)
        {
            var response = await _authService.ForgotPasswordAsync(forgotPasswordDto.Email);
            return Ok(response); // Always return OK to prevent email snooping
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromForm] ResetPasswordDto resetDto)
        {
            var response = await _authService.ResetPasswordAsync(resetDto);
            if (!response.Success)
            {
                return BadRequest(response);
            }
            return Ok(response);
        }
    }
}