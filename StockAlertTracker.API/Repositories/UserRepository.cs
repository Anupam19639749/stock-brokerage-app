using Microsoft.EntityFrameworkCore;
using StockAlertTracker.API.Data;
using StockAlertTracker.API.Interfaces.Repositories;
using StockAlertTracker.API.Models;

namespace StockAlertTracker.API.Repositories
{
    public class UserRepository : GenericRepository<User>, IUserRepository
    {
        public UserRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<User> GetByEmailAsync(string email)
        {
            return await _dbSet.FirstOrDefaultAsync(u => u.Email == email);
        }

        public override async Task<User> GetByIdAsync(int id)
        {
            return await _dbSet.Include(u => u.Wallet)
                               .FirstOrDefaultAsync(u => u.Id == id);
        }
    }
}