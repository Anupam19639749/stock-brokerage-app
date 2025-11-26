using StockAlertTracker.API.DTOs.Wallet;
using StockAlertTracker.API.Helpers;

namespace StockAlertTracker.API.Interfaces.Services
{
    public interface IWalletService
    {
        Task<ServiceResponse<WalletBalanceDto>> GetWalletBalanceAsync(int userId);
        Task<ServiceResponse<WalletBalanceDto>> AddMoneyAsync(int userId, AddMoneyRequestDto addMoneyDto);
        Task<IEnumerable<WalletTransactionDto>> GetWalletHistoryAsync(int userId);
    }
}