namespace backend.metrics;

public static class MetricsLogger
{
    private static string filePath = "metrics_log.txt";

    public static void ClearLogFile()
    {
        File.WriteAllText(filePath, string.Empty); // This clears the content of the file
    }

    public static void LogPublishLatency(string source, long latency)
    {
        File.AppendAllText(filePath, $"Source: {source}, Publish Latency: {latency} ms\n");
    }

    public static void LogReceivedMessage(string source, string messageId)
    {
        File.AppendAllText(filePath, $"Source: {source}, Received Message ID: {messageId}\n");
    }

    public static void LogDuplicateMessage(string source, string messageId)
    {
        File.AppendAllText(filePath, $"Source: {source}, Duplicate Message ID: {messageId}\n");
    }
}
