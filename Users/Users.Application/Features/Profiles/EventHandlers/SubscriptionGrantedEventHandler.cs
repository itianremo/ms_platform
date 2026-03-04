using MassTransit;
using Microsoft.Extensions.Logging;
using Shared.Messaging.Events;
using Users.Domain.Repositories;

namespace Users.Application.Features.Profiles.EventHandlers;

public class SubscriptionGrantedEventHandler : IConsumer<SubscriptionGrantedEvent>
{
    private readonly IUserProfileRepository _profileRepository;
    private readonly ILogger<SubscriptionGrantedEventHandler> _logger;

    public SubscriptionGrantedEventHandler(IUserProfileRepository profileRepository, ILogger<SubscriptionGrantedEventHandler> logger)
    {
        _profileRepository = profileRepository;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<SubscriptionGrantedEvent> context)
    {
        var msg = context.Message;
        _logger.LogInformation("Processing SubscriptionGrantedEvent for User {UserId} App {AppId}", msg.UserId, msg.AppId);

        var profile = await _profileRepository.GetByUserIdAndAppIdAsync(msg.UserId, msg.AppId);
        
        if (profile == null)
        {
            _logger.LogWarning("Profile not found for User {UserId} App {AppId}. Creating generic profile to store loyalty points...", msg.UserId, msg.AppId);
            return;
        }

        // Calculate 10% loyalty points based on the amount paid
        int loyaltyPointsToAdd = (int)Math.Round(msg.PricePaid * 0.10m, MidpointRounding.AwayFromZero);

        if (loyaltyPointsToAdd > 0)
        {
            profile.AddLoyaltyPoints(loyaltyPointsToAdd);
            _logger.LogInformation("Added {Points} loyalty points to User {UserId}", loyaltyPointsToAdd, msg.UserId);
        }

        if (msg.CoinsAmount > 0)
        {
            profile.AddCoins(msg.CoinsAmount);
            _logger.LogInformation("Added {Coins} coins to User {UserId}", msg.CoinsAmount, msg.UserId);
        }

        if (loyaltyPointsToAdd > 0 || msg.CoinsAmount > 0)
        {
            await _profileRepository.UpdateAsync(profile);
            _logger.LogInformation("Successfully updated Wallet/Loyalty for User {UserId}", msg.UserId);
        }
    }
}
