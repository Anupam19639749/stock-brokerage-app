using System.ComponentModel.DataAnnotations;
using StockAlertTracker.API.Validation;

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

        [DateNotInFuture] // Our first check
        [MinimumAge(12, ErrorMessage = "You must be at least 12 years old to use this service.")] 
        public DateTime? DateOfBirth { get; set; }
    }
}