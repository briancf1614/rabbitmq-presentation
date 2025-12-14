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

                    // --- TOPOLOGIA (Deve essere identica all'API) ---
                    const string exchangeName = "image_requests_exchange";
                    const string queueName = "image_requests_queue";
                    const string routingKey = "image_request";

                    await channel.ExchangeDeclareAsync(exchangeName, ExchangeType.Direct);
                    await channel.QueueDeclareAsync(queue: queueName, durable: false, exclusive: false, autoDelete: false, arguments: null);
                    await channel.QueueBindAsync(queueName, exchangeName, routingKey);

                    _logger.LogInformation(" [*] In attesa di messaggi...");

                    var consumer = new AsyncEventingBasicConsumer(channel);

                    consumer.ReceivedAsync += async (model, ea) =>
                    {
                        byte[] body = ea.Body.ToArray();
                        var messageString = Encoding.UTF8.GetString(body);

                        try
                        {
                            // 1. DESERIALIZZIAMO IL JSON
                            var request = JsonSerializer.Deserialize<ImageGenerationRequest>(messageString);

                            if (request != null)
                            {
                                _logger.LogInformation($"Ricevuto ordine: {request.Prompt} (Quantità: {request.Quantity})");

                                // 2. CICLO PER LA QUANTITÀ (Scaling interno)
                                for (int i = 0; i < request.Quantity; i++)
                                {
                                    _logger.LogInformation($" -> Generazione immagine {i + 1} di {request.Quantity}...");

                                    // Chiamata a Google (o logica fake per test)
                                    await GenerateAndSaveImage(request.Prompt, i, request.RequestId);
                                }

                                _logger.LogInformation(" [V] Ordine completato.");
                            }

                            // 3. INVIO ACK (Conferma che abbiamo finito)
                            await channel.BasicAckAsync(deliveryTag: ea.DeliveryTag, multiple: false);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError($"Errore durante l'elaborazione: {ex.Message}");
                            // In caso di errore grave, potresti fare un BasicNack per rimettere in coda
                            // await channel.BasicNackAsync(ea.DeliveryTag, false, true); 
                        }
                    };

                    await channel.BasicConsumeAsync(queueName, autoAck: false, consumer: consumer);

                    // Mantiene il servizio attivo
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

            if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);

            string apiKey = Environment.GetEnvironmentVariable("GOOGLE_API_KEY");
            if (string.IsNullOrEmpty(apiKey))
            {
                _logger.LogError("Manca GOOGLE_API_KEY");
                return;
            }

            try
            {
                _logger.LogInformation($"🎨 Chiedo a Gemini 2.5 Flash: {prompt}");

                // 1. URL DALLA TUA DOCUMENTAZIONE
                string url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash-image:generateContent?key={apiKey}";

                // 2. PAYLOAD JSON (Come da documentazione CURL)
                // Nota: "responseModalities": ["IMAGE"] è il trucco per avere le immagini!
                var payload = new
                {
                    contents = new[]
                    {
                        new { parts = new[] { new { text = prompt } } }
                    },
                    generationConfig = new
                    {
                        responseModalities = new[] { "IMAGE" }, // <--- IMPORTANTE
                        imageConfig = new { aspectRatio = "1:1" }
                    }
                };

                string jsonPayload = JsonSerializer.Serialize(payload);
                var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

                using var client = new HttpClient();
                var response = await client.PostAsync(url, content);
                string responseString = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception($"Google Error ({response.StatusCode}): {responseString}");
                }

                // 3. PARSING DELLA RISPOSTA (Secondo la struttura Gemini)
                // Cerca: candidates[0].content.parts[0].inlineData.data
                var jsonNode = JsonNode.Parse(responseString);

                // Navighiamo nel JSON in modo sicuro
                var base64Data = jsonNode?["candidates"]?[0]?["content"]?["parts"]?[0]?["inlineData"]?["data"]?.ToString();

                if (!string.IsNullOrEmpty(base64Data))
                {
                    byte[] imageBytes = Convert.FromBase64String(base64Data);
                    await File.WriteAllBytesAsync(fullPath, imageBytes);
                    _logger.LogInformation($"✅ Immagine salvata: {fullPath}");
                }
                else
                {
                    throw new Exception("Nessun dato immagine trovato nel JSON.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Errore generazione: {ex.Message}");
            }
        }

        // CLASSE DTO (Deve essere identica a quella dell'API)
        public class ImageGenerationRequest
        {
            public string RequestId { get; set; } // <--- NUOVO CAMPO
            public string Prompt { get; set; }
            public int Quantity { get; set; } = 1;
            public bool AddExtraEffect { get; set; }
        }
    }
}
