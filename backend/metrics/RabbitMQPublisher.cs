using System.Diagnostics;
using System.Text;
using RabbitMQ.Client;

namespace backend.metrics
{
  public class RabbitMQPublisher(string hostName, string queueName)
    {
    private readonly string _hostName = hostName;
    private readonly string _queueName = queueName;

      public async Task PublishAsync(string message)
      {
        var factory = new ConnectionFactory() { HostName = _hostName };

        using var connection = await factory.CreateConnectionAsync();
        using var channel = await connection.CreateChannelAsync();
        // Declare the queue if it doesn’t already exist
        await channel.QueueDeclareAsync(queue: _queueName,
                                        durable: false,
                                        exclusive: false,
                                        autoDelete: false,
                                        arguments: null);

        var body = Encoding.UTF8.GetBytes(message);

        var stopwatch = Stopwatch.StartNew();
        await channel.BasicPublishAsync(exchange: "", routingKey: _queueName, body: body);
        stopwatch.Stop();

        var publishLatency = stopwatch.ElapsedMilliseconds;
        Console.WriteLine($" [x] Sent '{message}' to queue '{_queueName}' with latency: {publishLatency} ms");

        // Log the publish latency
        MetricsLogger.LogPublishLatency("RabbitMQPublisher", publishLatency);
      }
  }
}
