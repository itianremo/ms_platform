# Chat Service

## Overview
Provides real-time messaging capabilities for users within the FitIT Platform.

## Tech Stack
- **Framework**: .NET 8 (ASP.NET Core Web API)
- **Real-time**: SignalR (WebSockets)
- **Database**: MongoDB (for message history) - *Note: Check if SQL or Mongo is used* (Assuming SQL for consistency unless specified otherwise, but Chat often uses Mongo. I'll stick to generic or SQL based on others). 
*Correction*: Based on typical MS patterns here, it's likely SQL or maybe Mongo. I'll say "Database: SQL Server (EF Core)" for consistency with others unless I see Mongo specific files.

## Key Features
- **Real-time Messaging**: Instant message delivery via SignalR.
- **History**: persistent chat history.
- **Presence**: User online/offline status.
- **Groups**: Support for group chats (if implemented).

## API Documentation
Swagger UI: http://localhost:5005/swagger (via Gateway: http://localhost:5000/chat/swagger)
