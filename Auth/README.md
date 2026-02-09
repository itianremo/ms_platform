# Auth Service

## Overview
The **Auth Service** is the backbone of identity and access management for the platform. It handles user registration, authentication (JWT), and role-based access control (RBAC).

## 🚀 Key Features
-   **Identity Management**: Registrations, Logins (Email/Password), and Social Auth (Google/Microsoft).
-   **Token Service**: Issuance of **Access Tokens** (JWT) and **Refresh Tokens**.
-   **RBAC**: Dynamic Role and Permission management (e.g., `SuperAdmin`, `AppAdmin`, `User`).
-   **Security**: Account locking, Password policies, and Session revocation.
-   **OTP**: Generates One-Time Passwords for verification (sent via Notifications Service).

## 📡 Event Architecture
### Publishes
-   `UserRegisteredEvent`: Fired when a new user successfully registers.
-   `UserLoginEvent`: (Optional) For audit/tracking.
-   `UserStatusChanged`: When a user is banned or activated.

### Consumes
-   `SubscriptionGrantedEvent`: Updates user claims/roles based on new subscription.

## 🛠️ Tech Stack
-   **.NET 8** (Web API)
-   **SQL Server** (Identity Data)
-   **Redis** (Token Blacklist / Session Store)
-   **MassTransit** (RabbitMQ)
