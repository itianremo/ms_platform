# Notifications Service

Centralized dispatch for Email, SMS, and In-App notifications.

## Features
- **SignalR Hub**: Delivers real-time alerts to connected clients.
- **Email/SMS Abstraction**: Pattern for integrating Twilio/SendGrid (Stubbed implementation).
- **Event Consumers**: Reacts to system events (e.g., `UserRegistered`) to send welcome messages.

## Tech Stack
- **.NET 8**
- **RabbitMQ**
- **SignalR**
