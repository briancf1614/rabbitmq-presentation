# 🐰 RabbitMQ .NET Demo: Architettura Asincrona e Scalabile

Questo progetto è una **Demo Didattica** creata per dimostrare i vantaggi di un'architettura a microservizi disaccoppiata basata su messaggi (Message Broker) rispetto a un'architettura monolitica sincrona.

Il caso d'uso reale simulato è la **Generazione di Immagini tramite AI (Google Gemini)**, un processo intrinsecamente lento e pesante.

## 🎯 Obiettivo della Demo

L'obiettivo è confrontare visivamente e tecnicamente due approcci:

1.  ❌ **Approccio Sincrono (Bloccante):** L'API chiama direttamente il servizio AI. L'utente aspetta, l'interfaccia si blocca, il server rischia il timeout.
2.  ✅ **Approccio Asincrono (RabbitMQ):** L'API delega il lavoro a una coda. L'utente riceve risposta immediata ("Fire and Forget"), mentre i **Worker** lavorano in background.

## 🏗️ Architettura del Progetto

Il sistema è composto da tre parti distinte:

### 1. API Producer (`/api-producer`)
* **Tecnologia:** .NET 8 Web API.
* **Ruolo:** Riceve le richieste dall'utente.
* **Endpoint A (Rabbit):** Invia un messaggio alla coda e restituisce subito `200 OK`.
* **Endpoint B (Direct):** Esegue il lavoro pesante bloccando la chiamata HTTP (per confronto).

### 2. Worker Service (`/worker-service`)
* **Tecnologia:** .NET 8 Worker Service (Console App).
* **Ruolo:** Consumatore (Consumer). Ascolta la coda `image_requests_queue`.
* **Funzionamento:**
    * Preleva un messaggio.
    * Chiama le API di Google Gemini per generare l'immagine.
    * Invia un **ACK** (Conferma) manuale solo a lavoro finito.
    * Gestisce errori e crash con logiche di **Retry/NACK**.

### 3. Frontend Client (`/frontend-angular`)
* **Tecnologia:** Angular (Generato da IA).
* **Nota:** Questo frontend serve **esclusivamente come trigger grafico** per la demo. Il codice non è oggetto di studio.

---

## ⚡ Caratteristiche Chiave Dimostrate

* **Fire and Forget:** L'API risponde in millisecondi indipendentemente dal carico di lavoro.
* **Scalabilità Orizzontale:** Possiamo avviare 1, 10 o 50 istanze del `Worker Service` per smaltire la coda più velocemente senza toccare l'API.
* **Resilienza:** Se un Worker crasha mentre genera un'immagine, RabbitMQ (grazie al meccanismo di ACK manuale) rimette il messaggio in coda per un altro Worker. Nessun dato viene perso.
* **Gestione dei Picchi:** Se arrivano 1000 richieste al secondo, il server non esplode. I messaggi si accumulano nella coda e vengono smaltiti a velocità costante.

---

## 🚀 Come Avviare il Progetto

### Prerequisiti
* [.NET 10 SDK](https://dotnet.microsoft.com/download)
* [Docker](https://www.docker.com/) (per RabbitMQ)
* Una API Key di Google Gemini (variabile d'ambiente: `GOOGLE_API_KEY`)

### 1. Avvia RabbitMQ
Esegui il container RabbitMQ con l'interfaccia di gestione:
```bash
docker run -d --hostnamemy-rabbit --name some-rabbit -p 15672:15672 -p 5672:5672 rabbitmq:4-management
