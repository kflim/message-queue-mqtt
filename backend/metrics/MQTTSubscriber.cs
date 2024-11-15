using System.Text;
using MQTTnet;
using MQTTnet.Client;

namespace backend.metrics;

public class MQTTSubscriber
{
    private readonly IMqttClient _client;
    private readonly HashSet<string> receivedMessages = [];

    public MQTTSubscriber(string brokerUrl, string topic, int maxRetries = 50, int delayInMilliseconds = 10000)
    {
        var factory = new MqttFactory();
        _client = factory.CreateMqttClient();
        var options = new MqttClientOptionsBuilder()
            .WithTcpServer(brokerUrl)
            .WithClientId(Guid.NewGuid().ToString()) // Generates a unique client ID
            .Build();

        int attempt = 0;
        bool connected = false;

        while (attempt < maxRetries && !connected)
        {
            try
            {
                _client.ConnectAsync(options).Wait();
                connected = true;
            }
            catch (Exception ex)
            {
                attempt++;
                if (attempt >= maxRetries)
                {
                    // Log the failure after reaching max retries
                    Console.WriteLine($"Failed to connect after {maxRetries} attempts. Error: {ex.Message}");
                    throw;  // Optionally re-throw or handle as needed
                }

                // Log the retry attempt and wait before retrying
                Console.WriteLine($"Connection attempt {attempt} failed. Retrying in {delayInMilliseconds}ms...");
                Task.Delay(delayInMilliseconds).Wait(); // Fake delay for retry
            }
        }

        // Once connected, subscribe to the topic
        _client.SubscribeAsync(topic).Wait();

        _client.ApplicationMessageReceivedAsync += e =>
        {
            var message = Encoding.UTF8.GetString(e.ApplicationMessage.PayloadSegment);
            var messageId = message.Split(':')[0];
            var processingStartTime = DateTime.UtcNow;

            if (receivedMessages.Add(messageId))
            {
                MetricsLogger.LogReceivedMessage("MQTTSubscriber", messageId);
            }
            else
            {
                MetricsLogger.LogDuplicateMessage("MQTTSubscriber", messageId);
            }

            var processingDuration = DateTime.UtcNow - processingStartTime;
            Console.WriteLine($"Message processing latency: {processingDuration.TotalMilliseconds} ms");
            MetricsLogger.LogPublishLatency("MQTTSubscriber", (long)processingDuration.TotalMilliseconds);

            // Log system resource usage (optional)
            var cpuUsage = SystemResourceMonitor.GetCpuUsage();
            var availableMemory = SystemResourceMonitor.GetAvailableMemory();
            Console.WriteLine($"CPU Usage: {cpuUsage}% | Available Memory: {availableMemory} MB");

            return Task.CompletedTask;
        };
    }

}
