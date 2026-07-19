# Enterprise System Architecture

This document describes the architecture of the enterprise system built using modern design patterns and technologies.

## 🏢 High-Level Architecture
The system follows a Microservices Architecture pattern, where different business domains are segregated into their respective services. These services communicate asynchronously via a Message Broker (RabbitMQ) to ensure loose coupling and high scalability.

An API Gateway (YARP) acts as the single entry point for external clients (Angular Web App and Android Mobile App), routing requests to the appropriate microservices.

### Components
1. **API Gateway (YARP)**: Built with C# and YARP. Routes incoming traffic from clients to the internal microservices. Handles cross-cutting concerns like authentication, rate limiting, and routing.
2. **Order Service**: A C# .NET microservice responsible for order management.
   - **Architecture**: Clean Architecture & CQRS (MediatR).
   - **Database**: PostgreSQL (Relational) via Entity Framework Core.
   - **Messaging**: Publishes `OrderCreatedEvent` to RabbitMQ.
3. **Inventory Service**: A C# .NET microservice responsible for managing stock.
   - **Architecture**: Clean Architecture.
   - **Database**: MongoDB (NoSQL) via official driver.
   - **Messaging**: Consumes `OrderCreatedEvent` from RabbitMQ to update stock asynchronously.
4. **Web Frontend (Angular)**: Modern Angular standalone application serving as the web interface.
5. **Mobile Frontend (Android)**: Native Android app using Kotlin and Jetpack Compose.
6. **Message Broker (RabbitMQ)**: Facilitates event-driven communication between Order and Inventory services.

## 🏗 Clean Architecture & CQRS
Both microservices use Clean Architecture, segregating the code into four layers:
- **Domain**: Contains enterprise logic, entities, and interfaces. No external dependencies.
- **Application**: Contains business rules, use cases (CQRS commands/queries via MediatR).
- **Infrastructure**: Implements interfaces defined in the Domain (Database access, Message Broker integration with MassTransit).
- **API**: The entry point (Controllers/Minimal APIs), handling HTTP requests.

## 🚀 Event-Driven Architecture
We use an event-driven approach to decouple services. When an order is placed, the `OrderService` does not directly call the `InventoryService`. Instead, it fires an event to RabbitMQ. The `InventoryService` listens to this event and reacts accordingly. This ensures that if the `InventoryService` is down, the `OrderService` can still process orders, demonstrating high availability and resilience.

## 🐳 Infrastructure & Deployment
The system is fully containerized using Docker.
- **Local Development**: `docker-compose.yml` orchestrates the services, databases, and RabbitMQ.
- **Production**: Kubernetes (`k8s`) manifests are provided for Deployments, Services, and Ingress to run in a professional, scalable cluster.

## 📚 Technology Stack Summary
- **Backend**: C# .NET 8, YARP, MediatR, MassTransit
- **Databases**: PostgreSQL, MongoDB
- **Message Broker**: RabbitMQ
- **Frontend Web**: Angular 17+ (Standalone, SCSS)
- **Frontend Mobile**: Android (Kotlin, Gradle)
- **DevOps**: Docker, Docker Compose, Kubernetes

## 🌟 Advanced Enterprise Patterns Addendum

### Transactional Outbox Pattern
To solve the dual-write problem (writing to a database and publishing an event to a message broker simultaneously), the `OrderService` implements the **Transactional Outbox** pattern using MassTransit's Entity Framework integration.
- When an order is created, the order data and the event (`OrderCreatedEvent`) are saved in the same PostgreSQL transaction.
- If the database transaction fails, neither is saved.
- If it succeeds, a background process reliably publishes the event to RabbitMQ, ensuring at-least-once delivery even in case of broker downtime.

### Domain-Driven Design (DDD)
The domain layer employs strong encapsulation:
- **Private Setters**: Entities like `Order` control their own state to prevent anemic domain models.
- **Constructors and Factories**: Objects are always created in a valid state.
- **Encapsulated Collections**: `Order` exposes `Items` as an `IReadOnlyCollection`, forcing modifications through behavior methods like `AddItem()` which also enforces business rules (recalculating totals).

### Resiliency and Cross-Cutting Concerns
- **Global Exception Handling**: A centralized middleware intercepts all unhandled exceptions, formatting them into standardized JSON error responses to prevent stack trace leaks.
- **CQRS Validation**: MediatR Pipeline Behaviors integrate with `FluentValidation` to validate commands (like `CreateOrderCommand`) *before* they reach the domain logic, failing fast.
- **Structured Logging**: `Serilog` is configured to replace the default logger, writing structured logs that can be ingested by observability tools like ELK or Seq.

### Frontend Enterprise Patterns
- **Angular (Web)**: Uses **NgRx** (Redux pattern) for state management. This separates UI components from side effects (API calls), providing a predictable, unidirectional data flow via Actions, Reducers, and Effects.
- **Android (Mobile)**: Uses the **MVVM** (Model-View-ViewModel) pattern with Jetpack Compose. `ViewModel` manages the state (using Kotlin `StateFlow`), surviving configuration changes and keeping the UI layer purely declarative.

### Distributed Caching
- **Redis**: The `InventoryService` utilizes Redis (`IDistributedCache`) to cache frequently accessed data (e.g., inventory stock levels), reducing load on the primary MongoDB database.
