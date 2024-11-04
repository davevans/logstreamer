namespace LogProducer;

public class LogWorker(ILogger<LogWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var counter = 0;
        var ts = TimeSpan.FromSeconds(2);
        while (!stoppingToken.IsCancellationRequested)
        {
            logger.LogInformation("{0} logging here {1}", DateTime.Now, counter++);
            await Task.Delay(ts, stoppingToken);
        }
        
        logger.LogInformation("Finished...");
    }
}