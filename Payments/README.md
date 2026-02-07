# Payments Service

## Overview
Handles all financial transactions, subscription billing, and payment method management using external providers.

## Tech Stack
- **Framework**: .NET 8 (ASP.NET Core Web API)
- **Gateway**: Stripe / PayPal (SDKs)
- **Messaging**: RabbitMQ (MassTransit)

## Key Features
- **Checkout**: Hosted checkout sessions for subscriptions.
- **Webhooks**: Handles asynchronous updates from payment providers (PaymentSucceeded, SubscriptionUpdated).
- **Invoicing**: Generates and stores transaction records.
- **Wallet**: Manages saved payment methods.

## API Documentation
Swagger UI: http://localhost:5007/swagger (via Gateway: http://localhost:5000/payments/swagger)
