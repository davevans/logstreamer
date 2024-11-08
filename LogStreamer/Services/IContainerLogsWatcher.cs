﻿using LogStreamer.Hubs;
using LogStreamer.Model;
using Microsoft.AspNetCore.SignalR;

namespace LogStreamer.Services;

public interface IContainerLogsWatcher
{
    Task WatchPodLogs(ContainerLogsRequest request, string connectionId, CancellationToken stoppingToken);
}