using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StockAlertTracker.API.Interfaces.Repositories;
using StockAlertTracker.API.Interfaces.Services;
using StockAlertTracker.API.Repositories;

namespace StockAlertTracker.API.Controllers
{
    [Authorize(Roles = "Admin")] // ONLY Admins can access this
    [ApiController]
    [Route("api/[controller]")]
    public class AdminController : ControllerBase
    {
        private readonly IAdminService _adminService;
        private readonly IUnitOfWork _unitOfWork;

        public AdminController(IAdminService adminService, IUnitOfWork unitOfWork)
        {
            _adminService = adminService;
            _unitOfWork = unitOfWork;
        }

        [HttpGet("kyc-requests")]
        public async Task<IActionResult> GetPendingKycRequests()
        {
            var response = await _adminService.GetKycRequestsAsync();
            return Ok(response);
        }

        [HttpPost("kyc/approve/{userId}")]
        public async Task<IActionResult> ApproveKyc(int userId)
        {
            var response = await _adminService.ApproveKycAsync(userId);
            if (!response.Success)
            {
                return NotFound(response);
            }
            return Ok(response);
        }

        [HttpPost("kyc/reject/{userId}")]
        public async Task<IActionResult> RejectKyc(int userId)
        {
            var response = await _adminService.RejectKycAsync(userId);
            if (!response.Success)
            {
                return NotFound(response);
            }
            return Ok(response);
        }

        [HttpGet("orders/pending")]
        public async Task<IActionResult> GetPendingOrders()
        {
            var response = await _adminService.GetPendingOrdersAsync();
            return Ok(response);
        }

        [HttpPost("orders/approve/{orderId}")]
        public async Task<IActionResult> ApproveOrder(int orderId)
        {
            var response = await _adminService.ApproveOrderAsync(orderId);
            if (!response.Success)
            {
                return BadRequest(response); // 400 Bad Request if order isn't valid
            }
            return Ok(response);
        }

        [HttpPost("orders/reject/{orderId}")]
        public async Task<IActionResult> RejectOrder(int orderId)
        {
            var response = await _adminService.RejectOrderAsync(orderId);
            if (!response.Success)
            {
                return BadRequest(response);
            }
            return Ok(response);
        }

        [HttpGet("stats")]
        public async Task<IActionResult> GetPlatformStats()
        {
            var response = await _adminService.GetPlatformStatsAsync();
            return Ok(response);
        }

        [HttpGet("users")]
        public async Task<IActionResult> GetAllUsers()
        {
            var response = await _adminService.GetAllUsersAsync();
            return Ok(response);
        }

        [HttpGet("users/{userId}")]
        public async Task<IActionResult> GetUserById(int userId)
        {
            var response = await _adminService.GetUserByIdAsync(userId);
            if (!response.Success)
            {
                return NotFound(response);
            }
            return Ok(response);
        }

        [HttpPost("users/{userId}/block")]
        public async Task<IActionResult> BlockUser(int userId)
        {
            var response = await _adminService.BlockUserAsync(userId);
            if (!response.Success)
            {
                return BadRequest(response);
            }
            return Ok(response);
        }

        [HttpPost("users/{userId}/unblock")]
        public async Task<IActionResult> UnblockUser(int userId)
        {
            var response = await _adminService.UnblockUserAsync(userId);
            if (!response.Success)
            {
                return BadRequest(response);
            }
            return Ok(response);
        }

        [HttpGet("users/{userId}/image")]
        public async Task<IActionResult> GetUserProfileImage(int userId)
        {
            var userFromDb = await _unitOfWork.Users.GetByIdAsync(userId); // Need to inject IUnitOfWork

            if (userFromDb?.ProfileImage == null || userFromDb.ProfileImage.Length == 0)
            {
                return NotFound(new { Success = false, Message = "No profile image found." });
            }

            var contentType = userFromDb.ProfileImageContentType ?? "application/octet-stream";
            return File(userFromDb.ProfileImage, contentType);
        }
    }
}