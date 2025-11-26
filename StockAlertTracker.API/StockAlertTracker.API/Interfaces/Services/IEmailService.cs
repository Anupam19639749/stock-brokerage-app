namespace StockAlertTracker.API.Interfaces.Services
{
    public interface IEmailService
    {
        Task SendEmailAsync(string toEmail, string subject, string body);
        Task<string> GetTemplateHtmlAsync(string templateName);
    }
}