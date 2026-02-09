# Scheduler Service

## Overview
The **Scheduler Service** manages background jobs and recurring tasks for the platform.

## 🚀 Key Features
-   **Recurring Jobs**: Daily/Weekly tasks (e.g., Subscription Renewals, Data Cleanup).
-   **Delayed Jobs**: One-off tasks scheduled for the future (e.g., "Remind me in 1 hour").
-   **Reliability**: Retries failed jobs automatically.
-   **Dashboard**: UI for monitoring job status and history.

## Key Jobs
-   `SubscriptionRenewalJob`: Checks for expiring subscriptions and triggers renewal logic.
-   `DataRetentionJob`: Cleans up old logs or temporary files.

## 🛠️ Tech Stack
-   **.NET 8** (Web API)
-   **Hangfire** (Job Processing)
-   **SQL Server** (Job Storage)
-   **MassTransit** (RabbitMQ) - Used to publish events from jobs.
