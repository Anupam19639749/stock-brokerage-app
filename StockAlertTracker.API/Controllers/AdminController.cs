using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StockAlertTracker.API.Interfaces.Services;

namespace StockAlertTracker.API.Controllers
{
    [Authorize(Roles = "Admin")] // ONLY Admins can access this
    [ApiController]
    [Route("api/[controller]")]
    public class AdminController : ControllerBase
    {
        private readonly IAdminService _adminService;

        public AdminController(IAdminService adminService)
        {
            _adminService = adminService;
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
    }
}