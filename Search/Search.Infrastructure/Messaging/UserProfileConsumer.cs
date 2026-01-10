using MassTransit;
using Search.Domain.Entities;
using Search.Domain.Repositories;
using Shared.Messaging.Events;

namespace Search.Infrastructure.Messaging;

public class UserProfileConsumer : 
    IConsumer<UserProfileCreatedEvent>,
    IConsumer<UserProfileUpdatedEvent>
{
    private readonly ISearchRepository _repository;

    public UserProfileConsumer(ISearchRepository repository)
    {
        _repository = repository;
    }

    public async Task Consume(ConsumeContext<UserProfileCreatedEvent> context)
    {
        var msg = context.Message;
        var profile = new UserSearchProfile
        {
            UserId = msg.UserId,
            AppId = msg.AppId,
            DisplayName = msg.DisplayName,
            Bio = msg.Bio,
            Age = msg.Age,
            Gender = msg.Gender,
            InterestsJson = msg.InterestsJson,
            IsVisible = msg.IsVisible,
            LastActive = DateTime.UtcNow
        };

        // Check if exists first? Or just try add. 
        // For simplicity, assuming new.
        await _repository.AddAsync(profile);
    }

    public async Task Consume(ConsumeContext<UserProfileUpdatedEvent> context)
    {
        var msg = context.Message;
        
        // Find existing
        // SearchRepository needs a GetByUserId method effectively. 
        // Using ListAsync for now as a workaround or add specific method.
        var profiles = await _repository.ListAsync(x => x.UserId == msg.UserId && x.AppId == msg.AppId);
        var existing = profiles.FirstOrDefault();

        if (existing != null)
        {
            existing.DisplayName = msg.DisplayName;
            existing.Bio = msg.Bio;
            existing.Age = msg.Age;
            existing.Gender = msg.Gender;
            existing.InterestsJson = msg.InterestsJson;
            existing.IsVisible = msg.IsVisible;
            existing.LastActive = DateTime.UtcNow;

            await _repository.UpdateAsync(existing);
        }
        else 
        {
            // Create if missing (self-healing)
            var profile = new UserSearchProfile
            {
                UserId = msg.UserId,
                AppId = msg.AppId,
                DisplayName = msg.DisplayName,
                Bio = msg.Bio,
                Age = msg.Age,
                Gender = msg.Gender,
                InterestsJson = msg.InterestsJson,
                IsVisible = msg.IsVisible,
                LastActive = DateTime.UtcNow
            };
            await _repository.AddAsync(profile);
        }
    }
}
