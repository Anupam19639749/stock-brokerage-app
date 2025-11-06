namespace StockAlertTracker.API.DTOs.Stock
{
    public class CompanyProfileDto
    {
        public string Name { get; set; }
        public string Ticker { get; set; }
        public string LogoUrl { get; set; }
        public string Industry { get; set; }
        public string WebsiteUrl { get; set; }
    }
}