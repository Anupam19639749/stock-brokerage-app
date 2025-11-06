namespace StockAlertTracker.API.DTOs.Stock
{
    public class MarketIndexDto
    {
        public string Ticker { get; set; }
        public string Name { get; set; }
        public decimal CurrentPrice { get; set; }
        public decimal Change { get; set; }
        public decimal PercentChange { get; set; }
    }
}