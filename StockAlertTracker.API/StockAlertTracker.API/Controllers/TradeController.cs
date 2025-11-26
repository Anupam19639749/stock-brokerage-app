using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StockAlertTracker.API.DTOs.Trade;
using StockAlertTracker.API.Interfaces.Services;
using StockAlertTracker.API.Models.Enums;
using System.Security.Claims;

namespace StockAlertTracker.API.Controllers
{
    [Authorize(Roles = "User")]
    [ApiController]
    [Route("api/[controller]")]
    public class TradeController : ControllerBase
    {
        private readonly ITradeService _tradeService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public TradeController(ITradeService tradeService, IHttpContextAccessor httpContextAccessor)
        {
            _tradeService = tradeService;
            _httpContextAccessor = httpContextAccessor;
        }

        private int GetUserIdFromToken()
        {
            return int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        }

        [HttpPost("order")]
        public async Task<IActionResult> PlaceOrder([FromForm] OrderRequestDto orderRequest)
        {
            var userId = GetUserIdFromToken();
            var response = await _tradeService.PlaceOrderAsync(userId, orderRequest);

            if (!response.Success)
            {
                // Return 400 Bad Request for user errors (e.g., insufficient funds)
                return BadRequest(response);
            }

            return Ok(response);
        }
    }
}