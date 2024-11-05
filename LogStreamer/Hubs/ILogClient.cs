using LogStreamer.Model;

namespace LogStreamer.Hubs;

/// <summary>
/// Defines functions on the client we can call.
/// </summary>
public interface ILogClient
{
    /// <summary>
    /// Receive log message from server.
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    Task ReceiveLogMessage(LogMessage message);
    
    Task ReceiveStatusMessage(string message);

    Task ServerError(string errorMessage);
}