using StockAlertTracker.API.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace StockAlertTracker.API.DTOs.Alert
{
    public class AlertCreateDto
    {
        [Required]
        public string Ticker { get; set; }

        [Required]
        public AlertCondition Condition { get; set; }

        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal TargetPrice { get; set; }
    }
}