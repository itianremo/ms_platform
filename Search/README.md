# Search Service (Full-Text Search)

Dedicated service for high-performance search across platform entities (Users, Apps).

## ✨ Features
- **Full-Text Search**: Optimized text queries using PostgreSQL capabilities.
- **Data Synchronization**: Listens to domain events to keep index updated in near real-time.
- **Filtering**: Advanced filtering capabilities.

## 🏗 Technology Stack
- **Framework**: .NET 8 (ASP.NET Core)
- **Database**: PostgreSQL (PostGIS enabled container used)
- **Messaging**: MassTransit (RabbitMQ)
- **Documentation**: Swagger / OpenAPI

## 🚀 Getting Started

### Prerequisites
- Docker & Docker Compose
- .NET 8 SDK

### Running Locally
```bash
cd Search/Search.API
dotnet run
```

### Running via Docker
```bash
docker-compose up -d search-api
```

## 🔌 API Documentation
- **Swagger UI**: http://localhost:5009/swagger
- **Health Check**: http://localhost:5009/health
