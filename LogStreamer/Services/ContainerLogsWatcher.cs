using k8s;
using LogStreamer.Hubs;
using LogStreamer.Model;
using Microsoft.AspNetCore.SignalR;

namespace LogStreamer.Services;

/// <summary>
/// Watches a single container for logs.
/// </summary>
public class ContainerLogsWatcher : IContainerLogsWatcher
{
    private readonly ILogger<ContainerLogsWatcher> _logger;
    private readonly IKubernetes _kubernetes;
    private readonly IHubContext<LogsHub, ILogClient> _logsHubContext;

    /// <summary>
    /// Create a new ContainerLogsWatcher
    /// </summary>
    /// <param name="kubernetes">Kubernetes client</param>
    /// <param name="logsHubContext"></param>
    /// <param name="serviceProvider">DI</param>
    public ContainerLogsWatcher(IKubernetes kubernetes, IHubContext<LogsHub, ILogClient> logsHubContext,  IServiceProvider serviceProvider)
    {
        _logger = serviceProvider.GetRequiredService<ILogger<ContainerLogsWatcher>>();
        _kubernetes = kubernetes;
        _logsHubContext = logsHubContext;
    }

    /// <summary>
    /// Watch a container for logs and broadcast each log line to the SignalR group.
    /// </summary>
    /// <param name="connectionId"></param>
    /// <param name="stoppingToken"></param>
    /// <param name="containerLogsRequest"></param>
    public async Task WatchPodLogs(ContainerLogsRequest containerLogsRequest, string connectionId, CancellationToken stoppingToken)
    {
        try
        {
            _logger.Log(LogLevel.Information, "Preparing to watch logs for {0} for {1}.", containerLogsRequest.Key, connectionId);
            var response = await _kubernetes.CoreV1.ReadNamespacedPodLogWithHttpMessagesAsync(
                containerLogsRequest.PodName, containerLogsRequest.Namespace, containerLogsRequest.ContainerName,
                follow: true, cancellationToken: stoppingToken);
            await using var stream = response.Body;
            using var reader = new StreamReader(stream);
            while (!stoppingToken.IsCancellationRequested && await reader.ReadLineAsync(stoppingToken) is { } line)
            {
                await _logsHubContext.Clients.Client(connectionId).ReceiveLogMessage(new LogMessage(line));
            }

            //todo: what happens if container ends... need abort the connection.

            _logger.LogInformation("WatchPodLogs loop ended.");
        }
        catch (TaskCanceledException canceledException)
        {
            _logger.LogInformation("Cancelled watch of {0} for connection {1}.", containerLogsRequest.Key, connectionId);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An error occurred watching {0}", containerLogsRequest.Key);
        }
        finally
        {
            _logger.Log(LogLevel.Information, "Completed watch of {0} for {1}.", containerLogsRequest.Key, connectionId);
        }
    }
    
    /*

    public void AddSubscriber()
    {
        _logger.LogInformation("Adding subscriber.");
        Interlocked.Increment(ref _subscriberCount);
    }

    /// <summary>
    /// returns true if subscriber count updated to zero.
    /// </summary>
    /// <returns></returns>
    public async Task<bool> RemoveSubscriber()
    {
        _logger.LogInformation("Removing subscriber.");
        var updated = Interlocked.Decrement(ref _subscriberCount);
        if (updated == 0)
        {
            _logger.LogInformation("No subscribers remaining. Cancelling logs watch on {0}.", containerLogsRequest.Key);
            await _cancellationTokenSource.CancelAsync();
            IsWatching = false;
        }

        return updated == 0;
    }
    */
}