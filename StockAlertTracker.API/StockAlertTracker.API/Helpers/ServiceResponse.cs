namespace StockAlertTracker.API.Helpers
{
    // A standard wrapper for all our service responses
    public class ServiceResponse<T>
    {
        public T? Data { get; set; }
        public bool Success { get; set; } = true;
        public string Message { get; set; } = string.Empty;
    }
}