namespace Shared.Messaging.Events;

public record SendOtpEvent(Guid UserId, string Destination, string Code, string Type); // Type: Email, Phone
