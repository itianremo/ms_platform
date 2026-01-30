using MassTransit;
using Shared.Messaging.Events;
using Users.Domain.Repositories;
using Microsoft.Extensions.Logging;

namespace Users.Application.Features.Users.EventConsumers;

public class UserContactUpdatedConsumer : IConsumer<UserContactUpdatedEvent>
{
    private readonly IUserProfileRepository _profileRepository;
    private readonly ILogger<UserContactUpdatedConsumer> _logger;

    public UserContactUpdatedConsumer(IUserProfileRepository profileRepository, ILogger<UserContactUpdatedConsumer> logger)
    {
        _profileRepository = profileRepository;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<UserContactUpdatedEvent> context)
    {
        var message = context.Message;
        _logger.LogInformation("Processing Contact Update for User {UserId}", message.UserId);

        var profiles = await _profileRepository.GetAllByUserIdAsync(message.UserId);
        foreach (var profile in profiles)
        {
            /* 
            // Update Phone if changed (DEPRECATED: Phone is now sourced from AuthDb)
            if (message.NewPhone != profile.PhoneNumber)
            {
                profile.UpdatePhone(message.NewPhone);
                await _profileRepository.UpdateAsync(profile);
                _logger.LogInformation("Updated Phone for UserProfile {Id} (App {AppId})", profile.Id, profile.AppId);
            }
            */
        }
    }
}
