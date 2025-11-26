using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StockAlertTracker.API.Models
{
    [Table("Wallets")]
    public class Wallet
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [Column(TypeName = "decimal(18, 2)")]
        [Range(0, double.MaxValue, ErrorMessage = "Balance cannot be negative")]
        public decimal Balance { get; set; } = 0.0m;

        // --- Navigation Properties ---
        [Required]
        public int UserId { get; set; } // Foreign Key
        [ForeignKey("UserId")]
        public virtual User User { get; set; } // User has one Wallet

        public virtual ICollection<WalletTransaction> WalletTransactions { get; set; }

        public Wallet()
        {
            WalletTransactions = new HashSet<WalletTransaction>();
        }
    }
}