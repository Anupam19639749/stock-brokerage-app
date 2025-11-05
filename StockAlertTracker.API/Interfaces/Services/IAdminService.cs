using StockAlertTracker.API.DTOs.Admin;
using StockAlertTracker.API.DTOs.Trade;
using StockAlertTracker.API.Helpers;

namespace StockAlertTracker.API.Interfaces.Services
{
    public interface IAdminService
    {
        Task<ServiceResponse<IEnumerable<KycRequestDetailsDto>>> GetKycRequestsAsync();
        Task<ServiceResponse<string>> ApproveKycAsync(int userId);
        Task<ServiceResponse<string>> RejectKycAsync(int userId);
        Task<ServiceResponse<IEnumerable<OrderDetailsDto>>> GetPendingOrdersAsync();
        Task<ServiceResponse<OrderDetailsDto>> ApproveOrderAsync(int orderId);
        Task<ServiceResponse<OrderDetailsDto>> RejectOrderAsync(int orderId);
        Task<ServiceResponse<AdminStatsDto>> GetPlatformStatsAsync();
    }
}