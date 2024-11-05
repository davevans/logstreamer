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
            await Clients.Caller.ReceiveStatusMessage("Invalid request");
            return;
        }
        
        var cts = new CancellationTokenSource();
        containerLogManager.Add(Context.ConnectionId, cts);
        try
        {
            await containerLogsWatcher.WatchPodLogs(request, Context.ConnectionId, cts.Token);
            
            logger.LogInformation("Finished for connection {0}.", Context.ConnectionId);
            
            // tell client is game over
            await Clients.Caller.ReceiveStatusMessage("No more logs coming.");
            Context.Abort();
        }
        catch (k8s.Autorest.HttpOperationException ex) when (ex.Message.Contains("NotFound"))
        {
            logger.LogInformation($"Container not found. {request.Namespace} {request.PodName} {request.ContainerName}");
            //await Clients.Caller.ContainerNotFound(request.Namespace, request.PodName, request.ContainerName);
            await Clients.Caller.ReceiveStatusMessage("Container not found");
            Context.Abort();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred watching {0}", request.Key);
            await Clients.Caller.ServerError($"An error occurred watching logs for {request.Key}.");
            Context.Abort();
        }
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