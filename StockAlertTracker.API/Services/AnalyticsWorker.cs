using StockAlertTracker.API.Interfaces.Repositories;
using StockAlertTracker.API.Models;
using System.Text.Json;

namespace StockAlertTracker.API.Services
{
    public class AnalyticsWorker : IHostedService, IDisposable
    {
        private readonly ILogger<AnalyticsWorker> _logger;
        private readonly IServiceProvider _serviceProvider;
        private Timer _timer;

        public AnalyticsWorker(ILogger<AnalyticsWorker> logger, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Analytics Worker starting.");

            // Calculate the time until the next midnight
            var now = DateTime.UtcNow;
            var nextRunTime = now.Date.AddDays(1); // Midnight tonight (UTC)
            var timeToFirstRun = TimeSpan.FromSeconds(10);

            _logger.LogInformation($"First analytics run scheduled for: {nextRunTime} UTC");

            // Set the timer to run once, then it will reschedule itself for every 24 hours
            _timer = new Timer(DoWork, null, timeToFirstRun, Timeout.InfiniteTimeSpan);

            return Task.CompletedTask;
        }

        private void DoWork(object state)
        {
            try
            {
                _logger.LogInformation("Analytics Worker is running.");

                // Create a new scope to use scoped services like IUnitOfWork
                using (var scope = _serviceProvider.CreateScope())
                {
                    var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                    RunAnalytics(unitOfWork).Wait(); // Run the async task and wait for it
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred in the Analytics Worker.");
            }
            finally
            {
                // Reschedule for 24 hours from now
                _timer?.Change(TimeSpan.FromHours(24), Timeout.InfiniteTimeSpan);
                _logger.LogInformation("Analytics run complete. Next run in 24 hours.");
            }
        }

        private async Task RunAnalytics(IUnitOfWork unitOfWork)
        {
            _logger.LogInformation("Calculating platform stats...");

            // 1. Get Total Users
            var allUsers = await unitOfWork.Users.FindAsync(u => u.Role == Models.Enums.RoleType.User);
            int totalUsers = allUsers.Count();

            // 2. Get Active Users (logged in within last 30 days)
            var thirtyDaysAgo = DateTime.UtcNow.AddDays(-30);
            int activeUsers = allUsers.Count(u => u.LastLogin.HasValue && u.LastLogin > thirtyDaysAgo);

            // 3. Get Top Held Stocks (from PortfolioHoldings)
            var topHeldStocks = (await unitOfWork.PortfolioHoldings.GetAllAsync())
                .GroupBy(h => h.Ticker)
                .Select(g => new { Ticker = g.Key, Count = g.Count() }) // Count = # of users who hold it
                .OrderByDescending(x => x.Count)
                .Take(10)
                .ToList();

            // 4. Get Top Alerted Stocks
            var topAlertedStocks = (await unitOfWork.PriceAlerts.GetAllAsync())
                .GroupBy(a => a.Ticker)
                .Select(g => new { Ticker = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .Take(10)
                .ToList();

            // 5. Create new stats record
            var newStats = new PlatformStats
            {
                DateCalculated = DateTime.UtcNow,
                TotalUsers = totalUsers,
                ActiveUsers = activeUsers,
                // Serialize the lists to JSON to store in the DB
                TopWishlistedStocks = JsonSerializer.Serialize(topHeldStocks), // Mapped from TopHeldStocks
                TopAlertedStocks = JsonSerializer.Serialize(topAlertedStocks)
            };

            // 6. Save to database
            await unitOfWork.PlatformStats.AddAsync(newStats);
            await unitOfWork.CompleteAsync();

            _logger.LogInformation($"Successfully saved new platform stats. Total Users: {totalUsers}");
        }


        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Analytics Worker stopping.");
            _timer?.Change(Timeout.Infinite, 0);
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}