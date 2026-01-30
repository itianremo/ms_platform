# Apps Service (Tenant Management)

Manages Tenant configuration, App-specific rules, and Feature Flags as the central control plane for multitenancy.

## ✨ Features
- **Tenant Configuration**: Manage App details, Descriptions, and Base URLs.
- **Feature Flags**: Toggle features per App.
- **Theming**: Store and serve UI theme preferences (JSON).
- **Security Policy**: Define Verification Types (Email/Phone) and Admin Approval rules.

## 🏗 Technology Stack
- **Framework**: .NET 8 (ASP.NET Core)
- **Database**: PostgreSQL (`AppsDb`)
- **Messaging**: MassTransit (RabbitMQ)
- **Documentation**: Swagger / OpenAPI

## 🚀 Getting Started

### Prerequisites
- Docker & Docker Compose
- .NET 8 SDK

### Running Locally
```bash
cd Apps/Apps.API
dotnet run
```

### Running via Docker
```bash
docker-compose up -d apps-api
```

## 🔌 API Documentation
- **Swagger UI**: http://localhost:5002/swagger
- **Health Check**: http://localhost:5002/health
