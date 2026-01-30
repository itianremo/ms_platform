using Audit.Domain.Entities;
using Audit.Infrastructure.Repositories;
using MassTransit;
using Shared.Messaging.Events;

namespace Audit.Infrastructure.Messaging;

public class RoleAssignedConsumer : IConsumer<RoleAssignedEvent>
{
    private readonly IAuditRepository _repository;

    public RoleAssignedConsumer(IAuditRepository repository)
    {
        _repository = repository;
    }

    public async Task Consume(ConsumeContext<RoleAssignedEvent> context)
    {
        var message = context.Message;
        string changes = $"Role changed from {message.OldRoleName} to {message.RoleName} (RoleId: {message.RoleId})";

        var auditLog = new AuditLog(
            action: "RoleAssignment",
            entityName: "UserAppMembership",
            entityId: $"{message.UserId}:{message.AppId}", // Composite key representation
            userId: message.PerformedByUserId,
            appId: message.AppId, 
            changesJson: changes
        )
        {
            Timestamp = message.Timestamp
        };

        await _repository.AddAsync(auditLog);
    }
}
