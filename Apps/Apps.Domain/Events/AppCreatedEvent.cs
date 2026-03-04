using Shared.Kernel;

namespace Apps.Domain.Events;

public record AppCreatedEvent(Guid AppId, string Name) : IDomainEvent;
