# FitIT Microservices Platform

## Overview
A comprehensive microservices ecosystem for fitness and wellness management, featuring advanced RBAC, subscription handling, and real-time notifications.

## Architecture
- **Gateway**: YARP-based Reverse Proxy with Rate Limiting (Redis).
- **Auth**: JWT Authentication, Social Login (Google/Microsoft), and RBAC.
- **Apps**: Application & Subscription Management (Weekly/Monthly/Yearly/Unlimited).
- **Payments**: Stripe/PayPal Webhook integration.
- **Notifications**: Real-time updates (SignalR) and Email/SMS dispatch.
- **Users**: User profile, preferences, and linked accounts.
- **Search**: Full-text search across the platform.
- **Audit**: Advanced logging and compliance.

## Infrastructure
- **Message Bus**: RabbitMQ (MassTransit)
- **Database**: SQL Server (EF Core)
- **Caching**: Redis (StackExchange.Redis)
- **Logging**: Serilog -> Seq
- **Frontend**: React (Global Admin, FitIT Admin, Wissler Admin)

## Getting Started
1. **Prerequisites**: Docker Desktop, .NET 8 SDK, Node.js.
2. **Run Infrastructure**: `docker-compose up -d sqlserver rabbitmq redis seq`
3. **Run Services**: `docker-compose up -d`
4. **Access**:
   - Global Admin: http://localhost:3000
   - Seq Logs: http://localhost:5341
   - Health Checks: http://localhost:5000/health-ui

## Recent Updates
- **Social Login**: Integrated Google & Microsoft OAuth.
- **Payments**: Added Stripe Webhook handling for automated subscriptions.
- **Monitoring**: Added HealthChecks UI and Seq Centralized Logging.
