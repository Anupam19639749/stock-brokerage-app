using System.ComponentModel.DataAnnotations;

namespace StockAlertTracker.API.DTOs.Auth
{
    public class ForgotPasswordDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}