namespace StockAlertTracker.API.DTOs.Wallet
{
    public class WalletTransactionDto
    {
        public int Id { get; set; }
        public decimal Amount { get; set; }
        public string Type { get; set; }
        public DateTime Timestamp { get; set; }
    }
}