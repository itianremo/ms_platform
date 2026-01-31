using MassTransit;
using Shared.Messaging.Events;
using Apps.Domain.Repositories;
using Apps.Domain.Entities;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Apps.Application.Features.Subscriptions.Consumers;

public class PaymentSucceededConsumer : IConsumer<PaymentSucceededEvent>
{
    private readonly ILogger<PaymentSucceededConsumer> _logger;
    private readonly IUserSubscriptionRepository _subscriptionRepo;
    private readonly ISubscriptionPackageRepository _packageRepo;
    private readonly IPublishEndpoint _publishEndpoint;

    public PaymentSucceededConsumer(
        ILogger<PaymentSucceededConsumer> logger,
        IUserSubscriptionRepository subscriptionRepo,
        ISubscriptionPackageRepository packageRepo,
        IPublishEndpoint publishEndpoint)
    {
        _logger = logger;
        _subscriptionRepo = subscriptionRepo;
        _packageRepo = packageRepo;
        _publishEndpoint = publishEndpoint;
    }

    public async Task Consume(ConsumeContext<PaymentSucceededEvent> context)
    {
        var message = context.Message;
        _logger.LogInformation("Processing PaymentSucceededEvent for User: {UserId}, Package: {PackageId}", message.UserId, message.PackageId);

        var package = await _packageRepo.GetByIdAsync(message.PackageId);
        if (package == null)
        {
            _logger.LogError("Package not found: {PackageId}", message.PackageId);
            return;
        }

        // Calculate Dates
        var startDate = DateTime.UtcNow;

        // Calculate EndDate based on Period
        DateTime endDate;
        switch (package.Period)
        {
             case SubscriptionPeriod.Weekly: endDate = startDate.AddDays(7); break;
             case SubscriptionPeriod.Monthly: endDate = startDate.AddMonths(1); break;
             case SubscriptionPeriod.Quarterly: endDate = startDate.AddMonths(3); break;
             case SubscriptionPeriod.SemiAnnually: endDate = startDate.AddMonths(6); break;
             case SubscriptionPeriod.Yearly: endDate = startDate.AddYears(1); break;
             case SubscriptionPeriod.Unlimited: endDate = startDate.AddYears(100); break;
             default: endDate = startDate.AddMonths((int)package.Period); break; 
        }

        // Create Subscription
        // Use logic similar to GrantSubscriptionCommandHandler
        var subscription = new UserSubscription(
            message.UserId,
            message.AppId,
            message.PackageId,
            startDate,
            endDate,
            message.Amount, // Use amount paid from event
            package.Description // Snapshot current features
        );

        await _subscriptionRepo.AddAsync(subscription);
        
        _logger.LogInformation("Subscription created for User {UserId} via Payment {TransactionId}", message.UserId, message.TransactionId);

        // Publish SubscriptionGrantedEvent for Auth Service to pick up
        await _publishEndpoint.Publish(new SubscriptionGrantedEvent
        {
            UserId = message.UserId,
            AppId = message.AppId,
            PackageId = message.PackageId,
            StartDate = startDate,
            EndDate = endDate,
            IsActive = true
        });
    }
}
