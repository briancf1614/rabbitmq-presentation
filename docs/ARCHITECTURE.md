# 🚀 Piattaforma Microservizi Enterprise: La "Bibbia"

Questo progetto rappresenta un concentrato assoluto di ingegneria del software moderna, progettato appositamente per scopi didattici e per fungere da guida pratica ai design pattern di livello "Enterprise".

## 🏗️ Livelli di Architettura e Design Pattern

Il codice è disseminato di commenti `// EDU:` che spiegano il "perché" delle scelte tecniche proprio nel punto in cui sono implementate.

### 1. Clean Architecture & Domain-Driven Design (DDD)
Ogni microservizio (es. `OrderService`, `InventoryService`) è strutturato in 4 livelli distinti:
*   **Domain**: Il cuore. Entità ricche (Aggregate Roots) con setter privati e metodi che incapsulano la logica di business. *Non ci sono dipendenze esterne qui.*
*   **Application**: I casi d'uso. Qui implementiamo il pattern **CQRS** utilizzando la libreria `MediatR`.
*   **Infrastructure**: L'implementazione pratica delle interfacce definite nel dominio (Accesso al database con `Entity Framework Core`, Message Broker con `MassTransit`).
*   **API**: Il punto di ingresso HTTP (`Controllers`), sottile e privo di logica di business.

### 2. CQRS Rigoroso: Entity Framework Core + Dapper
Nel servizio Ordini, separiamo le Scritture (Command) dalle Letture (Query):
*   **Comandi**: Usiamo **Entity Framework Core** per gestire la logica complessa di dominio, il tracciamento delle modifiche e la consistenza dei dati.
*   **Query**: Usiamo **Dapper** (Micro-ORM) per eseguire query SQL grezze e dirette. Questo offre prestazioni estreme, bypassando l'overhead di EF Core per le semplici letture.

### 3. Sincronizzazione Avanzata: Outbox Pattern e Domain Events
*   **Domain Events**: Le entità registrano eventi interni (es. `OrderStartedDomainEvent`). Un dispatcher personalizzato intercetta il salvataggio su DB e pubblica questi eventi internamente *prima* della commit, permettendo effetti collaterali nello stesso limite transazionale.
*   **Transactional Outbox**: Per evitare il "Dual-Write Problem" (salvare sul DB e poi crashare prima di avvisare RabbitMQ), usiamo l'Outbox di MassTransit. I messaggi per il broker vengono salvati nella stessa transazione SQL del DB, e un worker li invia in modo asincrono, garantendo una consegna "At-Least-Once".

### 4. Resilienza e Concorrenza
*   **Saga Pattern (State Machine)**: Invece dei lenti lock distribuiti, usiamo una State Machine di MassTransit nell'`OrderService` per orchestrare transazioni distribuite tra microservizi (Crea Ordine -> Riserva Inventario -> Completa).
*   **Idempotent Consumer (Inbox)**: Poiché RabbitMQ può consegnare un messaggio due volte, il consumatore in `InventoryService` usa il pattern Inbox (su MongoDB) per tracciare i `MessageId`. Se un messaggio arriva due volte, viene ignorato in sicurezza.
*   **Distributed Locking (RedLock)**: Usiamo `RedLock.net` su **Redis** per evitare race condition quando migliaia di richieste tentano di aggiornare l'inventario nello stesso esatto millisecondo.
*   **Circuit Breaker & Retry**: L'API Gateway integra **Polly** per gestire i guasti temporanei dei microservizi a valle, evitando di sovraccaricare un sistema già in difficoltà.

### 5. Comunicazione Inter-Servizio Multipla
*   **Asincrona (RabbitMQ/MassTransit)**: Il canale principale per scalabilità e disaccoppiamento.
*   **Sincrona ad altissime prestazioni (gRPC)**: Usata dove la consistenza immediata è obbligatoria. Es: L'`OrderService` chiama il `CatalogService` via gRPC su HTTP/2 per validare un prodotto in millisecondi prima di iniziare un ordine.

### 6. Endpoint, API e Front-end
*   **GraphQL**: Il `CatalogService` espone i prodotti non tramite REST, ma tramite **GraphQL (HotChocolate)**, permettendo al client di decidere esattamente quali campi scaricare (risolvendo over-fetching/under-fetching).
*   **API Versioning & HATEOAS**: Le API REST (`OrderService`) sono versionate (`/api/v1/`) e ritornano link ipermediali (Livello 3 di maturità REST).
*   **SignalR (WebSockets)**: Un `NotificationService` ascolta gli eventi di RabbitMQ e "spinge" aggiornamenti in tempo reale ai client (es. "Il tuo ordine è stato processato!") senza bisogno di polling.
*   **Front-end Angular & NgRx**: L'interfaccia non è banale. Usa **NgRx** per una gestione dello stato in stile Redux, separando la logica visuale dagli effetti collaterali (chiamate HTTP).
*   **Android (Kotlin + Jetpack Compose)**: L'app nativa usa la **Clean Architecture MVVM**, dove il ViewModel mantiene lo stato dell'interfaccia utente tramite `StateFlow`.

### 7. Sicurezza, Lavori in Background e Operazioni
*   **Identità e JWT**: Un `IdentityService` emette token JWT. L'API Gateway funge da controllore centralizzato.
*   **Secrets Management**: È presente uno stub che simula un **Azure Key Vault**, dimostrando come le chiavi di sicurezza vengano iniettate a runtime e mai salvate in chiaro.
*   **Job in Background (Hangfire)**: Integrato per gestire lavori programmati e ricorrenti (es. riconciliazione notturna), persistendo lo stato dei job su PostgreSQL.
*   **Rate Limiting**: L'API Gateway protegge l'infrastruttura con un Token Bucket / Fixed Window per evitare attacchi DDoS.
*   **Output Caching**: Implementato nel `CatalogService` per servire le richieste frequenti direttamente dalla memoria.

### 8. DevOps e Osservabilità Totale
*   **OpenTelemetry, Prometheus e Grafana**: Tutti i microservizi emettono metriche e trace. Il `docker-compose` include Prometheus (per la raccolta) e Grafana (per visualizzare i cruscotti).
*   **Testcontainers (Integration Testing)**: La suite di test non usa solo mock (usiamo Moq e FluentAssertions per l'unità), ma avvia *veri* container Docker di PostgreSQL e RabbitMQ tramite **Testcontainers** per test E2E reali al 100%.
*   **CI/CD Pipeline (GitHub Actions)**: Ogni push attiva una pipeline che compila tutto e fa girare i container di test per assicurare che il codice rotto non finisca mai in produzione.
