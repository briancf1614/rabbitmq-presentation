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
                // === LOGICA RABBITMQ (Versione Async) ===

                // 2. Connessione al container "rabbitmq" (nome del servizio nel docker-compose)
                var factory = new ConnectionFactory { HostName = "rabbitmq" };

                // 3. Creiamo connessione e canale
                // "await using" assicura che vengano chiusi e puliti alla fine della richiesta
                await using var connection = await factory.CreateConnectionAsync();
                await using var channel = await connection.CreateChannelAsync();

                // 4. Definiamo la Topologia (Idempotente: se esiste già, non fa nulla)
                // Usiamo costanti (stringhe) che devono essere UGUALI nel Worker
                const string exchangeName = "image_requests_exchange";
                const string queueName = "image_requests_queue";
                const string routingKey = "image_request";

                await channel.ExchangeDeclareAsync(exchangeName, ExchangeType.Direct);
                await channel.QueueDeclareAsync(queue: queueName, durable: false, exclusive: false, autoDelete: false, arguments: null);
                await channel.QueueBindAsync(queueName, exchangeName, routingKey);

                // 5. Prepariamo il Messaggio (Payload)
                // Serializziamo tutto l'oggetto (Prompt + Quantità + Opzioni) in JSON
                string jsonPayload = JsonSerializer.Serialize(request);
                var body = Encoding.UTF8.GetBytes(jsonPayload);

                // 6. Pubblichiamo il messaggio nell'Exchange
                await channel.BasicPublishAsync(exchange: exchangeName, routingKey: routingKey, body: body);

                // =========================================

                Console.WriteLine($"[API] Messaggio inviato: {request.Prompt} (x{request.Quantity})");

                // Rispondiamo subito all'utente (Angular) senza farlo aspettare
                return Ok(new
                {
                    message = "Richiesta presa in carico!",
                    details = $"Generazione di {request.Quantity} immagini avviata.",
                    requestId = request.RequestId,
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERRORE] Impossibile connettersi a RabbitMQ: {ex.Message}");
                return StatusCode(500, "Errore interno: Il servizio di coda non risponde.");
            }
        }
    }

    public class ImageGenerationRequest
    {
        public string? RequestId { get; set; }
        public string Prompt { get; set; }
        public int Quantity { get; set; } = 1;
        public bool AddExtraEffect { get; set; }
    }
}
