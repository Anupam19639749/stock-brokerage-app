using StockAlertTracker.API.Models;

namespace StockAlertTracker.API.Interfaces.Repositories
{
    public interface IPlatformStatsRepository : IGenericRepository<PlatformStats>
    {
        Task<PlatformStats> GetLatestStatsAsync();
    }
}