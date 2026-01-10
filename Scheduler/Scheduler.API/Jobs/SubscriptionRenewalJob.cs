using Hangfire;
using MassTransit;
using Shared.Messaging.Commands;

namespace Scheduler.API.Jobs;

public class SubscriptionRenewalJob
{
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<SubscriptionRenewalJob> _logger;

    public SubscriptionRenewalJob(IPublishEndpoint publishEndpoint, ILogger<SubscriptionRenewalJob> logger)
    {
        _publishEndpoint = publishEndpoint;
        _logger = logger;
    }

    public async Task ExecuteAsync()
    {
        _logger.LogInformation("Creating Subscription Renewal Command...");
        await _publishEndpoint.Publish(new ProcessSubscriptionRenewals());
        _logger.LogInformation("Subscription Renewal Command Published.");
    }
}
