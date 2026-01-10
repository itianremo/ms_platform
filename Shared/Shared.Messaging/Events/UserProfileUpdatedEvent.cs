namespace Shared.Messaging.Events;

public record UserProfileUpdatedEvent(
    Guid UserId,
    Guid AppId,
    string DisplayName,
    string Bio,
    int Age,
    string Gender,
    string InterestsJson,
    bool IsVisible
);
