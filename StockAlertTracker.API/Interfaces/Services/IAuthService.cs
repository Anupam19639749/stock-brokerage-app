using StockAlertTracker.API.DTOs.Auth;
using StockAlertTracker.API.Helpers; // We will create this folder

namespace StockAlertTracker.API.Interfaces.Services
{
    public interface IAuthService
    {
        Task<ServiceResponse<UserTokenDto>> RegisterAsync(RegisterUserDto registerDto);
        Task<ServiceResponse<UserTokenDto>> LoginAsync(LoginDto loginDto);
        Task<ServiceResponse<string>> ForgotPasswordAsync(string email);
        Task<ServiceResponse<string>> ResetPasswordAsync(ResetPasswordDto resetDto);
    }
}