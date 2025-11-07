using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StockAlertTracker.API.DTOs.Wallet;
using StockAlertTracker.API.Interfaces.Services;
using System.Security.Claims;

namespace StockAlertTracker.API.Controllers
{
    [Authorize(Roles = "User")] // Only authenticated Users
    [ApiController]
    [Route("api/[controller]")]
    public class WalletController : ControllerBase
    {
        private readonly IWalletService _walletService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public WalletController(IWalletService walletService, IHttpContextAccessor httpContextAccessor)
        {
            _walletService = walletService;
            _httpContextAccessor = httpContextAccessor;
        }

        private int GetUserIdFromToken()
        {
            return int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        }

        [HttpGet("balance")]
        public async Task<IActionResult> GetBalance()
        {
            var userId = GetUserIdFromToken();
            var response = await _walletService.GetWalletBalanceAsync(userId);
            if (!response.Success)
            {
                return NotFound(response);
            }
            return Ok(response);
        }

        [HttpGet("history")]
        public async Task<IActionResult> GetHistory()
        {
            var userId = GetUserIdFromToken();
            var history = await _walletService.GetWalletHistoryAsync(userId);
            return Ok(history);
        }

        [HttpPost("deposit")]
        public async Task<IActionResult> AddMoney([FromForm] AddMoneyRequestDto addMoneyDto)
        {
            var userId = GetUserIdFromToken();
            var response = await _walletService.AddMoneyAsync(userId, addMoneyDto);

            if (!response.Success)
            {
                // We use BadRequest here because it's a failed transaction (e.g., wrong password)
                return BadRequest(response);
            }

            return Ok(response);
        }
    }
}