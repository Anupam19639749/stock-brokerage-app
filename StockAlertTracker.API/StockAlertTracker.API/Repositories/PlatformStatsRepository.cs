using Microsoft.EntityFrameworkCore;
using StockAlertTracker.API.Data;
using StockAlertTracker.API.Interfaces.Repositories;
using StockAlertTracker.API.Models;

namespace StockAlertTracker.API.Repositories
{
    public class PlatformStatsRepository : GenericRepository<PlatformStats>, IPlatformStatsRepository
    {
        public PlatformStatsRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<PlatformStats> GetLatestStatsAsync()
        {
            return await _dbSet.OrderByDescending(s => s.DateCalculated).FirstOrDefaultAsync();
        }
    }
}