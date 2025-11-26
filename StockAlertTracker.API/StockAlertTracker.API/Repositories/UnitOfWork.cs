using StockAlertTracker.API.Data;
using StockAlertTracker.API.Interfaces.Repositories;

namespace StockAlertTracker.API.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;

        // Private instances of repositories
        private IUserRepository _users;
        private IWalletRepository _wallets;
        private IWalletTransactionRepository _walletTransactions;
        private IPortfolioHoldingRepository _portfolioHoldings;
        private IOrderRepository _orders;
        private IPriceAlertRepository _priceAlerts;
        private IPlatformStatsRepository _platformStats;
        private IPasswordResetTokenRepository _passwordResetTokens;

        public UnitOfWork(ApplicationDbContext context)
        {
            _context = context;
        }

        // Public properties that lazy-load the repositories
        // This ensures we only create a repository instance if we need it
        public IUserRepository Users => _users ??= new UserRepository(_context);
        public IWalletRepository Wallets => _wallets ??= new WalletRepository(_context);
        public IWalletTransactionRepository WalletTransactions => _walletTransactions ??= new WalletTransactionRepository(_context);
        public IPortfolioHoldingRepository PortfolioHoldings => _portfolioHoldings ??= new PortfolioHoldingRepository(_context);
        public IOrderRepository Orders => _orders ??= new OrderRepository(_context);
        public IPriceAlertRepository PriceAlerts => _priceAlerts ??= new PriceAlertRepository(_context);
        public IPlatformStatsRepository PlatformStats => _platformStats ??= new PlatformStatsRepository(_context);
        public IPasswordResetTokenRepository PasswordResetTokens => _passwordResetTokens ??= new PasswordResetTokenRepository(_context);


        public async Task<int> CompleteAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public void Dispose()
        {
            _context.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}