# Auth Service (Identity & Access)

Manages User Identity, Authentication (JWT), and Authorization (RBAC) for the entire platform.

## Features
- **OIDC/SSO**: Supports secure cookie sharing across subdomains.
- **RBAC**: Application-scoped Roles and Permissions.
- **Superadmin Protection**: Sealed entities logic.
- **Social Login**: Google, Facebook Integration.

## Tech Stack
- **.NET 8** (Onion Architecture)
- **SQL Server** (Persistence)
- **MassTransit** (Event Publishing)

## Key Endpoints
- `POST /api/auth/login`: Issue JWT & Cookie.
- `POST /api/auth/register`: Create new user.
- `POST /api/auth/external-login`: Social Auth.
