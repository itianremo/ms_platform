# Gateway Service

## Overview
The entry point for all external traffic to the FitIT Platform. Implements the Reverse Proxy pattern using YARP (Yet Another Reverse Proxy) to route requests to appropriate microservices.

## Tech Stack
- **Framework**: .NET 8 (YARP)
- **Rate Limiting**: Redis-based distributed rate limiting.
- **Cors**: Centralized Cross-Origin Resource Sharing policy.

## Key Features
- **Routing**: dynamic routing to backend services based on path (`/auth`, `/apps`, `/users`, etc.).
- **Load Balancing**: Distributes traffic across service instances (if scaled).
- **Authentication**: Validates JWT tokens at the edge (optional, mostly pass-through).
- **Rate Limiting**: Protects downstream services from abuse.

## Configuration
Routes and Clusters are defined in `appsettings.json` or loaded dynamically.
