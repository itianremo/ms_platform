using MassTransit;
using Shared.Messaging.Events;
using Users.Domain.Entities;
using Users.Domain.Repositories;

namespace Users.Application.Features.Users.EventConsumers;

public class UserRegisteredConsumer : IConsumer<UserRegisteredEvent>
{
    private readonly IUserProfileRepository _repository;

    public UserRegisteredConsumer(IUserProfileRepository repository)
    {
        _repository = repository;
    }

    public async Task Consume(ConsumeContext<UserRegisteredEvent> context)
    {
        var message = context.Message;

        // Check if profile exists, if not create
        var existing = await _repository.GetByUserIdAndAppIdAsync(message.UserId, message.AppId);
        if (existing == null)
        {
            var profile = new UserProfile(message.UserId, message.AppId, message.DisplayName);
            await _repository.AddAsync(profile);
        }
    }
}
