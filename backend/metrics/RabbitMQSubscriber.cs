using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace backend.metrics
{
    public class RabbitMQSubscriber(string hostName, string queueName)
    {
        private readonly string _hostName = hostName;
        private readonly string _queueName = queueName;

        public async Task StartListening()
        {
            var factory = new ConnectionFactory() { HostName = _hostName };

            var connection = await factory.CreateConnectionAsync();
            var channel = await connection.CreateChannelAsync();

            // Declare the queue in case it doesn't exist
            await channel.QueueDeclareAsync(queue: _queueName,
                                 durable: false,
                                 exclusive: false,
                                 autoDelete: false,
                                 arguments: null);

            var consumer = new AsyncEventingBasicConsumer(channel);
            consumer.ReceivedAsync += async (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);

                var startTime = DateTime.UtcNow;

                Console.WriteLine($" [x] Received '{message}'");

                // Log the message processing latency
                var processingDuration = DateTime.UtcNow - startTime;
                MetricsLogger.LogPublishLatency("RabbitMQSubscriber", (long)processingDuration.TotalMilliseconds);

                // Log system resource usage
                var cpuUsage = SystemResourceMonitor.GetCpuUsage();
                var availableMemory = SystemResourceMonitor.GetAvailableMemory();
                Console.WriteLine($"CPU Usage: {cpuUsage}% | Available Memory: {availableMemory} MB");

                await Task.CompletedTask;
            };

            await channel.BasicConsumeAsync(queue: _queueName, autoAck: true, consumer: consumer);

            Console.WriteLine($" [*] Waiting for messages in queue '{_queueName}'. Press [enter] to exit.");
            Console.ReadLine(); // Keep the subscriber running
        }
    }
}
