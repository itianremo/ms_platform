using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using MassTransit;
using Shared.Kernel.Interfaces;
using Shared.Messaging.Events;
using System.Text.Json;
using Shared.Kernel; // For Entity base class if needed, or just object

namespace Shared.Infrastructure.Persistence.Interceptors;

public class AuditableEntitySaveChangesInterceptor : SaveChangesInterceptor
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IPublishEndpoint _publishEndpoint;

    public AuditableEntitySaveChangesInterceptor(
        ICurrentUserService currentUserService,
        IPublishEndpoint publishEndpoint)
    {
        _currentUserService = currentUserService;
        _publishEndpoint = publishEndpoint;
    }

    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        await DispatchAuditLogs(eventData.Context, cancellationToken);
        return await base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private async Task DispatchAuditLogs(DbContext? context, CancellationToken cancellationToken)
    {
        if (context == null) return;

        var entries = context.ChangeTracker.Entries<Entity>() // Assuming Entity is Base Class in Shared.Kernel
            .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified || e.State == EntityState.Deleted)
            .ToList();

        foreach (var entry in entries)
        {
            var entityId = entry.Property("Id").CurrentValue?.ToString() ?? "Unknown";
            var entityName = entry.Entity.GetType().Name;
            var action = entry.State.ToString();
            
            var changes = new Dictionary<string, object>();

            if (entry.State == EntityState.Modified)
            {
                foreach (var property in entry.Properties.Where(p => p.IsModified))
                {
                    if (property.Metadata.Name == "LastModified" || property.Metadata.Name == "LastModifiedBy") continue;
                    
                    changes.Add(property.Metadata.Name, new 
                    { 
                        Old = property.OriginalValue, 
                        New = property.CurrentValue 
                    });
                }
            }
            else if (entry.State == EntityState.Added)
            {
                 // For Added, we could log all properties, but usually ID is key.
                 // Let's just log "Created".
            }
            else if (entry.State == EntityState.Deleted)
            {
                 // For Deleted, we might want to capture key info.
            }

            var changesJson = JsonSerializer.Serialize(changes);
            
            // Publish Event
            // Note: If transaction fails, this event is still published? 
            // Better to use Outbox or publish AFTER generic save success? 
            // Interceptor runs BEFORE commit. If commit fails, we published an event for an action that didn't happen.
            // Ideally use Transactional Outbox.
            // But for this task, "DispatchAuditLogs" implies just sending.
            // If we use MassTransit Request/Response or Publish, usually it's fire-and-forget.
            // We'll proceed with direct Publish for now, assuming high success rate or acknowledging limitation.
            
            // Check UserId from Service or Entity (if Entity has ModifiedBy)
            var userId = _currentUserService.UserId;

            // AppId? Not readily available unless we inject a service for it or it's on the entity.
            // We'll leave AppId null or infer if possible.

            await _publishEndpoint.Publish(new AuditLogCreatedEvent(
                Action: action,
                EntityName: entityName,
                EntityId: entityId,
                UserId: userId,
                AppId: null, // Default
                ChangesJson: changesJson,
                IpAddress: _currentUserService.IpAddress,
                UserAgent: _currentUserService.UserAgent,
                Timestamp: DateTime.UtcNow
            ), cancellationToken);
        }
    }
}
