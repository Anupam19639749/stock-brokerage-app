using StockAlertTracker.API.DTOs.User;

namespace StockAlertTracker.API.DTOs.Auth
{
    public class UserTokenDto
    {
        public string Token { get; set; }
        public UserDetailsDto User { get; set; }
    }
}