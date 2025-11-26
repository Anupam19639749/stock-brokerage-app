using System.Collections.Generic;

namespace StockAlertTracker.API.DTOs.Admin
{
    public class AdminStatsDto
    {
        public int TotalUsers { get; set; }
        public int ActiveUsers { get; set; }
        public List<StockStatDto> TopHeldStocks { get; set; } // <-- RENAMED
        public List<StockStatDto> TopAlertedStocks { get; set; }
    }

    public class StockStatDto
    {
        public string Ticker { get; set; }
        public int Count { get; set; }
    }
}