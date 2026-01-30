# Audit Service (Logging & Compliance)

Centralized service for capturing, storing, and querying system-wide audit logs.

## ✨ Features
- **Centralized Logging**: Captures user actions and system changes.
- **Compliance**: Immutable audit trails.
- **Searchable History**: Query logs by user, entity, or date.
- **Async Processing**: High-throughput log ingestion via queues.

## 🏗 Technology Stack
- **Framework**: .NET 8 (ASP.NET Core)
- **Database**: SQL Server
- **Messaging**: MassTransit (RabbitMQ)
- **Documentation**: Swagger / OpenAPI

## 🚀 Getting Started

### Prerequisites
- Docker & Docker Compose
- .NET 8 SDK

### Running Locally
```bash
cd Audit/Audit.API
dotnet run
```

### Running via Docker
```bash
docker-compose up -d audit-api
```

## 🔌 API Documentation
- **Swagger UI**: http://localhost:5008/swagger
- **Health Check**: http://localhost:5008/health
