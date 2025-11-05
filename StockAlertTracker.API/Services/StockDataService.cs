using StockAlertTracker.API.DTOs.Stock;
using StockAlertTracker.API.Helpers;
using StockAlertTracker.API.Interfaces.Services;
using System.Net.Http.Json; // Required for .NET 5+ GetFromJsonAsync

namespace StockAlertTracker.API.Services
{
    public class StockDataService : IStockDataService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;

        public StockDataService(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClient = httpClientFactory.CreateClient("Finnhub");
            _apiKey = configuration["Finnhub:ApiKey"]!;
        }

        public async Task<ServiceResponse<FinnhubQuoteDto>> GetLiveQuoteAsync(string ticker)
        {
            var response = new ServiceResponse<FinnhubQuoteDto>();
            try
            {
                // Finnhub's /quote endpoint
                var finnhubResponse = await _httpClient.GetFromJsonAsync<FinnhubQuoteDto>($"quote?symbol={ticker}&token={_apiKey}");

                if (finnhubResponse == null)
                {
                    response.Success = false;
                    response.Message = "Stock not found.";
                    return response;
                }

                response.Data = finnhubResponse;
                return response;
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = $"Error fetching Finnhub data: {ex.Message}";
                return response;
            }
        }

        public async Task<ServiceResponse<IEnumerable<StockSearchResultDto>>> SearchStockAsync(string query)
        {
            var response = new ServiceResponse<IEnumerable<StockSearchResultDto>>();
            try
            {
                // Finnhub's /search endpoint
                var finnhubResponse = await _httpClient.GetFromJsonAsync<FinnhubSearchDto>($"search?q={query}&token={_apiKey}");

                if (finnhubResponse == null || finnhubResponse.Result == null)
                {
                    response.Data = new List<StockSearchResultDto>();
                    return response;
                }

                // Map from Finnhub's format to our simple DTO
                response.Data = finnhubResponse.Result.Select(r => new StockSearchResultDto
                {
                    Ticker = r.Symbol,
                    Description = r.Description
                }).ToList();

                return response;
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = $"Error fetching Finnhub data: {ex.Message}";
                return response;
            }
        }
    }
}