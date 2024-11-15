using System.Diagnostics;
using MQTTnet;
using MQTTnet.Client;

namespace backend.metrics
{
    public class MQTTPublisher(string brokerUrl)
    {
        private readonly Stopwatch stopwatch = new Stopwatch();
        private readonly int maxAttempts = 50;
        private readonly int retryDelayMilliseconds = 10000;
        private readonly string brokerUrl = brokerUrl;

        public async Task PublishAsync(string topic, string message)
        {
            // Artificial 1-minute delay before attempting connection
            Console.WriteLine("Starting 1-minute delay before attempting MQTT connection...");
            await Task.Delay(60000);
            Console.WriteLine("Delay complete. Attempting MQTT connection...");

            var mqttFactory = new MqttFactory();
            var mqttClientOptions = new MqttClientOptionsBuilder()
                .WithTcpServer("broker.hivemq.com")
                .Build();

            int attempt = 0;
            while (attempt < maxAttempts)
            {
                try
                {
                    using var mqttClient = mqttFactory.CreateMqttClient();
                    
                    stopwatch.Restart();
                    await mqttClient.ConnectAsync(mqttClientOptions, CancellationToken.None);
                    Console.WriteLine("Connected to MQTT broker.");

                    var mqttMessage = new MqttApplicationMessageBuilder()
                        .WithTopic(topic)
                        .WithPayload(message)
                        .WithRetainFlag()
                        .Build();

                    await mqttClient.PublishAsync(mqttMessage, CancellationToken.None);
                    await mqttClient.DisconnectAsync();

                    stopwatch.Stop();
                    var publishLatency = stopwatch.ElapsedMilliseconds;
                    Console.WriteLine($"Message published with latency: {publishLatency} ms");
                    
                    // Log the publish latency
                    MetricsLogger.LogPublishLatency("MQTTPublisher", publishLatency);
                    break;
                }
                catch (Exception ex)
                {
                    attempt++;
                    Console.WriteLine($"Connection attempt {attempt} failed: {ex.Message}");

                    if (attempt < maxAttempts)
                    {
                        Console.WriteLine($"Retrying in {retryDelayMilliseconds}ms...");
                        await Task.Delay(retryDelayMilliseconds);
                    }
                    else
                    {
                        Console.WriteLine("Failed to connect after maximum attempts.");
                    }
                }
            }
        }
    }
}
