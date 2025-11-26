using System.Text.Json.Serialization;

namespace StockAlertTracker.API.DTOs.Stock
{
    public class FinnhubProfileDto
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("ticker")]
        public string Ticker { get; set; }

        [JsonPropertyName("logo")]
        public string LogoUrl { get; set; }

        [JsonPropertyName("finnhubIndustry")]
        public string Industry { get; set; }

        [JsonPropertyName("weburl")]
        public string WebsiteUrl { get; set; }
    }
}