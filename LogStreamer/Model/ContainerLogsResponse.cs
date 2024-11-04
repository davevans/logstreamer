namespace LogStreamer.Model;

public record ContainerLogsResponse(bool ContainerFound, IEnumerable<LogMessage> LogMessages);