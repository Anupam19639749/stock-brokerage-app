using StockAlertTracker.API.DTOs.Alert;
using StockAlertTracker.API.Helpers;

namespace StockAlertTracker.API.Interfaces.Services
{
    public interface IAlertService
    {
        Task<ServiceResponse<AlertDetailsDto>> CreateAlertAsync(int userId, AlertCreateDto alertDto);
        Task<ServiceResponse<IEnumerable<AlertDetailsDto>>> GetMyAlertsAsync(int userId);
        Task<ServiceResponse<string>> DeleteAlertAsync(int userId, int alertId);
    }
}