# Audit Service (Compliance & Logging)

Centralized service for tracking all critical system actions.

## Features
- **Audit Logging**: Consumes messages from other services to record "who did what and when".
- **Compliance**: Immutable record of changes (Soft Deletes, Role Changes).

## Tech Stack
- **.NET 8**
- **MongoDB** (Log Storage)
- **MassTransit**
