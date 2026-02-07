# Scheduler Service

## Overview
Background job orchestrator for recurring tasks and delayed operations.

## Tech Stack
- **Framework**: .NET 8 (Worker Service / ASP.NET Core)
- **Engine**: Quartz.NET / Hangfire
- **Messaging**: RabbitMQ (MassTransit)

## Key Features
- **Cron Jobs**: Execute recurring tasks (e.g., Daily Subscription Checks, Email Summaries).
- **Delayed Jobs**: Schedule one-time tasks for the future.
- **Reliability**: Retries and error handling for background jobs.
- **Integration**: Triggers commands via RabbitMQ to other services.

## Configuration
Define jobs in `appsettings.json` or via code registration.
