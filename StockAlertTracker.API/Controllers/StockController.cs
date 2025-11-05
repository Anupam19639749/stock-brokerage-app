using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StockAlertTracker.API.Interfaces.Services;

namespace StockAlertTracker.API.Controllers
{
    [Authorize] // Any logged-in user (User or Admin) can access this
    [ApiController]
    [Route("api/[controller]")]
    public class StockController : ControllerBase
    {
        private readonly IStockDataService _stockDataService;

        public StockController(IStockDataService stockDataService)
        {
            _stockDataService = stockDataService;
        }

        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return BadRequest(new { Success = false, Message = "Query cannot be empty." });
            }

            var response = await _stockDataService.SearchStockAsync(query);
            if (!response.Success)
            {
                return StatusCode(500, response);
            }
            return Ok(response);
        }

        [HttpGet("quote/{ticker}")]
        public async Task<IActionResult> GetQuote(string ticker)
        {
            if (string.IsNullOrWhiteSpace(ticker))
            {
                return BadRequest(new { Success = false, Message = "Ticker cannot be empty." });
            }

            var response = await _stockDataService.GetLiveQuoteAsync(ticker);
            if (!response.Success)
            {
                return NotFound(response); // 404 if the ticker isn't found
            }
            return Ok(response);
        }
    }
}