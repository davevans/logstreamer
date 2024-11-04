using LogStreamer.Model;

namespace LogStreamer.Services;

public interface IContainerLogManager
{
    void Add(string connectionId, CancellationTokenSource? cts);
    bool TryGet(string connectionId, out CancellationTokenSource? cts);
    void Remove(string connectionId);
}