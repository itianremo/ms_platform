# Auth Service

## Overview
Central Authentication and Authorization service using JWT.

## Features
- **JWT Authentication**: Issues Access and Refresh tokens.
- **Social Login**: Google and Microsoft OAuth2 support.
- **RBAC**: Role-based access control with dynamic permissions.
- **Events**: Listens for `SubscriptionGrantedEvent` to update user claims.
- **Security**: Rate Limiting (Redis), Session Blacklisting (Revocation), and Advanced Audit Logging (IP/UserAgent).
- **Session Management**: Revoke active sessions via API.
- **Caching**: Redis caching for permissions and user data.

## Configuration
- **OAuth**: Requires `Google` and `Microsoft` ClientIds/Secrets in `appsettings.json`.
- **Redis**: Required for Rate Limiting and Token Revocation.
