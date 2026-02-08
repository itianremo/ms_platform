# Global Admin Portal

## Overview
The **Global Admin Portal** is the "Super-App" for the Unified Microservices Platform (UMP). It serves as the central control plane for IT Operations and Support teams to manage the entire multi-tenant ecosystem.

## Key Capabilities
1.  **Tenant Management**: Onboard new applications (e.g., "App C"), generate API keys, and configure callbacks.
2.  **Global Configuration**:
    -   **Communication**: Configure the single global SMS (Twilio) and SMTP (SendGrid) providers.
    -   **Auth Providers**: Enable/Disable Social Logins (Google, Facebook, Microsoft, Apple) available to tenants.
    -   **Payment Gateways**: Manage available payment processors (Stripe, PayPal).
3.  **User Management**: Cross-tenant user search, ban/suspend users globally, and manage RBAC roles (`SuperAdmin`, `TenantAdmin`).
4.  **System Health**: View aggregate health status of all 25+ containers and access centralized logs (Seq).

## Development
-   **Stack**: React 19, Vite, TypeScript, Tailwind CSS, Shadcn/UI.
-   **Run**: `npm run dev` (Port 3000).
