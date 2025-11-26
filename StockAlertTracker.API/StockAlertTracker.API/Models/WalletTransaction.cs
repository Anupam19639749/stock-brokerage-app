using StockAlertTracker.API.Models.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StockAlertTracker.API.Models
{
    [Table("WalletTransactions")]
    public class WalletTransaction
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal Amount { get; set; } // Positive for deposit/sell, negative for buy

        [Required]
        public WalletTransactionType Type { get; set; }

        [Required]
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        // --- Navigation Properties ---
        [Required]
        public int WalletId { get; set; } // Foreign Key
        [ForeignKey("WalletId")]
        public virtual Wallet Wallet { get; set; }
    }
}