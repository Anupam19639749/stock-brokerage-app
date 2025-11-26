using StockAlertTracker.API.DTOs.Stock;
using StockAlertTracker.API.Helpers;

namespace StockAlertTracker.API.Interfaces.Services
{
    public interface IStockDataService
    {
        Task<ServiceResponse<FinnhubQuoteDto>> GetLiveQuoteAsync(string ticker);
        Task<ServiceResponse<IEnumerable<StockSearchResultDto>>> SearchStockAsync(string query);
        Task<ServiceResponse<IEnumerable<MarketIndexDto>>> GetMarketIndicesAsync();
        Task<ServiceResponse<IEnumerable<StockQuoteCardDto>>> GetMarketOverviewAsync(int page, int limit);
        Task<ServiceResponse<CompanyProfileDto>> GetCompanyProfileAsync(string ticker);
    }
}