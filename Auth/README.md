# Auth Service (Identity & Access)

The centralized Identity Provider (IdP) handling Authentication, Authorization (RBAC), and Session Management.

## ✨ Features
- **OIDC/SSO**: JWT-based authentication with Secure Cookie Sharing across subdomains.
- **RBAC**: Role-Based Access Control with granular Permissions.
- **OTP Verification**: Email and SMS verification logic.
- **User Management**: Registration, Login, and Password Reset flows.

## 🏗 Technology Stack
- **Framework**: .NET 8 (ASP.NET Core)
- **Database**: SQL Server (`AuthDb`)
- **Messaging**: MassTransit (RabbitMQ)
- **Documentation**: Swagger / OpenAPI

## 🚀 Getting Started

### Prerequisites
- Docker & Docker Compose
- .NET 8 SDK

### Running Locally
```bash
cd Auth/Auth.API
dotnet run
```

### Running via Docker
```bash
docker-compose up -d ms-auth-api
```

## 🔌 API Documentation
- **Swagger UI**: http://localhost:5001/swagger
- **Health Check**: http://localhost:5001/health
