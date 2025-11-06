using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StockAlertTracker.API.DTOs.Alert;
using StockAlertTracker.API.Interfaces.Services;
using System.Security.Claims;

namespace StockAlertTracker.API.Controllers
{
    [Authorize(Roles = "User")]
    [ApiController]
    [Route("api/[controller]")]
    public class AlertController : ControllerBase
    {
        private readonly IAlertService _alertService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AlertController(IAlertService alertService, IHttpContextAccessor httpContextAccessor)
        {
            _alertService = alertService;
            _httpContextAccessor = httpContextAccessor;
        }

        private int GetUserIdFromToken()
        {
            return int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        }

        [HttpPost]
        public async Task<IActionResult> CreateAlert(AlertCreateDto alertDto)
        {
            var userId = GetUserIdFromToken();
            var response = await _alertService.CreateAlertAsync(userId, alertDto);

            if (!response.Success)
            {
                return BadRequest(response); // e.g., "Alert already exists"
            }

            return Ok(response);
        }

        [HttpGet("my-alerts")]
        public async Task<IActionResult> GetMyAlerts()
        {
            var userId = GetUserIdFromToken();
            var response = await _alertService.GetMyAlertsAsync(userId);
            return Ok(response);
        }

        [HttpDelete("{alertId}")]
        public async Task<IActionResult> DeleteAlert(int alertId)
        {
            var userId = GetUserIdFromToken();
            var response = await _alertService.DeleteAlertAsync(userId, alertId);

            if (!response.Success)
            {
                // Could be 404 Not Found or 403 Forbidden
                return NotFound(response);
            }

            return Ok(response);
        }
    }
}