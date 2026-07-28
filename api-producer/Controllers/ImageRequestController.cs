using Microsoft.AspNetCore.Mvc;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace api_producer.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ImageRequestController : ControllerBase
    {
        // Endpoint ASYNC con RabbitMQ
        [HttpPost("generate")]
        public async Task<IActionResult> GenerateRabbit([FromBody] ImageGenerationRequest request)
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

                // Pubblica N job (1 job = 1 immagine)
                for (int i = 0; i < request.Quantity; i++)
                {
                    var job = new ImageJob
                    {
                        RequestId = request.RequestId,
                        Prompt = request.Prompt,
                        Index = i,
                        Total = request.Quantity,
                    };

                    var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(job));

                    // publica il lavoro su RabbitMQ
                    await channel.BasicPublishAsync(exchange: exchangeName, routingKey: routingKey, body: body);
                }

                // Risposta immediata ad Angular    FIRE-AND-FORGET
                return Ok(new ApiResponse
                {
                    Message = "Richiesta presa in carico (RabbitMQ)!",
                    Details = $"Avviata generazione di {request.Quantity} immagini.",
                    RequestId = request.RequestId
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERRORE] RabbitMQ: {ex.Message}");
                return StatusCode(500, "Errore interno: RabbitMQ non risponde.");
            }
        }

        // Endpoint SYNC senza RabbitMQ (per confronto in presentazione)
        [HttpPost("generate-direct")]
        public async Task<IActionResult> GenerateDirect([FromBody] ImageGenerationRequest request)
        {
            #region Api lenta che genera senza rabbitmq
            request.RequestId = Guid.NewGuid().ToString();

            var apiKey = Environment.GetEnvironmentVariable("GOOGLE_API_KEY") ?? "";
            if (string.IsNullOrEmpty(apiKey))
                return StatusCode(500, "Manca GOOGLE_API_KEY.");

            try
            {
                // salva dove l'API già serve le immagini: /generated-images -> /images
                string folderPath = Path.Combine(Directory.GetCurrentDirectory(), "generated-images");
                if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);

                // genera tutte qui (blocca la request — perfetto per demo)
                for (int i = 0; i < request.Quantity; i++)
                {
                    await GenerateAndSaveImageDirect(apiKey, request.Prompt, request.RequestId, i, folderPath);
                }

                return Ok(new ApiResponse
                {
                    Message = "Generazione completata (senza Rabbit)!",
                    Details = $"Generate {request.Quantity} immagini in modo sincrono.",
                    RequestId = request.RequestId
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERRORE] Direct: {ex.Message}");
                return StatusCode(500, $"Errore generazione diretta: {ex.Message}");
            }
            #endregion
        }

        private static async Task GenerateAndSaveImageDirect(string apiKey, string prompt, string requestId, int index, string folderPath)
        {
            #region Lavoro pesante senza rabbitmq

            string url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash-image:generateContent?key={apiKey}";

            var payload = new
            {
                contents = new[]
                {
                new { parts = new[] { new { text = prompt } } }
            },
                generationConfig = new
                {
                    responseModalities = new[] { "IMAGE" },
                    imageConfig = new { aspectRatio = "1:1" }
                }
            };

            using var client = new HttpClient();
            var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
            var response = await client.PostAsync(url, content);
            var responseString = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new Exception($"Google Error ({response.StatusCode}): {responseString}");

            var jsonNode = JsonNode.Parse(responseString);
            var base64Data = jsonNode?["candidates"]?[0]?["content"]?["parts"]?[0]?["inlineData"]?["data"]?.ToString();

            if (string.IsNullOrEmpty(base64Data))
                throw new Exception("Nessun dato immagine trovato nel JSON.");

            byte[] imageBytes = Convert.FromBase64String(base64Data);
            var fullPath = Path.Combine(folderPath, $"{requestId}_{index}.jpg");
            await System.IO.File.WriteAllBytesAsync(fullPath, imageBytes);

            #endregion
        }
    }
    #region DtoClasses
    // DTO request che arriva da Angular
    public class ImageGenerationRequest
    {
        public string? RequestId { get; set; }
        public string Prompt { get; set; } = "";
        public int Quantity { get; set; } = 1;

        // 👇 toggle UI: Rabbit ON/OFF
        public bool UseRabbit { get; set; } = true;
    }

    // DTO job per RabbitMQ (1 job = 1 immagine)
    public class ImageJob
    {
        public string RequestId { get; set; } = "";
        public string Prompt { get; set; } = "";
        public int Index { get; set; }
        public int Total { get; set; }
    }

    public class ApiResponse
    {
        public string Message { get; set; } = "";
        public string RequestId { get; set; } = "";
        public string? Details { get; set; }
    }
    #endregion
}
