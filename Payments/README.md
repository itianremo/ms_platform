# Payments Service (Dynamic Gateway)

Manages subscriptions, plans, and processes payments via configurable gateways.

## Features
- **Strategy Pattern**: Abstracts payment providers (`IPaymentGateway`).
- **Dynamic Switching**: Supports switching between **Stripe** and **Mock** gateways per tenant configuration.
- **Plans & Subscriptions**: Manages billing cycles and platform access.

## Tech Stack
- **.NET 8**
- **PostgreSQL**
