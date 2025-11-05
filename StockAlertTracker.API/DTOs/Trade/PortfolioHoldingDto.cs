namespace StockAlertTracker.API.DTOs.Trade
{
    public class PortfolioHoldingDto
    {
        public int Id { get; set; }
        public string Ticker { get; set; }
        public int Quantity { get; set; }
        public decimal AverageCostPrice { get; set; }
        public decimal TotalCost => Quantity * AverageCostPrice;
    }
}