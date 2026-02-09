# Apps Service

## Overview
The **Apps Service** manages tenant application logic, specifically focusing on **Subscriptions**, **Memberships**, and App-specific configurations. It acts as the bridge between the core platform and specific tenant applications like **FitIT** and **Wissler**.

## 🚀 Key Features
-   **Subscription Management**: Handles user subscriptions (Free, Premium, VIP) and their statuses.
-   **Plan Configuration**: Defines available plans, pricing, and features for each tenant app.
-   **App Membership**: Tracks which users belong to which apps.
-   **Payment Integration**: Listens for `PaymentSucceeded` events to grant or renew subscriptions.

## 📡 Event Architecture
### Consumes
-   `PaymentSucceededEvent`: Triggers subscription activation or renewal.
-   `UserRegisteredEvent`: Creates a default free membership for the user.

### Publishes
-   `SubscriptionGrantedEvent`: Notifies other services (like Auth/Users) of a new subscription.
-   `SubscriptionStatusChangedEvent`: Updates when a subscription expires or is cancelled.

## 🛠️ Tech Stack
-   **.NET 8** (Web API)
-   **SQL Server** (EF Core)
-   **MassTransit** (RabbitMQ)
