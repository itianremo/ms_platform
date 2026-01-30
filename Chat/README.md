# Chat Service (Real-time Messaging)

Provides real-time chat functionality, message history, and user presence for the platform.

## ✨ Features
- **Real-time Messaging**: SignalR-based WebSocket communication.
- **Message Persistence**: stores chat history in MongoDB for scalability.
- **Presence**: Tracks User Online/Offline status.
- **Presence**: Tracks User Online/Offline status.
- **Channels**: Supports 1:1 and Group Chat scenarios (Match Groups).
- **Match Integration**: Dedicated channels for matched users.

## 🏗 Technology Stack
- **Framework**: .NET 8 (ASP.NET Core)
- **Database**: MongoDB (NoSQL)
- **Real-time**: SignalR
- **Documentation**: Swagger / OpenAPI

## 🚀 Getting Started

### Prerequisites
- Docker & Docker Compose
- .NET 8 SDK

### Running Locally
```bash
cd Chat/Chat.API
dotnet run
```

### Running via Docker
```bash
docker-compose up -d chat-api
```

## 🔌 API Documentation
- **Swagger UI**: http://localhost:5006/swagger
- **Health Check**: http://localhost:5006/health
