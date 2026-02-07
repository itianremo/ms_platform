# Notifications Service

## Overview
Central hub for dispatching notifications to users via multiple channels (Real-time, Email, SMS).

## Tech Stack
- **Framework**: .NET 8 (ASP.NET Core Web API)
- **Real-time**: SignalR (WebSockets)
- **Email**: SMTP / SendGrid
- **Messaging**: RabbitMQ (MassTransit) - Listens for `SendNotification` commands.

## Key Features
- **Multi-channel**: Supports Web (SignalR), Email, and SMS (Twilio/Generic).
- **Templates**: HTML email templates for system events (Welcome, Reset Password).
- **User Preferences**: Check user settings before sending (Do Not Disturb).
- **History**: Stores notification history and read status.

## API Documentation
Swagger UI: http://localhost:5006/swagger (via Gateway: http://localhost:5000/notifications/swagger)
