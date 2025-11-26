namespace StockAlertTracker.API.Interfaces.Repositories
{
    public interface IUnitOfWork : IDisposable
    {
        IUserRepository Users { get; }
        IWalletRepository Wallets { get; }
        IWalletTransactionRepository WalletTransactions { get; }
        IPortfolioHoldingRepository PortfolioHoldings { get; }
        IOrderRepository Orders { get; }
        IPriceAlertRepository PriceAlerts { get; }
        IPlatformStatsRepository PlatformStats { get; }
        IPasswordResetTokenRepository PasswordResetTokens { get; }

        Task<int> CompleteAsync();
    }
}