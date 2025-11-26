using StockAlertTracker.API.Models.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StockAlertTracker.API.Models
{
    [Table("Orders")]
    public class Order
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required(ErrorMessage = "Ticker symbol is required")]
        [StringLength(10)]
        public string Ticker { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1")]
        public int Quantity { get; set; }

        [Required]
        [Column(TypeName = "decimal(18, 2)")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be positive")]
        public decimal PricePerShare { get; set; } // The price at the time of the order

        [Required]
        public OrderType Type { get; set; }

        [Required]
        public OrderStatus Status { get; set; } = OrderStatus.Pending;

        [Required]
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        // --- Navigation Properties ---
        [Required]
        public int UserId { get; set; } // Foreign Key
        [ForeignKey("UserId")]
        public virtual User User { get; set; }
    }
}