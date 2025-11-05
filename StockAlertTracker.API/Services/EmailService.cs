using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using StockAlertTracker.API.Interfaces.Services;

namespace StockAlertTracker.API.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config;
        private readonly IWebHostEnvironment _env;

        public EmailService(IConfiguration config, IWebHostEnvironment env)
        {
            _config = config;
            _env = env;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            var emailSettings = _config.GetSection("EmailSettings");

            var email = new MimeMessage();
            email.From.Add(new MailboxAddress(emailSettings["FromName"], emailSettings["FromEmail"]));
            email.To.Add(MailboxAddress.Parse(toEmail));
            email.Subject = subject;

            var builder = new BodyBuilder
            {
                HtmlBody = body
            };
            email.Body = builder.ToMessageBody();

            using var smtp = new SmtpClient();

            // Connect to Gmail SMTP
            await smtp.ConnectAsync(emailSettings["SmtpHost"], int.Parse(emailSettings["SmtpPort"]!), SecureSocketOptions.StartTls);

            // Authenticate using the password from user-secrets
            await smtp.AuthenticateAsync(emailSettings["FromEmail"], emailSettings["Password"]);

            // Send the email
            await smtp.SendAsync(email);
            await smtp.DisconnectAsync(true);
        }

        // We'll add helper methods here to read our HTML templates
        public async Task<string> GetTemplateHtmlAsync(string templateName)
        {
            // Gets the path to wwwroot/EmailTemplates/templateName.html
            string filePath = Path.Combine(_env.ContentRootPath, "EmailTemplates", templateName);

            if (!File.Exists(filePath))
            {
                // Fallback or error
                return $"<p>Email template '{templateName}' not found.</p>";
            }

            string htmlBody = await File.ReadAllTextAsync(filePath);
            return htmlBody;
        }
    }
}