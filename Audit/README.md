# Audit Service

## Overview
The **Audit Service** provides centralized logging for security, compliance, and system activity. It tracks "Who did What, When, and Where" across the entire platform.

## 🚀 Key Features
-   **Centralized Logging**: Aggregates audit logs from all microservices.
-   **Interceptors**: Uses EF Core Interceptors in other services to automatically capture data changes (Create/Update/Delete).
-   **Security**: Immutable log storage for compliance.
-   **Searchable**: Logs are indexed for quick retrieval by Admins.

## 📡 Event Architecture
### Consumes
-   `AuditLogCreatedEvent`: The primary event consumed from all services containing the change details (JSON diffs).

## 🛠️ Tech Stack
-   **.NET 8** (Web API)
-   **SQL Server** (Log Storage)
-   **MassTransit** (RabbitMQ)
