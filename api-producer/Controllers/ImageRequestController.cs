using Microsoft.AspNetCore.Mvc;
using RabbitMQ.Client;
using System.Text;

namespace api_producer.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ImageRequestController : ControllerBase
    {
        [HttpPost("generate")]
        public IActionResult RequestImageGeneration([FromBody] string prompt)
        {
            var factory = new ConnectionFactory { HostName = "rabbitmq" };
            using var connection = factory.CreateConnection();
            using var channel = connection.CreateModel();

            // 2. Nombres (DEBEN SER IGUALES EN EL CONSUMER)
            const string ExchangeName = "image_requests_exchange";
            const string QueueName = "image_requests_queue";
            const string RoutingKey = "image_request";

            // 3. Declaración: Crea el Exchange y la Cola si no existen
            channel.ExchangeDeclare(exchange: ExchangeName, type: ExchangeType.Direct);
            channel.QueueDeclare(queue: QueueName, durable: false, exclusive: false, autoDelete: false, arguments: null);
            channel.QueueBind(queue: QueueName, exchange: ExchangeName, routingKey: RoutingKey);

            // 4. Publicación
            var body = Encoding.UTF8.GetBytes(prompt);

            channel.BasicPublish(exchange: ExchangeName,
                                 routingKey: RoutingKey,
                                 basicProperties: null,
                                 body: body);

            // === FIN DEL ENVÍO ===

            // Devuelve una respuesta inmediata al frontend (Angular)
            return Ok(new { message = $"Solicitud recibida y encolada para: '{prompt}'." });
        }
    }
}
