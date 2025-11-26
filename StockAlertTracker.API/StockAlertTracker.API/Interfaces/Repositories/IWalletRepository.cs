using StockAlertTracker.API.Models;

namespace StockAlertTracker.API.Interfaces.Repositories
{
    public interface IWalletRepository : IGenericRepository<Wallet>
    {
        Task<Wallet> GetByUserIdAsync(int userId);
    }
}