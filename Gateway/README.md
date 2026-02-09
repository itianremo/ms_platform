# Gateway Service

## Overview
The **Gateway Service** is the central entry point for the entire **Unified Microservices Platform**. It routes all incoming traffic to the appropriate downstream microservices using **YARP (Yet Another Reverse Proxy)**.

## 🚀 Key Features
-   **Centralized Routing**: Maps external requests (e.g., `/auth/*`) to internal services (e.g., `http://auth-service:8080`).
-   **Authentication Proxy**: Validates JWTs before forwarding requests to protected endpoints.
-   **Health Checks UI**: Aggregates health status from all downstream services into a single dashboard.
-   **Rate Limiting**: Protects backend services from abuse.
-   **CORS Management**: Centralized Cross-Origin Resource Sharing policies.

## 🛠️ Configuration
The routing logic is defined in `appsettings.json` (or `appsettings.Docker.json` for containerized environments).

### Main Routes
| Route | Cluster | Destination |
| :--- | :--- | :--- |
| `/auth/*` | `auth-cluster` | Auth Service |
| `/users/*` | `users-cluster` | Users Service |
| `/apps/*` | `apps-cluster` | Apps Service |
| `/notifications/*` | `notifications-cluster` | Notifications Service |
| `/media/*` | `media-cluster` | Media Service |
| `/chat/*` | `chat-cluster` | Chat Service |
| `/payments/*` | `payments-cluster` | Payments Service |
| `/audit/*` | `audit-cluster` | Audit Service |
| `/search/*` | `search-cluster` | Search Service |
| `/geo/*` | `geo-cluster` | Geo Service |
| `/recommendation/*` | `rec-cluster` | Recommendation Service |

## 📦 Tech Stack
-   **.NET 8**
-   **YARP**
-   **AspNetCore.HealthChecks.UI**
