using StockAlertTracker.API.Models.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StockAlertTracker.API.Models
{
    [Table("PriceAlerts")]
    public class PriceAlert
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required(ErrorMessage = "Ticker symbol is required")]
        [StringLength(10)]
        public string Ticker { get; set; }

        [Required]
        public AlertCondition Condition { get; set; }

        [Required]
        [Column(TypeName = "decimal(18, 2)")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Target price must be positive")]
        public decimal TargetPrice { get; set; }

        [Required]
        public AlertStatus Status { get; set; } = AlertStatus.Active;

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // --- Navigation Properties ---
        [Required]
        public int UserId { get; set; } // Foreign Key
        [ForeignKey("UserId")]
        public virtual User User { get; set; }
    }
}