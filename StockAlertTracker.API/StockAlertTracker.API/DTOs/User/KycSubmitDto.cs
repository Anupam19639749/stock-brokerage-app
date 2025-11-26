using System.ComponentModel.DataAnnotations;

namespace StockAlertTracker.API.DTOs.User
{
    public class KycSubmitDto
    {
        [Required]
        [StringLength(10, MinimumLength = 10, ErrorMessage = "PAN must be 10 characters")]
        [RegularExpression("^[A-Z]{5}[0-9]{4}[A-Z]{1}$", ErrorMessage = "Invalid PAN format")]
        public string PanNumber { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Bank name must be between 3 and 100 characters")]
        public string BankName { get; set; }

        [Required]
        [StringLength(20, MinimumLength = 8, ErrorMessage = "Bank account number must be between 8 and 20 digits")]
        [RegularExpression("^[0-9]*$", ErrorMessage = "Bank account number must contain only digits")]
        public string BankAccountNumber { get; set; }

        [Required]
        [StringLength(11, MinimumLength = 11, ErrorMessage = "IFSC code must be 11 characters")]
        public string BankIfscCode { get; set; }
    }
}