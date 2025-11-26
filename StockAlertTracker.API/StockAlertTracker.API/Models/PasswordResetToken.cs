using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StockAlertTracker.API.Models
{
    [Table("PasswordResetTokens")]
    public class PasswordResetToken
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [StringLength(128)]
        public string TokenHash { get; set; }

        [Required]
        public DateTime ExpiryDate { get; set; }

        public bool IsUsed { get; set; } = false;

        // --- Navigation Properties ---
        [Required]
        public int UserId { get; set; } // Foreign Key
        [ForeignKey("UserId")]
        public virtual User User { get; set; }
    }
}