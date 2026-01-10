# Shared Kernel & Infrastructure

A class library containing common building blocks used across all microservices.

## Components
- **Shared.Kernel**: Interfaces (`IEntity`, `IRepository`, `IEventBus`, `ICacheService`).
- **Shared.Infrastructure**: Implementations (`MassTransitEventBus`, `RedisCacheService`, `EfCoreRepository`).
- **Shared.Messaging**: Integration Events (Contracts) shared between producers and consumers.

## Usage
Add reference to `Shared.Infrastructure` in any new microservice to gain access to the EventBus and Caching capabilities.
