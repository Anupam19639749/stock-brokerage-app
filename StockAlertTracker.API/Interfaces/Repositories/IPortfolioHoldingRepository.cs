using StockAlertTracker.API.Models;

namespace StockAlertTracker.API.Interfaces.Repositories
{
    public interface IPortfolioHoldingRepository : IGenericRepository<PortfolioHolding>
    {
        Task<PortfolioHolding> GetHoldingAsync(int userId, string ticker);
        Task<IEnumerable<PortfolioHolding>> GetHoldingsByUserIdAsync(int userId);
    }
}