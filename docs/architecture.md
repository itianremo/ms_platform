# System Architecture

## Overview
The **Unified Microservices Platform** is a distributed system built on **.NET 8** and **Docker**, featuring a centralized API Gateway and an Event-Driven Architecture.

### Core Components
1.  **API Gateway (YARP)**: Single entry point for all client requests. Handles routing, authentication, and load balancing.
2.  **Microservices**: Independent services responsible for specific domains (Auth, Users, Apps, etc.).
3.  **Message Broker (RabbitMQ)**: Facilitates asynchronous communication between services via MassTransit.
4.  **Databases**: Dedicated SQL Server databases for services (Auth, Users, Apps, etc.) and Redis for caching.

## Architecture Diagram

```mermaid
graph TD
    %% Clients
    Client([Client Apps / Web]) --> Gateway[("API Gateway (YARP)")]

    %% Gateway Routing
    Gateway -->|/auth| Auth[Auth Service]
    Gateway -->|/users| Users[Users Service]
    Gateway -->|/apps| Apps[Apps Service]
    Gateway -->|/notifications| Notifications[Notifications Service]
    Gateway -->|/media| Media[Media Service]
    Gateway -->|/chat| Chat[Chat Service]
    Gateway -->|/payments| Payments[Payments Service]
    Gateway -->|/audit| Audit[Audit Service]
    Gateway -->|/search| Search[Search Service]
    Gateway -->|/scheduler| Scheduler[Scheduler Service]
    Gateway -->|/geo| Geo[Geo Service]
    Gateway -->|/recommendation| Rec[Recommendation Service]

    %% Event Bus (RabbitMQ)
    subgraph EventBus [Event Bus (RabbitMQ)]
        Auth
        Users
        Apps
        Notifications
        Media
        Payments
        Audit
        Search
        Rec
    end

    %% Event Flows
    Auth -.->|UserRegisteredEvent| Users
    Auth -.->|UserRegisteredEvent| Notifications
    Users -.->|UserProfileCreated| Search
    Apps -.->|SubscriptionGranted| Auth
    Payments -.->|PaymentSucceeded| Apps
```

## Data Flow
1.  **Request Flow**:
    -   Clients authenticate via `Auth Service` to obtain a JWT.
    -   Requests are sent to the `Gateway` with the JWT.
    -   `Gateway` routes the request to the appropriate downstream service based on the path (e.g., `/users/*` -> `Users Service`).

2.  **Event Flow (Async)**:
    -   Services publish domain events to the **Event Bus** (RabbitMQ).
    -   Subscribed services consume these events to update their local state or trigger side effects.
    -   *Example*: When a new user registers in `Auth Logic`, a `UserRegisteredEvent` is published. The `Users Service` consumes this to create a profile, and `Notifications Service` sends a welcome email.

## Key Technologies
-   **Gateway**: YARP (Yet Another Reverse Proxy)
-   **Communication**: HTTP/REST (Synchronous), MassTransit/RabbitMQ (Asynchronous)
-   **Observability**: Serilog, HealthChecks UI
