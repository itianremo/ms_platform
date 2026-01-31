# Users Service (Profile Management)

Manages User Profiles, Preferences, and User-specific data distinct from Identity credentials.

## ✨ Features
- **Profile Management**: Stores Display Name, Avatar, and Personal Details.
- **Preferences**: Manages User Settings (Theme, Notifications) synchronized with App defaults.
- **Linked Accounts**: Manage Social Logins (Google/Microsoft).
- **Audit History**: View personal audit logs (login history, profile changes).
- **Search Synchronization**: Publishes profile updates to the Search Index.

## 🏗 Technology Stack
- **Framework**: .NET 8 (ASP.NET Core)
- **Database**: PostgreSQL (`UsersDb`)
- **Messaging**: MassTransit (RabbitMQ)
- **Documentation**: Swagger / OpenAPI

## 🚀 Getting Started

### Prerequisites
- Docker & Docker Compose
- .NET 8 SDK

### Running Locally
```bash
cd Users/Users.API
dotnet run
```

### Running via Docker
```bash
docker-compose up -d users-api
```

## 🔌 API Documentation
- **Swagger UI**: http://localhost:5003/swagger
- **Health Check**: http://localhost:5003/health
