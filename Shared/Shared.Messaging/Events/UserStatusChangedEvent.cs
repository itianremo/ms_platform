using Shared.Kernel;
using Shared.Kernel.Enums;

namespace Shared.Messaging.Events;

public record UserStatusChangedEvent(
    Guid UserId, 
    GlobalUserStatus OldStatus, 
    GlobalUserStatus NewStatus, 
    Guid? PerformedByUserId, 
    DateTime Timestamp
);
