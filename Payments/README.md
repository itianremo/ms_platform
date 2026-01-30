# Payments Service (Financial Integration)

Manages subscriptions, payment strategies, and transaction history.

## ✨ Features
- **Strategy Pattern**: Pluggable gateways (e.g., Stripe, Mock).
- **Multi-Tenant Config**: Different payment providers per tenant app.
- **Subscriptions**: Recurring billing and plan management.
- **Webhooks**: Handling gateway callbacks securely.

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
cd Payments/Payments.API
dotnet run
```

### Running via Docker
```bash
docker-compose up -d payments-api
```

## 🔌 API Documentation
- **Swagger UI**: http://localhost:5007/swagger
- **Health Check**: http://localhost:5007/health
