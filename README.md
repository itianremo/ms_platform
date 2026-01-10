# MS Platform (Microservices Architecture)

A scalable, modular microservices platform built with **.NET 8 (Backend)** and **React + Vite (Frontend)**. This project demonstrates a Clean Architecture approach with a focus on multitenancy, RBAC, and high scalability.

## 🚀 Features

- **Microservices Architecture**: 14+ isolated services communicating via RabbitMQ (MassTransit) and HTTP (YARP Gateway).
- **Identity & Access**: OIDC-compliant Auth service with JWT, Cookie Sharing, and Sealed Superadmin support.
- **Real-time Communication**: SignalR-based Chat and Notifications.
- **Content Management**: MinIO-based Media storage with automatic Image Resizing and Moderation.
- **Search**: PostgreSQL Full-Text Search synchronized via Events.
- **Dynamic Payments**: Strategy-based Payment Gateway (Stripe/Mock) with configurable providers per Tenant.
- **Frontend**: 
  - **FitITAdmin**: Tenant Administration Dashboard.
  - **GlobalAdmin**: Superadmin Dashboard.
  - **App**: User-facing Mobile Web App.

## 📂 Architecture Overview

### Core Services
| Service | Tech Stack | Responsibility |
| :--- | :--- | :--- |
| **Auth** | .NET 8, SQL Server | Identity, SSO, Roles (RBAC), Permissions. |
| **Users** | .NET 8, PostgreSQL | User Profiles, Settings. |
| **Apps** | .NET 8, PostgreSQL | Tenant Configuration, App Rules. |
| **Gateway** | YARP | API Gateway, Reverse Proxy. |
| **Shared** | .NET Class Library | Kernel, EventBus, Caching, Common Types. |

### Feature Services
| Service | Tech Stack | Responsibility |
| :--- | :--- | :--- |
| **Chat** | .NET 8, MongoDB | Real-time Messaging, History. |
| **Media** | .NET 8, MinIO | File Uploads, Resizing, Moderation. |
| **Search** | .NET 8, PostgreSQL | User Search (Syncs with Users/Apps). |
| **Payments** | .NET 8, PostgreSQL | Subscriptions, Plans, Gateways. |
| **Notifications** | .NET 8, RabbitMQ | Email/SMS Dispatch, In-App Alerts. |
| **Audit** | .NET 8, MongoDB | centralized Audit Logging. |
| **Geo** | .NET 8, PostGIS | Location Services, Proximity Search. |
| **Recommendation**| Python / ML.NET | AI-driven User Matching. |
| **Scheduler** | Hangfire | Recurring Jobs, Maintenance. |

## 🛠️ Infrastructure

- **Docker Compose**: Orchestrates all services and infrastructure dependencies.
- **RabbitMQ**: Message Broker for Asynchronous Events.
- **Redis**: Distributed Caching.
- **PostgreSQL / SQL Server / MongoDB**: Polyglot Persistence.

## 🏁 Getting Started

Detailed instructions for Local, AWS, and Azure deployment can be found in the **[Deployment Guide](file:///C:/Users/ramim/.gemini/antigravity/brain/cea935a0-32cf-4cce-a420-d14139c1a197/deployment_guide.md)**.

### Running Locally (Quick Start)
1. **Clone the Repository**
2. **Start Infrastructure**
   ```bash
   docker-compose up -d --build
   ```
3. **Access Services**
   - **FitIT Admin**: http://localhost:3001
   - **Global Admin**: http://localhost:3000
   - **Gateway**: http://localhost:7032

## 🔐 Credentials (Default Seeding)
- **Superadmin**: `admin@fitit.com` / *(Check Seed Config)*

## 📄 License
[Proprietary/Internal]
