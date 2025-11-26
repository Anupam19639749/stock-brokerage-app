using StockAlertTracker.API.Models;

namespace StockAlertTracker.API.Interfaces.Repositories
{
    public interface IOrderRepository : IGenericRepository<Order>
    {
        Task<IEnumerable<Order>> GetPendingOrdersAsync();
        Task<IEnumerable<Order>> GetOrdersByUserIdAsync(int userId);
    }
}