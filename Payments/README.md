# Payments Service

## Overview
Handles payment processing and webhook events from payment providers (Stripe, PayPal).

## Features
- **Stripe Checkout**: Real Payment Gateway integration using `Stripe.Checkout.Session` for subscriptions.
- **Stripe Webhooks**: Idempotent handling of `invoice.payment_succeeded` and `checkout.session.completed` events.
- **Event Publishing**: Publishes `PaymentSucceededEvent` to Message Bus upon successful payment.
- **Infrastructure**: Uses `MassTransit` for event propagation.

## Configuration
- **Stripe**: Configure `StripeSettings:WebhookSecret` in `appsettings.json`.
- **Message Bus**: RabbitMQ connection required.
