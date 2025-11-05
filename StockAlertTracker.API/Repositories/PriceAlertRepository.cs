using Microsoft.EntityFrameworkCore;
using StockAlertTracker.API.Data;
using StockAlertTracker.API.Interfaces.Repositories;
using StockAlertTracker.API.Models;
using StockAlertTracker.API.Models.Enums;

namespace StockAlertTracker.API.Repositories
{
    public class PriceAlertRepository : GenericRepository<PriceAlert>, IPriceAlertRepository
    {
        public PriceAlertRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<PriceAlert>> GetActiveAlertsAsync()
        {
            return await _dbSet.Where(a => a.Status == AlertStatus.Active).ToListAsync();
        }
    }
}