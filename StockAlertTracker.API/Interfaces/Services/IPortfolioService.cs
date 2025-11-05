using StockAlertTracker.API.DTOs.Trade;
using StockAlertTracker.API.Helpers;

namespace StockAlertTracker.API.Interfaces.Services
{
    public interface IPortfolioService
    {
        Task<ServiceResponse<IEnumerable<PortfolioHoldingDto>>> GetPortfolioAsync(int userId);
    }
}