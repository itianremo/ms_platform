# Gateway Service (API Gateway)

The entry point for all client requests, routing traffic to appropriate microservices and handling cross-cutting concerns.

## ✨ Features
- **Reverse Proxy**: Powered by YARP (Yet Another Reverse Proxy).
- **Route Management**: Centralized routing configuration.
- **Authentication Hand-off**: Validates JWTs at the edge (optional configuration).
- **CORS Handling**: Centralized CORS policies.

## 🏗 Technology Stack
- **Framework**: .NET 8 (ASP.NET Core)
- **Library**: YARP
- **Documentation**: Swagger / OpenAPI

## 🚀 Getting Started

### Prerequisites
- Docker & Docker Compose
- .NET 8 SDK

### Running Locally
```bash
cd Gateway/Gateway.API
dotnet run
```

### Running via Docker
```bash
docker-compose up -d gateway-api
```

## 🔌 API Documentation
- **Swagger UI**: http://localhost:7032/swagger
- **Health Check**: http://localhost:7032/health
