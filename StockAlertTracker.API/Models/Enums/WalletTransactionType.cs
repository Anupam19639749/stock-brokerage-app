namespace StockAlertTracker.API.Models.Enums
{
    public enum WalletTransactionType
    {
        DEPOSIT,
        TRADE_DEBIT,     // Money locked for a BUY order
        TRADE_CREDIT,    // Money received from a SELL order
        ADJUSTMENT       // Admin manual adjustment
    }
}
