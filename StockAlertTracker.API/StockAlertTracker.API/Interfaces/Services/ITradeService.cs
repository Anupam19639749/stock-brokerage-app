using StockAlertTracker.API.DTOs.Trade;
using StockAlertTracker.API.Helpers;

namespace StockAlertTracker.API.Interfaces.Services
{
    public interface ITradeService
    {
        Task<ServiceResponse<OrderDetailsDto>> PlaceOrderAsync(int userId, OrderRequestDto orderRequest);
        Task<ServiceResponse<IEnumerable<OrderDetailsDto>>> GetMyOrdersAsync(int userId);
    }
}