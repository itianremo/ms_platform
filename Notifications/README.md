# Notifications Service

## Overview
The **Notifications Service** acts as the central communication hub, dispatching messages to users via multiple channels (Email, SMS, Push).

## 🚀 Key Features
-   **Multi-Channel Support**:
    -   **Email**: SMTP / SendGrid.
    -   **SMS**: Twilio / AWS SNS.
    -   **Push**: Firebase Cloud Messaging (FCM).
-   **Templating**: HTML email templates for consistent branding.
-   **Event-Driven**: Listens for events (e.g., `UserRegistered`, `SubscriptionExpiring`) to trigger notifications automatically.

## 📡 Event Architecture
### Consumes
-   `UserRegisteredEvent`: Sends Welcome Email.
-   `SendOtpEvent`: Dispatches OTP via Email/SMS.
-   `SubscriptionStatusChanged`: Notifies user of status.

## 🛠️ Tech Stack
-   **.NET 8** (Web API)
-   **MassTransit** (RabbitMQ)
-   **SMTP / FCM**
