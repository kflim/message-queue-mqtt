using Microsoft.AspNetCore.Mvc;
using MQTTnet;
using MQTTnet.Client;

namespace backend.Controllers
{
  [ApiController]
  [Route("api/mqtt")]
  public class MqttController() : ControllerBase
  {

    [HttpPost("publish")]
    public async Task<IActionResult> Publish([FromBody] MqttMessageRequest request)
    {
      if (string.IsNullOrEmpty(request.Topic) || string.IsNullOrEmpty(request.Message))
      {
        return BadRequest("Topic and message are required");
      }

      var mqttFactory = new MqttFactory();

      using (var mqttClient = mqttFactory.CreateMqttClient())
      {
        var mqttClientOptions = new MqttClientOptionsBuilder()
          .WithTcpServer("broker.hivemq.com")
          .Build();

        await mqttClient.ConnectAsync(mqttClientOptions, CancellationToken.None);

        var message = new MqttApplicationMessageBuilder()
          .WithTopic(request.Topic)
          .WithPayload(request.Message)
          .WithRetainFlag()
          .Build();

        await mqttClient.PublishAsync(message, CancellationToken.None);
        await mqttClient.DisconnectAsync();
      }

      return Ok($"Published message to topic {request.Topic}");
    }
  }

  public class MqttMessageRequest
  {
    public string? Topic { get; set; }
    public string? Message { get; set; }
  }
}
