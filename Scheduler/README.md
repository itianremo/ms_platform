# Scheduler Service (Job Orchestration)

Manages background jobs, recurring tasks, and distributed locks for the platform.

## ✨ Features
- **Recurring Jobs**: Cron-based scheduling for system maintenance.
- **Delayed Jobs**: One-time background tasks with delay.
- **Distributed Locking**: Ensures atomic execution across replicas.
- **Dashboard**: GUI for monitoring job status.

## 🏗 Technology Stack
- **Framework**: .NET 8 (ASP.NET Core)
- **Engine**: Hangfire (or similar scheduler if replaced) - Currently SQL backed (`SchedulerDb`).
- **Database**: SQL Server
- **Documentation**: Swagger / OpenAPI

## 🚀 Getting Started

### Prerequisites
- Docker & Docker Compose
- .NET 8 SDK

### Running Locally
```bash
cd Scheduler/Scheduler.API
dotnet run
```

### Running via Docker
```bash
docker-compose up -d scheduler-api
```

## 🔌 API Documentation
- **Swagger UI**: http://localhost:5010/swagger
- **Health Check**: http://localhost:5010/health
