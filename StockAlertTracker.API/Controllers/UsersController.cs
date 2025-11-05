using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StockAlertTracker.API.DTOs.User;
using StockAlertTracker.API.Interfaces.Services;
using System.Security.Claims;

namespace StockAlertTracker.API.Controllers
{
    [Authorize(Roles = "User")] // Only authenticated Users can access this
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UsersController(IUserService userService, IHttpContextAccessor httpContextAccessor)
        {
            _userService = userService;
            _httpContextAccessor = httpContextAccessor;
        }

        // Helper to get the logged-in user's ID from the JWT token
        private int GetUserIdFromToken()
        {
            return int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        }

        [HttpGet("me")]
        public async Task<IActionResult> GetMyProfile()
        {
            var userId = GetUserIdFromToken();
            var response = await _userService.GetUserDetailsAsync(userId);
            if (!response.Success)
            {
                return NotFound(response);
            }
            return Ok(response);
        }

        [HttpPut("profile")]
        public async Task<IActionResult> UpdateMyProfile(ProfileUpdateDto profileDto)
        {
            var userId = GetUserIdFromToken();
            var response = await _userService.UpdateProfileAsync(userId, profileDto);
            if (!response.Success)
            {
                return NotFound(response);
            }
            return Ok(response);
        }

        [HttpPost("kyc")]
        public async Task<IActionResult> SubmitKyc(KycSubmitDto kycDto)
        {
            var userId = GetUserIdFromToken();
            var response = await _userService.SubmitKycAsync(userId, kycDto);
            if (!response.Success)
            {
                return BadRequest(response);
            }
            return Ok(response);
        }

        [HttpPost("profile/image")]
        public async Task<IActionResult> UploadProfileImage(IFormFile file)
        {
            var userId = GetUserIdFromToken();

            if (file == null || file.Length == 0)
            {
                return BadRequest(new { Success = false, Message = "No file uploaded." });
            }

            // Convert IFormFile to byte[] (this is the "dirty" HTTP part)
            byte[] imageBytes;
            using (var memoryStream = new MemoryStream())
            {
                await file.CopyToAsync(memoryStream);
                imageBytes = memoryStream.ToArray();
            }

            // Call the service with the "clean" byte array
            var response = await _userService.UpdateProfileImageAsync(userId, imageBytes);
            if (!response.Success)
            {
                return BadRequest(response);
            }

            return Ok(response);
        }
    }
}