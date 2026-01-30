# Geo Service (Location Services)

Provides geospatial functionality, location tracking, and proximity search.

## ✨ Features
- **Geocoding**: Address to Coordinate conversion (and vice versa).
- **Location Updates**: Real-time user location tracking (`POST /location`).
- **Proximity Search**: Find entities "near me" using PostGIS spatial queries (`GET /nearby`).
- **Regions**: Manage geographical boundaries (polygons).

## 🏗 Technology Stack
- **Framework**: .NET 8 (ASP.NET Core)
- **Database**: PostgreSQL + PostGIS Extension
- **Messaging**: MassTransit (RabbitMQ)
- **Documentation**: Swagger / OpenAPI

## 🚀 Getting Started

### Prerequisites
- Docker & Docker Compose
- .NET 8 SDK

### Running Locally
```bash
cd Geo/Geo.API
dotnet run
```

### Running via Docker
```bash
docker-compose up -d geo-api
```

## 🔌 API Documentation
- **Swagger UI**: http://localhost:5011/swagger
- **Health Check**: http://localhost:5011/health
