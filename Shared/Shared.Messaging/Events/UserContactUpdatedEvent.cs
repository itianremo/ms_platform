using System;

namespace Shared.Messaging.Events;

public record UserContactUpdatedEvent(Guid UserId, string? NewEmail, string? NewPhone);
