using StockAlertTracker.API.Models.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StockAlertTracker.API.Models
{
    [Table("Users")]
    public class User
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required(ErrorMessage = "First name is required")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "First name must be between 2 and 50 characters")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Last name is required")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "Last name must be between 2 and 50 characters")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [StringLength(256)]
        [EmailAddress(ErrorMessage = "Invalid email address format")]
        public string Email { get; set; }

        [Phone(ErrorMessage = "Invalid phone number format")]
        [StringLength(20)]
        public string? PhoneNumber { get; set; }

        [Required]
        public byte[] PasswordHash { get; set; }

        [Required]
        public byte[] PasswordSalt { get; set; }

        [StringLength(20)]
        public string? Gender { get; set; }

        [Column(TypeName = "date")]
        public DateTime? DateOfBirth { get; set; }

        public byte[]? ProfileImage { get; set; }

        [StringLength(50)] // e.g., "image/jpeg"
        public string? ProfileImageContentType { get; set; }

        // --- KYC Fields ---
        [StringLength(10, MinimumLength = 10, ErrorMessage = "PAN must be 10 characters")]
        [RegularExpression("^[A-Z]{5}[0-9]{4}[A-Z]{1}$", ErrorMessage = "Invalid PAN format")]
        public string? PanNumber { get; set; }

        [StringLength(100)]
        public string? BankName { get; set; }

        [StringLength(50)]
        public string? BankAccountNumber { get; set; }

        [StringLength(11, MinimumLength = 11, ErrorMessage = "IFSC code must be 11 characters")]
        public string? BankIfscCode { get; set; }

        [Required]
        public KycStatus KycStatus { get; set; } = KycStatus.NotSubmitted;

        // --- System Fields ---
        public DateTime? LastLogin { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Required]
        public RoleType Role { get; set; } = RoleType.User;

        [Required]
        public bool IsActive { get; set; } = true;

        // --- Navigation Properties ---
        public virtual Wallet? Wallet { get; set; } // User has one Wallet
        public virtual ICollection<PortfolioHolding> PortfolioHoldings { get; set; }
        public virtual ICollection<Order> Orders { get; set; }
        public virtual ICollection<PriceAlert> PriceAlerts { get; set; }
        public virtual ICollection<PasswordResetToken> PasswordResetTokens { get; set; }

        public User()
        {
            PortfolioHoldings = new HashSet<PortfolioHolding>();
            Orders = new HashSet<Order>();
            PriceAlerts = new HashSet<PriceAlert>();
            PasswordResetTokens = new HashSet<PasswordResetToken>();
        }
    }
}