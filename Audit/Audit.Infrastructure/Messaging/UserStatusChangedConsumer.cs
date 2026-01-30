using Audit.Domain.Entities;
using Audit.Infrastructure.Repositories;
using MassTransit;
using Shared.Messaging.Events;

namespace Audit.Infrastructure.Messaging;

public class UserStatusChangedConsumer : IConsumer<UserStatusChangedEvent>
{
    private readonly IAuditRepository _repository;

    public UserStatusChangedConsumer(IAuditRepository repository)
    {
        _repository = repository;
    }

    public async Task Consume(ConsumeContext<UserStatusChangedEvent> context)
    {
        var message = context.Message;
        string changes = $"Status changed from {message.OldStatus} to {message.NewStatus}";

        var auditLog = new AuditLog(
            action: "GlobalStatusChange",
            entityName: "User",
            entityId: message.UserId.ToString(),
            userId: message.PerformedByUserId, // Might be null
            appId: null, // Global action
            changesJson: changes // Storing simple string for now or serialized object
        )
        {
            Timestamp = message.Timestamp
        };

        await _repository.AddAsync(auditLog);
    }
}
