using StockAlertTracker.API.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace StockAlertTracker.API.DTOs.Trade
{
    public class OrderRequestDto
    {
        [Required]
        public string Ticker { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int Quantity { get; set; }

        [Required]
        public OrderType Type { get; set; } // BUY or SELL
    }
}