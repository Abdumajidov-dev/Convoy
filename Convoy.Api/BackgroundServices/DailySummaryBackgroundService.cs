using Convoy.Service.Interfaces;

namespace Convoy.Api.BackgroundServices;

/// <summary>
/// Har kecha soat 12:00 da kechagi kunning ma'lumotlarini aggregate qiladi
/// </summary>
public class DailySummaryBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<DailySummaryBackgroundService> _logger;

    public DailySummaryBackgroundService(
        IServiceProvider serviceProvider,
        ILogger<DailySummaryBackgroundService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Daily Summary Background Service started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var now = DateTime.UtcNow;
                var nextMidnight = now.Date.AddDays(1);
                var delay = nextMidnight - now;

                _logger.LogInformation($"Next daily summary processing scheduled at {nextMidnight:yyyy-MM-dd HH:mm:ss} UTC (in {delay.TotalHours:F2} hours)");

                await Task.Delay(delay, stoppingToken);

                _logger.LogInformation("Starting daily summary processing...");
                await ProcessYesterdayDataAsync();
                _logger.LogInformation("Daily summary processing completed");

                _logger.LogInformation("Starting old locations cleanup...");
                await CleanupOldLocationsAsync();
                _logger.LogInformation("Old locations cleanup completed");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Daily Summary Background Service");
                await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
            }
        }

        _logger.LogInformation("Daily Summary Background Service stopped");
    }

    private async Task ProcessYesterdayDataAsync()
    {
        using var scope = _serviceProvider.CreateScope();
        var dailySummaryService = scope.ServiceProvider.GetRequiredService<IDailySummaryService>();
        await dailySummaryService.ProcessYesterdayDataAsync();
    }

    private async Task CleanupOldLocationsAsync()
    {
        using var scope = _serviceProvider.CreateScope();
        var dailySummaryService = scope.ServiceProvider.GetRequiredService<IDailySummaryService>();
        await dailySummaryService.CleanupOldLocationsAsync(daysToKeep: 7);
    }
}
