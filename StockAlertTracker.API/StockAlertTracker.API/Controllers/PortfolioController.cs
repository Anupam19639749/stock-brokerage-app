using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StockAlertTracker.API.Interfaces.Services;
using System.Security.Claims;

namespace StockAlertTracker.API.Controllers
{
    [Authorize(Roles = "User")]
    [ApiController]
    [Route("api/[controller]")]
    public class PortfolioController : ControllerBase
    {
        private readonly IPortfolioService _portfolioService;
        private readonly ITradeService _tradeService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public PortfolioController(IPortfolioService portfolioService, ITradeService tradeService, IHttpContextAccessor httpContextAccessor)
        {
            _portfolioService = portfolioService;
            _tradeService = tradeService;
            _httpContextAccessor = httpContextAccessor;
        }

        private int GetUserIdFromToken()
        {
            return int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        }

        [HttpGet]
        public async Task<IActionResult> GetMyPortfolio()
        {
            var userId = GetUserIdFromToken();
            var response = await _portfolioService.GetPortfolioAsync(userId);
            return Ok(response);
        }

        [HttpGet("orders")]
        public async Task<IActionResult> GetMyOrders()
        {
            var userId = GetUserIdFromToken();
            var response = await _tradeService.GetMyOrdersAsync(userId);
            return Ok(response);
        }
    }
}