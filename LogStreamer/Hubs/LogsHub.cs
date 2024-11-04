using LogStreamer.Model;
using LogStreamer.Services;
using Microsoft.AspNetCore.SignalR;

namespace LogStreamer.Hubs;

public class LogsHub(IContainerLogsWatcher containerLogsWatcher, IContainerLogManager containerLogManager, ILogger<LogsHub> logger) : Hub<ILogClient>
{
    
    
    /// <summary>
    /// Client requests logs for a container
    /// </summary>
    /// <param name="request"></param>
    public async Task GetLogs(ContainerLogsRequest request)
    {
        logger.LogInformation("Received call to {0}from {1} for container {2}.", nameof(GetLogs), Context.ConnectionId, request.Key);
        if (string.IsNullOrWhiteSpace(request.ContainerName))
        {  
            // todo - validate request
            await Clients.Caller.ReceiveServerError("No container name specified.");
            return;
        }
        
        var cts = new CancellationTokenSource();
        containerLogManager.Add(Context.ConnectionId, cts);
        await containerLogsWatcher.WatchPodLogs(request, Context.ConnectionId, cts.Token);
    }
    
    // lifecycle hooks
    public override async Task OnConnectedAsync()
    {
        // todo: increment connected clients metric
        await base.OnConnectedAsync();
    }
    
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        logger.LogInformation("Cancelling watch of {0}.", Context.ConnectionId);
        if (containerLogManager.TryGet(Context.ConnectionId, out var cts) && cts != null)
        {
            await cts.CancelAsync();
        }
        
        await base.OnDisconnectedAsync(exception);
    }
}