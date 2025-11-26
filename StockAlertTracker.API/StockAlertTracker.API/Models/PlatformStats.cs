using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StockAlertTracker.API.Models
{
    [Table("PlatformStats")]
    public class PlatformStats
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public DateTime DateCalculated { get; set; }

        [Required]
        public int TotalUsers { get; set; }

        [Required]
        public int ActiveUsers { get; set; }

        [Required]
        public string TopWishlistedStocks { get; set; } // Stored as JSON string

        [Required]
        public string TopAlertedStocks { get; set; } // Stored as JSON string
    }
}