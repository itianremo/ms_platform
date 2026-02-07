# Apps Service

## Overview
Handles the lifecycle of Client Applications and Subscription Packages within the FitIT Platform.

## Tech Stack
- **Framework**: .NET 8 (ASP.NET Core Web API)
- **Database**: SQL Server (EF Core)
- **Messaging**: RabbitMQ (MassTransit)

## Key Features
- **App Management**: Create, update, and configure applications (Tenants).
- **Subscriptions**: Define and manage subscription packages (Free, Pro, Enterprise).
- **User Assignment**: Link users to applications with specific roles.
- **Billing Integration**: Webhooks for payment providers (Stripe/PayPal) to provision subscriptions.

## API Documentation
Swagger UI: http://localhost:5003/swagger (via Gateway: http://localhost:5000/apps/swagger)
