using LogStreamer.Model;

namespace LogStreamer.Hubs;

/// <summary>
/// Defines functions on the client we can call.
/// </summary>
public interface ILogClient
{
    Task ReceiveLogMessage(LogMessage message);
    Task ReceiveLogMessages(IEnumerable<LogMessage> messages);
    
    /// <summary>
    /// Tells client the container they requested cannot be found.
    /// </summary>
    /// <param name="containerName"></param>
    /// <returns></returns>
    Task ContainerNotFound(string? containerName);

    Task ReceiveServerError(string message);
}