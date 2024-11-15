/* var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();

var app = builder.Build();

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

// Connect to the MQTT broker on startup

app.Run();
 */

using backend.metrics;

namespace backend;

public class Program {
  public static async Task Main(string[] args) {
    MetricsLogger.ClearLogFile();

    var mqttSubscriber = new MQTTSubscriber("broker.hivemq.com", "test/throughput");
    var mqttPublisher = new MQTTPublisher("broker.hivemq.com");

    var rabbitSubscriber = new RabbitMQSubscriber("localhost", "sampleQueue");
    var rabbitPublisher = new RabbitMQPublisher("localhost", "sampleQueue");

    await mqttPublisher.PublishAsync("test/throughput", "Test MQTT Message");
    
    await rabbitSubscriber.StartListening();
    await rabbitPublisher.PublishAsync("Test RabbitMQ Message");

    var timer = new System.Timers.Timer(1000);
    timer.Elapsed += (sender, e) =>
    {
        float cpuUsage = SystemResourceMonitor.GetCpuUsage();
        float availableMemory = SystemResourceMonitor.GetAvailableMemory();
        Console.WriteLine($"CPU Usage: {cpuUsage}% | Available Memory: {availableMemory} MB");
        File.AppendAllText("metrics_log.txt", $"CPU: {cpuUsage}%, Memory: {availableMemory} MB\n");
    };
    timer.Start();

    await Task.Delay(5000);
    timer.Stop();
  }
}

