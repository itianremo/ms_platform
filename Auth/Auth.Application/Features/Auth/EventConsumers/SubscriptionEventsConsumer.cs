using MassTransit;
using Auth.Domain.Repositories;
using Auth.Domain.Entities;
using Shared.Messaging.Events;
using Microsoft.Extensions.Logging;

namespace Auth.Application.Features.Auth.EventConsumers;

public class SubscriptionEventsConsumer : 
    IConsumer<SubscriptionGrantedEvent>,
    IConsumer<SubscriptionStatusChangedEvent>
{
    private readonly IUserRepository _userRepository;
    private readonly ILogger<SubscriptionEventsConsumer> _logger;

    public SubscriptionEventsConsumer(IUserRepository userRepository, ILogger<SubscriptionEventsConsumer> logger)
    {
        _userRepository = userRepository;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<SubscriptionGrantedEvent> context)
    {
        var msg = context.Message;
        _logger.LogInformation("Processing Subscription Granted for User {UserId} App {AppId}", msg.UserId, msg.AppId);

        // Subscriptions and Expirations are now managed by the Users.API.
        // This consumer is deprecated in Auth.API and should be migrated to Users.API or removed entirely.
        await Task.CompletedTask;
    }

    public async Task Consume(ConsumeContext<SubscriptionStatusChangedEvent> context)
    {
        var msg = context.Message;
        _logger.LogInformation("Processing Subscription Status Change for User {UserId}", msg.UserId);

        // Subscription handling migrated to Users.API as part of Membership consolidation.
        await Task.CompletedTask;
    }
}
