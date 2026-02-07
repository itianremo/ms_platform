# Auth Service

## Overview
Central Authentication and Authorization service for the FitIT Platform. Handles user identity, token issuance, and permission management.

## Tech Stack
- **Framework**: .NET 8 (ASP.NET Core Web API)
- **Database**: SQL Server (EF Core)
- **Caching**: Redis (StackExchange.Redis)
- **Messaging**: RabbitMQ (MassTransit)

## Key Features
- **JWT Authentication**: Issues Access and Refresh tokens.
- **Social Login**: Google and Microsoft OAuth2 integration.
- **RBAC**: Dynamic Role-Based Access Control and Permission management.
- **Security**: Rate Limiting, Session Management (Revocation), and Audit Logging.
- **User Stats**: Provides analytics on Admin vs Visitor counts per application.

## API Documentation
Swagger UI: http://localhost:5001/swagger (via Gateway: http://localhost:5000/auth/swagger)
