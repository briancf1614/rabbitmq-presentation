using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

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
                                    await GenerateAndSaveImage(request.Prompt, i);
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

        private async Task GenerateAndSaveImage(string prompt, int index)
        {
            try
            {
                // LEGGIAMO LA CHIAVE SEGRETA DALL'AMBIENTE (Iniettata da Docker)
                string apiKey = Environment.GetEnvironmentVariable("GOOGLE_API_KEY");

                // Qui dovresti mettere la tua logica reale verso Google Custom Search API
                // Per la demo, simulo un download o creo un file dummy se non vuoi bruciare la quota API

                // SIMULAZIONE (Sostituisci con la tua logica Google reale se vuoi)
                // Usiamo un placeholder online per testare che salvi il file
                string dummyImageUrl = $"https://placehold.co/600x400?text={Uri.EscapeDataString(prompt + " " + index)}";
                byte[] imageBytes = await _httpClient.GetByteArrayAsync(dummyImageUrl);

                // --- SALVATAGGIO SUL VOLUME CONDIVISO ---
                // Il percorso /app/generated-images è mappato nel docker-compose
                string folderPath = "/app/generated-images";

                // Assicuriamoci che la cartella esista
                if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);

                // Nome file unico: timestamp_prompt_index.jpg
                string safePrompt = prompt.Replace(" ", "_").Substring(0, Math.Min(10, prompt.Length)); // Primi 10 caratteri
                string fileName = $"{DateTime.Now:yyyyMMdd_HHmmss}_{safePrompt}_{index}.jpg";
                string fullPath = Path.Combine(folderPath, fileName);

                await File.WriteAllBytesAsync(fullPath, imageBytes);

                _logger.LogInformation($"File salvato: {fullPath}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Errore nel download/salvataggio immagine: {ex.Message}");
            }
        }
    }
}
