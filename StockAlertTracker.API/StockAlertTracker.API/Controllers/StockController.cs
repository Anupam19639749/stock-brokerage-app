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

        [HttpGet("indices")]
        public async Task<IActionResult> GetMarketIndices()
        {
            var response = await _stockDataService.GetMarketIndicesAsync();
            return Ok(response);
        }

        [HttpGet("market-overview")]
        public async Task<IActionResult> GetMarketOverview([FromQuery] int page = 1, [FromQuery] int limit = 6)
        {
            if (page < 1) page = 1;
            if (limit < 1) limit = 6;

            var response = await _stockDataService.GetMarketOverviewAsync(page, limit);
            return Ok(response);
        }

        [HttpGet("profile/{ticker}")]
        public async Task<IActionResult> GetCompanyProfile(string ticker)
        {
            var response = await _stockDataService.GetCompanyProfileAsync(ticker);
            if (!response.Success)
            {
                return NotFound(response);
            }
            return Ok(response);
        }
    }
}

