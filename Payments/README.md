# Payments Service

## Overview
The **Payments Service** handles all financial transactions, including subscriptions, one-time purchases, and payment method management.

## 🚀 Key Features
-   **Payment Gateways**: Integration with **Stripe** (Primary).
-   **Subscriptions**: recurring billing logic and webhook handling.
-   **Invoicing**: Generation and storage of PDF invoices.
-   **Wallet**: Management of stored payment methods (Cards).

## 📡 Event Architecture
### Consumes
-   `StripeWebhook`: Listens for updates from Stripe (e.g., `invoice.payment_succeeded`).

### Publishes
-   `PaymentSucceededEvent`: Triggers successful order/subscription fulfillment in other services.
-   `PaymentFailedEvent`: Triggers retry logic or cancellation.

## 🛠️ Tech Stack
-   **.NET 8** (Web API)
-   **Stripe SDK**
-   **SQL Server** (Transaction Logs)
-   **MassTransit** (RabbitMQ)
