# 🚀 Enterprise Microservices Platform

Welcome to the Enterprise Microservices Platform! This project is a comprehensive, production-ready infrastructure demonstrating advanced software engineering patterns, architectures, and technologies. It was built to serve as a deep-learning resource and a blueprint for scalable, resilient systems.

## 🎯 Project Goals
The primary goal of this repository is to showcase a **"Super Professional Infrastructure"**, moving beyond simple CRUD apps to implement a robust, decoupled, and scalable ecosystem. It incorporates:
- Microservices Architecture
- Clean Architecture principles
- CQRS (Command Query Responsibility Segregation)
- Event-Driven Architecture (Pub/Sub)
- API Gateways
- Multiple client types (Web & Mobile)
- Containerization and Orchestration

## 📁 Repository Structure
```
├── docs/                     # Architectural documentation (ARCHITECTURE.md)
├── src/
│   ├── ApiGateway/           # YARP-based Reverse Proxy entry point
│   ├── OrderService/         # C# Microservice (Clean Arch, CQRS, PostgreSQL, EF Core)
│   ├── InventoryService/     # C# Microservice (Clean Arch, MongoDB)
│   ├── WebFrontend/          # Angular App (Standalone Components, SCSS)
│   ├── MobileApp/            # Android Native App (Kotlin)
│   └── Infrastructure/       # Docker Compose & Kubernetes manifests
```

## 🛠️ Technologies Used
- **Backend**: C# 8.0/10.0, ASP.NET Core Web API
- **Architecture Patterns**: Clean Architecture, Microservices, CQRS (MediatR)
- **Messaging/Integration**: RabbitMQ, MassTransit
- **Databases**: PostgreSQL (Relational), MongoDB (NoSQL)
- **Gateway**: YARP (Yet Another Reverse Proxy)
- **Frontend**: Angular
- **Mobile**: Android (Kotlin)
- **DevOps**: Docker, Docker Compose, Kubernetes (K8s)

## 📖 Deep Dive
To truly understand the design decisions, patterns, and how the services interact, please read the detailed architectural documentation:
👉 [Read ARCHITECTURE.md](./docs/ARCHITECTURE.md)

## 🚀 Getting Started

### Prerequisites
- Docker & Docker Compose
- .NET 8 SDK (or latest)
- Node.js & npm (for Angular)

### Running Locally with Docker
The easiest way to spin up the entire infrastructure (Databases, RabbitMQ, and Microservices) is using Docker Compose:

```bash
cd src/Infrastructure/docker
docker-compose up -d
```
This will start:
- RabbitMQ on port 15672 (Management UI)
- PostgreSQL & MongoDB
- OrderService, InventoryService
- API Gateway on port 5000

Enjoy studying this architecture!
