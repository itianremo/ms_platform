# Unified Microservices Platform (FitIT & Wissler)

Welcome to the **Unified Microservices Platform**, a robust, scalable backend system powering multiple tenant applications including **FitIT** (Fitness) and **Wissler** (Social/Dating).

## 🚀 Overview

The platform is designed with a **Microservices Architecture** using **.NET 8**, **Docker**, and an **Event-Driven** approach. It provides a centralized API Gateway, shared identity management, and specialized services for various domain functions.

### Key Applications
*   **FitIT**: A comprehensive fitness tracking application helping users achieve their health goals. [Read More](Mobile/apps/fitit/README.md)
*   **Wissler**: A social and dating application featuring biometric security and advanced profile matching. [Read More](Mobile/apps/wissler/README.md)

## 🏗️ Architecture

The system uses a **YARP API Gateway** as the single entry point, routing traffic to downstream services. Communication between services is handled asynchronously via **RabbitMQ (MassTransit)**.

 > [!NOTE]
 > For a detailed architecture diagram and data flow, please refer to the [Architecture Documentation](docs/architecture.md).

### Core Services
| Service | Description |
| :--- | :--- |
| **Gateway** | Central API Gateway handling routing, rate limiting, and auth proxying. |
| **Auth** | Identity provider (JWT), OAuth2, and RBAC management. |
| **Users** | Manages user profiles, settings, and extended personal data. |
| **Apps** | Handles tenant application logic, subscriptions, and memberships. |
| **Notifications** | Centralized notification sender (Email, SMS, Push). |
| **Media** | Handles file uploads, storage (MinIO/S3), and serving. |
| **Chat** | Real-time messaging using SignalR. |
| **Payments** | Integration with Stripe/payment providers. |
| **Audit** | system-wide audit logging for security and compliance. |
| **Search** | ElasticSearch/OpenSearch integration for discovery. |
| **Scheduler** | Background job processing using Hangfire. |
| **Geo** | Location services, country/city data. |
| **Recommendation** | AI/ML powered matching and recommendations. |

## 🛠️ Getting Started

### Prerequisites
*   **Docker Desktop** (Running)
*   **.NET 8 SDK**
*   **Visual Studio 2022** or **VS Code**

### Quick Start
1.  **Clone the repository**:
    ```bash
    git clone <repo-url>
    ```
2.  **Run with Docker**:
    The easiest way to start the entire platform is via Docker Compose.
    ```bash
    docker-compose up -d --build
    ```
    *This will spin up all services, databases, Redis, and RabbitMQ.*

3.  **Access Services**:
    *   **Gateway (API)**: `http://localhost:5000`
    *   **Health Dashboard**: `http://localhost:5000/health-dashboard`
    *   **Swagger (Auth)**: `http://localhost:5001/swagger` (Direct) or `http://localhost:5000/auth/swagger` (Gateway)

## 📦 Deployment

The platform is containerized and ready for deployment on Kubernetes or any Docker-compatible orchestrator. Configuration is managed via `appsettings.json` and Environment Variables.

## 🤝 Contributing

Please ensure you follow the coding standards and run all tests before submitting a Pull Request.
