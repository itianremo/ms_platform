using Audit.Domain.Entities;
using Audit.Infrastructure.Repositories;
using MassTransit;
using Shared.Messaging.Events;

namespace Audit.Infrastructure.Messaging;

public class AuditLogConsumer : IConsumer<AuditLogCreatedEvent>
{
    private readonly IAuditRepository _repository;

    public AuditLogConsumer(IAuditRepository repository)
    {
        _repository = repository;
    }

    public async Task Consume(ConsumeContext<AuditLogCreatedEvent> context)
    {
        var message = context.Message;

        var auditLog = new AuditLog(
            message.Action,
            message.EntityName,
            message.EntityId,
            message.UserId,
            message.AppId,
            message.ChangesJson
        )
        {
            Timestamp = message.Timestamp
        };

        await _repository.AddAsync(auditLog);
    }
}
