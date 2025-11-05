using System.Text.Json.Serialization;

namespace StockAlertTracker.API.DTOs.Stock
{
    public class FinnhubSearchDto
    {
        [JsonPropertyName("result")]
        public List<FinnhubSearchResult> Result { get; set; }
    }

    public class FinnhubSearchResult
    {
        [JsonPropertyName("symbol")]
        public string Symbol { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; }
    }
}