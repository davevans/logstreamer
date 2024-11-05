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
    /// <param name="request"></param>
    public async Task WatchPodLogs(ContainerLogsRequest request, string connectionId, CancellationToken stoppingToken)
    {
        try
        {
            _logger.Log(LogLevel.Information, "Preparing to watch logs for {0} for {1}.", request.Key, connectionId);
            var response = await _kubernetes.CoreV1.ReadNamespacedPodLogWithHttpMessagesAsync(
                request.PodName, request.Namespace, request.ContainerName,
                follow: true, 
                previous: request.Previous,
                cancellationToken: stoppingToken);
            using var reader = new StreamReader(response.Body);
            while (!stoppingToken.IsCancellationRequested && await reader.ReadLineAsync(stoppingToken) is { } line)
            {
                await _logsHubContext.Clients.Client(connectionId).ReceiveLogMessage(new LogMessage(line));
            }

            _logger.LogInformation("WatchPodLogs loop ended.");
        }
        
        catch (TaskCanceledException)
        {
            _logger.LogInformation("Cancelled watch of {0} for connection {1}.", request.Key, connectionId);
        }
        finally
        {
            _logger.Log(LogLevel.Information, "Completed watch of {0} for {1}.", request.Key, connectionId);
        }
    }
}