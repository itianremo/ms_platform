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
            var defaultSettings = await _repository.GetAppDefaultSettingsAsync(message.AppId);
            if (string.IsNullOrWhiteSpace(defaultSettings) || defaultSettings == "{}")
            {
               // Retry logic or valid fallback? Already handled by repo to return "{}"
            }

            string displayName = message.DisplayName;
            
            // 1. Sanitize or Generate DisplayName
            // Pattern: user-{number} e.g. user-102938
            // Must be unique.
            
            if (string.IsNullOrWhiteSpace(displayName) || displayName == "N/A" || displayName == "Unknown" || displayName == "user")
            {
                 // Generate Unique Name
                 displayName = await GenerateUniqueDisplayNameAsync();
            }
            else
            {
                // Sanitize: Only Allow A-Z, 0-9, -, _, .
                var sanitized = System.Text.RegularExpressions.Regex.Replace(displayName, @"[^a-zA-Z0-9_\-\.]", "");
                if (string.IsNullOrWhiteSpace(sanitized))
                {
                     displayName = await GenerateUniqueDisplayNameAsync();
                }
                else 
                {
                    displayName = sanitized;
                }
            }
            
            // Ensure Uniqueness (Double Check)
            if (await _repository.ListAsync(u => u.DisplayName == displayName && u.AppId == message.AppId).ContinueWith(t => t.Result.Any()))
            {
                displayName = await GenerateUniqueDisplayNameAsync();
            }

            var profile = new UserProfile(message.UserId, message.AppId, displayName, defaultSettings, message.RoleId);
            await _repository.AddAsync(profile);
        }
    }

    private async Task<string> GenerateUniqueDisplayNameAsync()
    {
        // Simple Random/Timestamp strategy
        // user-67890
        var random = new Random();
        while (true)
        {
            var suffix = random.Next(10000, 99999);
            var name = $"user-{suffix}";
            
            // Check existence (Assuming Repository has a check method, or we use ListAsync which is inefficient but functional)
            // Ideally _repository.ExistsAsync(name)
            var exists = (await _repository.ListAsync(u => u.DisplayName == name)).Any();
            if (!exists) return name;
        }
    }
}
