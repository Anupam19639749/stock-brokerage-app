namespace StockAlertTracker.API.DTOs.Trade
{
    public class OrderDetailsDto
    {
        public int Id { get; set; }
        public string Ticker { get; set; }
        public int Quantity { get; set; }
        public decimal PricePerShare { get; set; }
        public decimal TotalValue => Quantity * PricePerShare;
        public string Type { get; set; }
        public string Status { get; set; }
        public DateTime Timestamp { get; set; }
    }
}