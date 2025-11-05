using StockAlertTracker.API.Data;
using StockAlertTracker.API.Interfaces.Repositories;
using StockAlertTracker.API.Models;

namespace StockAlertTracker.API.Repositories
{
    public class WalletTransactionRepository : GenericRepository<WalletTransaction>, IWalletTransactionRepository
    {
        public WalletTransactionRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}