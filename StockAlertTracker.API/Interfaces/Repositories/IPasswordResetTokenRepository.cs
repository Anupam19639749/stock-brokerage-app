using StockAlertTracker.API.Models;

namespace StockAlertTracker.API.Interfaces.Repositories
{
    public interface IPasswordResetTokenRepository : IGenericRepository<PasswordResetToken>
    {
        Task<PasswordResetToken> GetByTokenHashAsync(string tokenHash);
    }
}