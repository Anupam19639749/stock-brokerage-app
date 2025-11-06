using StockAlertTracker.API.DTOs.Stock;
using StockAlertTracker.API.Helpers;
using StockAlertTracker.API.Interfaces.Services;
using System.Net.Http.Json; 

namespace StockAlertTracker.API.Services
{
    public class StockDataService : IStockDataService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;

        // --- Hard-coded list for Market Overview ---
        private static readonly List<string> _marketOverviewTickers = new List<string> // <-- Renamed for clarity
        {
            "AAPL", "MSFT", "GOOGL", "AMZN", "TSLA", "NVDA",
            "META", "JPM", "JNJ", "V", "PG", "NFLX",
            "DIS", "PYPL", "INTC", "CSCO", "PEP", "ADBE"
        };

        // --- Hard-coded list for Indices ---
        private static readonly Dictionary<string, string> _marketIndices = new Dictionary<string, string>
        {
            { "^NSEI", "Nifty 50" },
            { "^BSESN", "S&P BSE Sensex" }
        };

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
                    response.Message = "Stock not found or data unavailable";
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

        public async Task<ServiceResponse<IEnumerable<MarketIndexDto>>> GetMarketIndicesAsync()
        {
            var response = new ServiceResponse<IEnumerable<MarketIndexDto>>();
            var indexList = new List<MarketIndexDto>();

            foreach (var index in _marketIndices)
            {
                var quoteResponse = await GetLiveQuoteAsync(index.Key); // Reuse our existing method
                if (quoteResponse.Success && quoteResponse.Data != null)
                {
                    var quote = quoteResponse.Data;
                    decimal change = quote.CurrentPrice - quote.PreviousClosePrice;
                    decimal percentChange = 0;
                    if (quote.PreviousClosePrice != 0)
                    {
                        percentChange = (change / quote.PreviousClosePrice) * 100;
                    }

                    indexList.Add(new MarketIndexDto
                    {
                        Ticker = index.Key,
                        Name = index.Value,
                        CurrentPrice = quote.CurrentPrice,
                        Change = change,
                        PercentChange = percentChange
                    });
                }
            }

            response.Data = indexList;
            return response;
        }

        public async Task<ServiceResponse<IEnumerable<StockQuoteCardDto>>> GetMarketOverviewAsync(int page, int limit)
        {
            var response = new ServiceResponse<IEnumerable<StockQuoteCardDto>>();
            var quoteList = new List<StockQuoteCardDto>();

            // Calculate which tickers to fetch based on page and limit
            var tickersToFetch = _marketOverviewTickers
                .Skip((page - 1) * limit)
                .Take(limit)
                .ToList();

            if (!tickersToFetch.Any())
            {
                response.Data = new List<StockQuoteCardDto>(); // Return empty list if page is too high
                return response;
            }

            // Fetch all 6 quotes in parallel
            var tasks = tickersToFetch.Select(async ticker =>
            {
                var quoteResponse = await GetLiveQuoteAsync(ticker);
                if (quoteResponse.Success && quoteResponse.Data != null)
                {
                    var quote = quoteResponse.Data;
                    decimal change = quote.CurrentPrice - quote.PreviousClosePrice;
                    decimal percentChange = 0;
                    if (quote.PreviousClosePrice != 0)
                    {
                        percentChange = (change / quote.PreviousClosePrice) * 100;
                    }

                    return new StockQuoteCardDto
                    {
                        Ticker = ticker,
                        CurrentPrice = quote.CurrentPrice,
                        Change = change,
                        PercentChange = percentChange
                    };
                }
                return null;
            });

            var results = await Task.WhenAll(tasks);
            response.Data = results.Where(q => q != null); // Filter out any that failed
            return response;
        }
        public async Task<ServiceResponse<CompanyProfileDto>> GetCompanyProfileAsync(string ticker)
        {
            var response = new ServiceResponse<CompanyProfileDto>();
            try
            {
                // Finnhub's /stock/profile2 endpoint
                var finnhubResponse = await _httpClient.GetFromJsonAsync<FinnhubProfileDto>($"stock/profile2?symbol={ticker}&token={_apiKey}");

                if (finnhubResponse == null || string.IsNullOrEmpty(finnhubResponse.Name))
                {
                    response.Success = false;
                    response.Message = "Company profile not found.";
                    return response;
                }

                // We're just mapping this 1-to-1, but we can use AutoMapper later if we want
                response.Data = new CompanyProfileDto
                {
                    Name = finnhubResponse.Name,
                    Ticker = finnhubResponse.Ticker,
                    LogoUrl = finnhubResponse.LogoUrl,
                    Industry = finnhubResponse.Industry,
                    WebsiteUrl = finnhubResponse.WebsiteUrl
                };
                return response;
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = $"Error fetching company profile: {ex.Message}";
                return response;
            }
        }
    }
}
