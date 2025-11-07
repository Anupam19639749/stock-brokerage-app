using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StockAlertTracker.API.DTOs.User;
using StockAlertTracker.API.Interfaces.Repositories;
using StockAlertTracker.API.Interfaces.Services;
using System.Security.Claims;

namespace StockAlertTracker.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IUnitOfWork _unitOfWork;

        public UsersController(IUserService userService, IHttpContextAccessor httpContextAccessor,
                                IUnitOfWork unitOfWork)
        {
            _userService = userService;
            _httpContextAccessor = httpContextAccessor;
            _unitOfWork = unitOfWork;
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
        public async Task<IActionResult> UpdateMyProfile([FromForm] ProfileUpdateDto profileDto)
        {
            var userId = GetUserIdFromToken();
            var response = await _userService.UpdateProfileAsync(userId, profileDto);
            if (!response.Success)
            {
                return NotFound(response);
            }
            return Ok(response);
        }

        [Authorize(Roles = "User")]
        [HttpPost("kyc")]
        public async Task<IActionResult> SubmitKyc([FromForm] KycSubmitDto kycDto)
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

            if (!file.ContentType.StartsWith("image/"))
            {
                return BadRequest(new { Success = false, Message = "Invalid file type. Only images are allowed." });
            }

            // Convert IFormFile to byte[] (this is the "dirty" HTTP part)
            byte[] imageBytes;
            using (var memoryStream = new MemoryStream())
            {
                await file.CopyToAsync(memoryStream);
                imageBytes = memoryStream.ToArray();
            }

            // Call the service with the "clean" byte array
            var response = await _userService.UpdateProfileImageAsync(userId, imageBytes, file.ContentType);
            if (!response.Success)
            {
                return BadRequest(response);
            }

            return Ok(response);
        }

        [HttpGet("my-image")]
        public async Task<IActionResult> GetMyProfileImage()
        {
            var userId = GetUserIdFromToken();

            // We can't use a DTO, we need the raw model for the byte[] and content type
            var userFromDb = await _unitOfWork.Users.GetByIdAsync(userId);

            if (userFromDb?.ProfileImage == null || userFromDb.ProfileImage.Length == 0)
            {
                // Return 404 Not Found if there's no image
                return NotFound(new { Success = false, Message = "No profile image found." });
            }

            // Get the saved content type (e.g., "image/png", "image/jpeg")
            var contentType = userFromDb.ProfileImageContentType ?? "application/octet-stream";

            // Return the image data as a file
            return File(userFromDb.ProfileImage, contentType);
        }
    }
}