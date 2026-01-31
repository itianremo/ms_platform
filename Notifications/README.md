# Notifications Service (Communication)

Centralized service for dispatching emails, SMS, and in-app notifications.

## ✨ Features
- **Multi-Channel Dispatch**: Support for Email (SMTP), SMS, and In-App alerts.
- **Real-Time Alerts**: SignalR Hub for instant frontend notifications (Toast/In-App).
- **Templates**: Dynamic template rendering for notifications.
- **Event-Driven**: Listens to system events (e.g., `UserRegistered`) to trigger notifications.
- **User Preferences**: Respects user opt-in/opt-out settings.

## 🏗 Technology Stack
- **Framework**: .NET 8 (ASP.NET Core)
- **Messaging**: MassTransit (RabbitMQ)
- **Database**: SQL Server (for logs/templates) - `NotificationsDb`
- **Documentation**: Swagger / OpenAPI

## 🚀 Getting Started

### Prerequisites
- Docker & Docker Compose
- .NET 8 SDK

### Running Locally
```bash
cd Notifications/Notifications.API
dotnet run
```

### Running via Docker
```bash
docker-compose up -d notifications-api
```

## 🔌 API Documentation
- **Swagger UI**: http://localhost:5004/swagger
- **Health Check**: http://localhost:5004/health
