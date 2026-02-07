# Audit Service

## Overview
Responsible for comprehensive system-wide logging and audit trails. Tracks critical user actions and data changes for compliance and security.

## Tech Stack
- **Framework**: .NET 8 (ASP.NET Core Web API)
- **Database**: SQL Server (EF Core)
- **Messaging**: RabbitMQ (MassTransit) - Listens for `AuditLogCreated` events.

## Key Features
- **Centralized Logging**: Aggregates logs from all microservices via message bus.
- **Change Tracking**: Records entity changes (OldValue vs NewValue).
- **Search**: Provides API for querying logs by User, App, or Timeframe.
- **Compliance**: Immutable ledger of critical actions.

## API Documentation
Swagger UI: http://localhost:5004/swagger (via Gateway: http://localhost:5000/audit/swagger)
