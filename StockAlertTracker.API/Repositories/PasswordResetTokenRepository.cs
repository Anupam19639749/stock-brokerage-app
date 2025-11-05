using Microsoft.EntityFrameworkCore;
using StockAlertTracker.API.Data;
using StockAlertTracker.API.Interfaces.Repositories;
using StockAlertTracker.API.Models;

namespace StockAlertTracker.API.Repositories
{
    public class PasswordResetTokenRepository : GenericRepository<PasswordResetToken>, IPasswordResetTokenRepository
    {
        public PasswordResetTokenRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<PasswordResetToken> GetByTokenHashAsync(string tokenHash)
        {
            return await _dbSet.FirstOrDefaultAsync(t => t.TokenHash == tokenHash && !t.IsUsed && t.ExpiryDate > DateTime.UtcNow);
        }
    }
}