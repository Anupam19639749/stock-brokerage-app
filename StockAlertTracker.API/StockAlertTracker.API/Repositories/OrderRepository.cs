using Microsoft.EntityFrameworkCore;
using StockAlertTracker.API.Data;
using StockAlertTracker.API.Interfaces.Repositories;
using StockAlertTracker.API.Models;
using StockAlertTracker.API.Models.Enums;

namespace StockAlertTracker.API.Repositories
{
    public class OrderRepository : GenericRepository<Order>, IOrderRepository
    {
        public OrderRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Order>> GetPendingOrdersAsync()
        {
            return await _dbSet.Where(o => o.Status == OrderStatus.Pending)
                               .Include(o => o.User) // Include user details for the admin
                               .OrderByDescending(o => o.Timestamp)
                               .ToListAsync();
        }

        public async Task<IEnumerable<Order>> GetOrdersByUserIdAsync(int userId)
        {
            return await _dbSet.Where(o => o.UserId == userId)
                               .OrderByDescending(o => o.Timestamp)
                               .ToListAsync();
        }
    }
}