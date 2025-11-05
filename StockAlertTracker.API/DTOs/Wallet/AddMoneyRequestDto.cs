using System.ComponentModel.DataAnnotations;

namespace StockAlertTracker.API.DTOs.Wallet
{
    public class AddMoneyRequestDto
    {
        [Required]
        [Range(100, 1000000, ErrorMessage = "Amount must be between 100 and 1,000,000")]
        public decimal Amount { get; set; }

        [Required(ErrorMessage = "Password is required to confirm the transaction")]
        public string Password { get; set; }
    }
}