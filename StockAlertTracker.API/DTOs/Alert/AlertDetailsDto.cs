namespace StockAlertTracker.API.DTOs.Alert
{
    public class AlertDetailsDto
    {
        public int Id { get; set; }
        public string Ticker { get; set; }
        public string Condition { get; set; }
        public decimal TargetPrice { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}