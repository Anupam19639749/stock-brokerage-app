using StockAlertTracker.API.DTOs.User;
using StockAlertTracker.API.Helpers;

namespace StockAlertTracker.API.Interfaces.Services
{
    public interface IUserService
    {
        Task<ServiceResponse<UserDetailsDto>> GetUserDetailsAsync(int userId);
        Task<ServiceResponse<string>> SubmitKycAsync(int userId, KycSubmitDto kycDto);
        Task<ServiceResponse<UserDetailsDto>> UpdateProfileAsync(int userId, ProfileUpdateDto profileDto);

        Task<ServiceResponse<string>> UpdateProfileImageAsync(int userId, byte[] imageBytes);
    }
}