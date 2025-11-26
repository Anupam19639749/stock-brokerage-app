using Microsoft.EntityFrameworkCore;
using StockAlertTracker.API.Data;
using StockAlertTracker.API.Interfaces.Repositories;
using StockAlertTracker.API.Models;

namespace StockAlertTracker.API.Repositories
{
    public class PortfolioHoldingRepository : GenericRepository<PortfolioHolding>, IPortfolioHoldingRepository
    {
        public PortfolioHoldingRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<PortfolioHolding> GetHoldingAsync(int userId, string ticker)
        {
            return await _dbSet.FirstOrDefaultAsync(h => h.UserId == userId && h.Ticker == ticker);
        }

        public async Task<IEnumerable<PortfolioHolding>> GetHoldingsByUserIdAsync(int userId)
        {
            return await _dbSet.Where(h => h.UserId == userId).ToListAsync();
        }
    }
}