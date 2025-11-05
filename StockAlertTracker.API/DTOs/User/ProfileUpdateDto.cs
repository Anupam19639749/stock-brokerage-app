using System.ComponentModel.DataAnnotations;

namespace StockAlertTracker.API.DTOs.User
{
    public class ProfileUpdateDto
    {
        [Required]
        [StringLength(50, MinimumLength = 2)]
        public string FirstName { get; set; }

        [Required]
        [StringLength(50, MinimumLength = 2)]
        public string LastName { get; set; }

        [Phone]
        public string? PhoneNumber { get; set; }

        [StringLength(20)]
        public string? Gender { get; set; }

        public DateTime? DateOfBirth { get; set; }
    }
}