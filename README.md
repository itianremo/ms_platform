# Unified Microservices Platform (UMP)

## Overview
A scalable **App Factory** designed to incubate and manage multiple distinct applications ("Tenants") from a single control plane. UMP currently powers **FitIt** (Fitness) and **Wissler** (Dating), managing **25+ Dockerized containers** to provide shared infrastructure and distinct business logic.

## Architecture
The platform is built on a **Shared Kernel** of microservices that provide "Day 1" readiness for new apps:

-   **Gateway**: YARP-based Reverse Proxy with Rate Limiting (Redis).
-   **Identity (Auth)**: Centralized JWT Authentication with Configurable Social Logins (Google, Microsoft, Facebook, Apple).
-   **Payments**: Unified engine supporting multi-tenant Gateways (Stripe, PayPal).
-   **Communication**: Single Global Pipeline for SMS/SMTP (Twilio/SendGrid) + SignalR for Real-time updates.
-   **Data**: SQL Server (Relational), MongoDB (Chat), PostGIS (Geo), MinIO (Media), Redis (Cache).

## Multi-Tenant Configuration Strategy
| Feature | Strategy | Description |
| :--- | :--- | :--- |
| **SMS/SMTP** | **Single Global** | Configured once in Global Admin. High-reputation sender shared by all apps. |
| **Social Auth** | **Multi-Tenant** | Configurable per App. App A can use Google, App B can use Microsoft. |
| **Payments** | **Multi-Tenant** | Configurable per App. Revenue streams are completely isolated. |


## Application Portfolio (Examples)
1.  **FitIT**: Fitness management with Gym discovery (Geo API) and Workout streaming (Media API).
2.  **Wissler**: Dating application with "People Nearby" (Geo API) and Real-time Messaging (Chat API).

## Mobile Applications
The platform includes native mobile experiences for both Tenants, built with **Flutter**:
-   **FitIT App**: Manages workout plans and tracks progress.
-   **Wissler App**: Discovery and social interaction.
Both apps share a common `API Client` and `Authentication Core` while maintaining distinct visual identities through **Dynamic Theming**.

## Dynamic Theme Engine
The platform enforces consistent branding across Web and Mobile:
-   **Public/Logout State**: Applications automatically load the **App-Specific Default Theme** configurations (e.g., FitIT Blue, Wissler Orange) from the backend.
-   **Authenticated State**: Applications adapt to the **User's Personal Preferences** (Dark/Light mode) stored in their profile.

## Getting Started

1.  **Prerequisites**: Docker Desktop, .NET 8 SDK, Node.js.
2.  **Run Infrastructure**: `docker-compose up -d sqlserver rabbitmq redis seq`
3.  **Run Services/Apps**: `docker-compose up -d`
4.  **Access Portals**:
    -   **Global Admin**: http://localhost:3000 (Manage Tenants & System Configs)
    -   **FitIT Admin**: http://localhost:3001
    -   **Wissler Admin**: http://localhost:3002
    -   **Seq Logs**: http://localhost:5341
