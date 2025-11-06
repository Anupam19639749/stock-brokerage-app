using Microsoft.AspNetCore.SignalR;
using StockAlertTracker.API.Hubs;
using StockAlertTracker.API.Interfaces.Repositories;
using StockAlertTracker.API.Interfaces.Services;
using StockAlertTracker.API.Models;
using StockAlertTracker.API.Models.Enums;

namespace StockAlertTracker.API.Services
{
    public class RealTimePriceWorker : BackgroundService
    {
        private readonly ILogger<RealTimePriceWorker> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly IHubContext<PriceHub> _hubContext;

        public RealTimePriceWorker(ILogger<RealTimePriceWorker> logger,
                                 IServiceProvider serviceProvider,
                                 IHubContext<PriceHub> hubContext)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
            _hubContext = hubContext;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Real-Time Price Worker starting.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    // Create a new "scope" to use our scoped services
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                        var stockDataService = scope.ServiceProvider.GetRequiredService<IStockDataService>();
                        var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();

                        await ProcessPriceUpdates(unitOfWork, stockDataService, emailService);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occurred in the Real-Time Price Worker.");
                }

                // Wait for 30 seconds before running again
                await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
            }

            _logger.LogInformation("Real-Time Price Worker stopping.");
        }

        private async Task ProcessPriceUpdates(IUnitOfWork unitOfWork,
                                       IStockDataService stockDataService,
                                       IEmailService emailService)
        {
            _logger.LogInformation("Processing price updates..."); // <-- RENAMED for clarity

            // 1. Get all unique tickers we need to track
            var holdingTickers = (await unitOfWork.PortfolioHoldings.GetAllAsync()).Select(h => h.Ticker).Distinct();
            var alertTickers = (await unitOfWork.PriceAlerts.GetActiveAlertsAsync()).Select(a => a.Ticker).Distinct();
            var allTickers = holdingTickers.Union(alertTickers).ToList();

            if (!allTickers.Any())
            {
                _logger.LogInformation("No tickers to track. Skipping.");
                return;
            }

            _logger.LogInformation($"Found {allTickers.Count} unique tickers to track."); 

            // 2. Get live prices from Finnhub for all of them
            var priceTasks = allTickers.Select(async ticker => new
            {
                Ticker = ticker,
                Quote = (await stockDataService.GetLiveQuoteAsync(ticker)).Data
            });
            var priceResults = await Task.WhenAll(priceTasks);

            // 3. BROADCAST prices via SignalR (for Live P&L)
            foreach (var result in priceResults.Where(p => p.Quote != null))
            {
                // --- THIS IS THE LOG YOU WERE LOOKING FOR ---
                _logger.LogInformation($"Broadcasting price for {result.Ticker}: ${result.Quote.CurrentPrice}");  

                // Send to ALL connected clients
                await _hubContext.Clients.All.SendAsync(
                    "ReceivePriceUpdate",  // This is the "channel" React will listen to
                    result.Ticker,
                    result.Quote.CurrentPrice
                );
            }

            // 4. CHECK ALERTS
            var activeAlerts = await unitOfWork.PriceAlerts.GetActiveAlertsAsync();
            var alertsToTrigger = new List<PriceAlert>();
            var usersToEmail = new Dictionary<int, User>(); // To avoid fetching user multiple times

            foreach (var alert in activeAlerts)
            {
                var livePrice = priceResults.FirstOrDefault(p => p.Ticker == alert.Ticker)?.Quote?.CurrentPrice;
                if (livePrice == null) continue;

                bool shouldTrigger = false;
                if (alert.Condition == AlertCondition.ABOVE && livePrice >= alert.TargetPrice)
                {
                    shouldTrigger = true;
                }
                else if (alert.Condition == AlertCondition.BELOW && livePrice <= alert.TargetPrice)
                {
                    shouldTrigger = true;
                }

                if (shouldTrigger)
                {
                    // Mark for triggering
                    alert.Status = AlertStatus.Triggered;
                    alertsToTrigger.Add(alert);

                    // Get user for email (if we haven't already)
                    if (!usersToEmail.ContainsKey(alert.UserId))
                    {
                        usersToEmail[alert.UserId] = await unitOfWork.Users.GetByIdAsync(alert.UserId);
                    }
                    var user = usersToEmail[alert.UserId];

                    // Send the email
                    await SendAlertEmail(emailService, user, alert, livePrice.Value);
                }
            }

            // 5. Save changes to all triggered alerts
            if (alertsToTrigger.Any())
            {
                foreach (var alert in alertsToTrigger)
                {
                    unitOfWork.PriceAlerts.Update(alert);
                }
                await unitOfWork.CompleteAsync();
                _logger.LogInformation($"Triggered {alertsToTrigger.Count} alerts.");
            }
            else
            {
                _logger.LogInformation("No new alerts triggered."); 
            }
        }

        private async Task SendAlertEmail(IEmailService emailService, User user, PriceAlert alert, decimal currentPrice)
        {
            try
            {
                string htmlBody = await ((EmailService)emailService).GetTemplateHtmlAsync("PriceAlert.html");

                htmlBody = htmlBody.Replace("{{UserName}}", user.FirstName);
                htmlBody = htmlBody.Replace("{{Ticker}}", alert.Ticker);
                htmlBody = htmlBody.Replace("{{Condition}}", alert.Condition.ToString().ToUpper());
                htmlBody = htmlBody.Replace("{{TargetPrice}}", alert.TargetPrice.ToString("F2"));
                htmlBody = htmlBody.Replace("{{CurrentPrice}}", currentPrice.ToString("F2"));

                await emailService.SendEmailAsync(user.Email, $"Stock Alert: {alert.Ticker} has reached your target!", htmlBody);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to send alert email for user {user.Id} and ticker {alert.Ticker}");
            }
        }
    }
}