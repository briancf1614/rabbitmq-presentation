using Microsoft.AspNetCore.Mvc;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace api_producer.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ImageRequestController : ControllerBase
    {
        // Endpoint: POST api/ImageRequest/generate
        [HttpPost("generate")]
        public async Task<IActionResult> RequestImageGeneration([FromBody] ImageGenerationRequest request)
        {
            request.RequestId = Guid.NewGuid().ToString();

            try
            {
                var factory = new ConnectionFactory { HostName = "rabbitmq" };

                await using var connection = await factory.CreateConnectionAsync();
                await using var channel = await connection.CreateChannelAsync();

                const string exchangeName = "image_requests_exchange";
                const string queueName = "image_requests_queue";
                const string routingKey = "image_request";

                await channel.ExchangeDeclareAsync(exchangeName, ExchangeType.Direct, durable: false, autoDelete: false);
                await channel.QueueDeclareAsync(queue: queueName, durable: false, exclusive: false, autoDelete: false, arguments: null);
                await channel.QueueBindAsync(queueName, exchangeName, routingKey);

                // 🔥 Fan-out: publicamos 1 mensaje por imagen
                for (int i = 0; i < request.Quantity; i++)
                {
                    var job = new ImageJob
                    {
                        RequestId = request.RequestId,
                        Prompt = request.Prompt,
                        Index = i,
                        Total = request.Quantity,
                        AddExtraEffect = request.AddExtraEffect
                    };

                    var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(job));
                    await channel.BasicPublishAsync(exchange: exchangeName, routingKey: routingKey, body: body);
                }

                Console.WriteLine($"[API] Pubblicati {request.Quantity} job per requestId={request.RequestId}");

                return Ok(new
                {
                    message = "Richiesta presa in carico!",
                    details = $"Generazione di {request.Quantity} immagini avviata.",
                    requestId = request.RequestId,
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERRORE] RabbitMQ: {ex.Message}");
                return StatusCode(500, "Errore interno: Il servizio di coda non risponde.");
            }
        }
    }

    public class ImageGenerationRequest
    {
        public string? RequestId { get; set; }
        public string Prompt { get; set; } = "";
        public int Quantity { get; set; } = 1;
        public bool AddExtraEffect { get; set; }
    }

    public class ImageJob
    {
        public string RequestId { get; set; } = "";
        public string Prompt { get; set; } = "";
        public int Index { get; set; }
        public int Total { get; set; }
        public bool AddExtraEffect { get; set; }
    }
}
