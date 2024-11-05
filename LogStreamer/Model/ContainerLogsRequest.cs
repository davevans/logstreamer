namespace LogStreamer.Model;

public record ContainerLogsRequest(string Namespace, string PodName, string ContainerName, bool Previous)
{
    public string Key => $"{Namespace}-{PodName}-{ContainerName}";

    public string ToLogString()
    {
        return $"{Namespace}/{PodName}/{ContainerName}";
    }
}