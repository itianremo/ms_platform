# Shared Kernel (Common Library)

Core shared library containing building blocks for Clean Architecture and Domain-Driven Design.

## 📦 Components
- **Domain**: Base Entity, AggregateRoot, ValueObject implementations.
- **Application**: CQRS Behaviours (Validation, Logging), Exceptions, Interfaces.
- **Infrastructure**: EF Core configurations, Repository implementations, MassTransit consumers.
- **Messaging**: Event definitions shared across microservices.

## 🔨 Usage
This project is referenced by all microservices to ensure consistency in patterns like:
- Result Pattern
- Domain Events
- Repository Pattern
- Cross-Cutting Concerns

## 🚀 Building
```bash
dotnet build Shared/Shared.Kernel/Shared.Kernel.csproj
```
