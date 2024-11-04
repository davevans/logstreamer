namespace LogStreamer.Model;

public record ContainerLogsRequest(string Namespace, string PodName, string ContainerName)
{
    public string Key => $"{Namespace}-{PodName}-{ContainerName}";
}