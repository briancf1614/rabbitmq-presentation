using Mscc.GenerativeAI;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace worker_consumer
{
    public class Worker(ILogger<Worker> _logger, HttpClient _httpClient) : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {

                var factory = new ConnectionFactory { HostName = "rabbitmq" };
                try
                {
                    await using var connection = await factory.CreateConnectionAsync();
                    await using var channel = await connection.CreateChannelAsync();

                    const string exchangeName = "image_requests_exchange";
                    const string queueName = "image_requests_queue";
                    const string routingKey = "image_request";

                    await channel.ExchangeDeclareAsync(exchangeName, ExchangeType.Direct, durable: false, autoDelete: false);
                    await channel.QueueDeclareAsync(queue: queueName, durable: false, exclusive: false, autoDelete: false, arguments: null);
                    await channel.QueueBindAsync(queueName, exchangeName, routingKey);

                    // ✅ Reparte mejor los jobs entre workers
                    await channel.BasicQosAsync(prefetchSize: 0, prefetchCount: 1, global: false);

                    _logger.LogInformation(" [*] Worker pronto. In attesa di job...");

                    var consumer = new AsyncEventingBasicConsumer(channel);

                    consumer.ReceivedAsync += async (model, ea) =>
                    {
                        var messageString = Encoding.UTF8.GetString(ea.Body.ToArray());

                        try
                        {
                            var job = JsonSerializer.Deserialize<ImageJob>(messageString);

                            if (job == null)
                                throw new Exception("Job null / JSON invalido");

                            _logger.LogInformation($"📩 Job: {job.RequestId} img {job.Index + 1}/{job.Total}");

                            await GenerateAndSaveImage(job.Prompt, job.Index, job.RequestId);

                            // ✅ ACK solo si terminó bien
                            await channel.BasicAckAsync(deliveryTag: ea.DeliveryTag, multiple: false);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError($"❌ Errore job: {ex.Message}");

                            // Para demo: reintenta (requeue: true)
                            await channel.BasicNackAsync(deliveryTag: ea.DeliveryTag, multiple: false, requeue: true);
                        }
                    };

                    await channel.BasicConsumeAsync(queue: queueName, autoAck: false, consumer: consumer);

                    // Mantiene el worker vivo
                    await Task.Delay(Timeout.Infinite, stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError($"RabbitMQ non raggiungibile: {ex.Message}. Riprovo tra 5 sec...");
                    await Task.Delay(5000, stoppingToken);
                }
            }
        }

        private async Task GenerateAndSaveImage(string prompt, int index, string requestId)
        {
            string folderPath = "/app/generated-images";
            string fileName = $"{requestId}_{index}.jpg";
            string fullPath = Path.Combine(folderPath, fileName);

            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            string apiKey = Environment.GetEnvironmentVariable("GOOGLE_API_KEY") ?? "";
            if (string.IsNullOrEmpty(apiKey))
            {
                _logger.LogError("Manca GOOGLE_API_KEY");
                return;
            }

            try
            {
                _logger.LogInformation($"🎨 Gemini: {prompt} (index={index})");

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

                var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

                // ✅ usa el HttpClient inyectado
                var response = await _httpClient.PostAsync(url, content);
                var responseString = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                    throw new Exception($"Google Error ({response.StatusCode}): {responseString}");

                var jsonNode = JsonNode.Parse(responseString);
                var base64Data = jsonNode?["candidates"]?[0]?["content"]?["parts"]?[0]?["inlineData"]?["data"]?.ToString();

                if (string.IsNullOrEmpty(base64Data))
                    throw new Exception("Nessun dato immagine trovato nel JSON.");

                byte[] imageBytes = Convert.FromBase64String(base64Data);
                await File.WriteAllBytesAsync(fullPath, imageBytes);

                _logger.LogInformation($"✅ Salvata: {fullPath}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Errore generazione: {ex.Message}");
                throw; // 👈 importante: para que el Nack ocurra arriba
            }
        }

        // ✅ DTO del job (debe coincidir con el producer)
        public class ImageJob
        {
            public string RequestId { get; set; } = "";
            public string Prompt { get; set; } = "";
            public int Index { get; set; }
            public int Total { get; set; }
            public bool AddExtraEffect { get; set; }
        }
    }
}
