using Mscc.GenerativeAI;
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
            try
            {
                string apiKey = Environment.GetEnvironmentVariable("GOOGLE_API_KEY");
                if (string.IsNullOrEmpty(apiKey)) throw new Exception("Manca GOOGLE_API_KEY");

                // --- QUI INIZIA LA MAGIA DELLA LIBRERIA ---

                // 1. Inizializziamo Google AI
                var googleAI = new GoogleAI(apiKey);

                // 2. Selezioniamo il modello
                // NOTA IMPORTANTE: Per generare immagini si usa solitamente "imagen-3.0-generate-001".
                // "gemini-flash" di solito è per testo/multimodale in input.
                var model = googleAI.GenerativeModel("imagen-3.0-generate-001");

                // 3. Richiediamo l'immagine
                // La libreria Mscc.GenerativeAI potrebbe richiedere un metodo specifico per le immagini
                // a seconda della versione, ma GenerateContent è standard per i modelli generici.
                var response = await model.GenerateContent(prompt);

                // 4. Estraiamo l'immagine (Base64)
                // La struttura dipende da cosa restituisce esattamente il modello Imagen tramite questa libreria.
                if (response.Candidates?[0].Content?.Parts?[0].InlineData != null)
                {
                    var base64Data = response.Candidates[0].Content.Parts[0].InlineData.Data;
                    byte[] imageBytes = Convert.FromBase64String(base64Data);

                    // 5. Salvataggio su file
                    string fileName = $"{requestId}_{index}.jpg";
                    string folderPath = "/app/generated-images"; // Assicurarsi che il path esista nel container

                    if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);

                    string fullPath = Path.Combine(folderPath, fileName);

                    await File.WriteAllBytesAsync(fullPath, imageBytes);
                    _logger.LogInformation($"Immagine salvata: {fullPath}");
                }
                else
                {
                    _logger.LogWarning("La risposta di Google non conteneva dati immagine (InlineData null).");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Errore nella generazione AI: {ex.Message}");
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
