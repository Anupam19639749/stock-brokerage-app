namespace StockAlertTracker.API.Interfaces.Services
{
    public interface IStockDataService
    {
        Task<decimal?> GetLivePriceAsync(string ticker);
    }
}