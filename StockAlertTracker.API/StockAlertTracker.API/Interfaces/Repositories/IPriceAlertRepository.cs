using StockAlertTracker.API.Models;

namespace StockAlertTracker.API.Interfaces.Repositories
{
    public interface IPriceAlertRepository : IGenericRepository<PriceAlert>
    {
        Task<IEnumerable<PriceAlert>> GetActiveAlertsAsync();
    }
}