using System.Collections.Concurrent;

namespace LogStreamer.Services;

/// <summary>
/// 
/// </summary>
public class ContainerLogManager : IContainerLogManager
{
    private readonly ConcurrentDictionary<string, CancellationTokenSource?> _watchers = new();
    
    public void Add(string connectionId, CancellationTokenSource? cts)
    {
        _watchers.TryAdd(connectionId, cts);
    }

    public bool TryGet(string connectionId, out CancellationTokenSource? cts)
    {
        return _watchers.TryGetValue(connectionId, out cts);
    }

    public void Remove(string connectionId)
    {
        _watchers.TryRemove(connectionId, out _);
    }
}