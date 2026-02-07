# Users Service

## Overview
Manages user profiles, preferences, and personal information. Acts as the source of truth for user details beyond authentication credentials.

## Tech Stack
- **Framework**: .NET 8 (ASP.NET Core Web API)
- **Database**: SQL Server (EF Core)
- **Messaging**: RabbitMQ (MassTransit)

## Key Features
- **Profiles**: Manages display names, avatars, and bio.
- **Preferences**: Stores user settings (Theme, Sidebar state) per application via `customDataJson`.
- **Integration**: Consumes `UserRegistered` events to create profiles automatically.
- **Search**: Syncs user data to the Search service for discovery.

## API Documentation
Swagger UI: http://localhost:5002/swagger (via Gateway: http://localhost:5000/users/swagger)
