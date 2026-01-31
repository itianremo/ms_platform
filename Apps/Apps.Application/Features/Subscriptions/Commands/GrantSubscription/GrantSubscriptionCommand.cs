using MediatR;
using Apps.Domain.Entities;
using Apps.Domain.Repositories;
using MassTransit;
using Shared.Messaging.Events;

namespace Apps.Application.Features.Subscriptions.Commands.GrantSubscription;

public record GrantSubscriptionCommand(Guid UserId, Guid AppId, Guid PackageId, DateTime? StartDate, DateTime? EndDate) : IRequest<Guid>;

public class GrantSubscriptionCommandHandler : IRequestHandler<GrantSubscriptionCommand, Guid>
{
    private readonly IUserSubscriptionRepository _subscriptionRepository;
    private readonly ISubscriptionPackageRepository _packageRepository;
    private readonly IPublishEndpoint _publishEndpoint;

    public GrantSubscriptionCommandHandler(
        IUserSubscriptionRepository subscriptionRepository, 
        ISubscriptionPackageRepository packageRepository,
        IPublishEndpoint publishEndpoint)
    {
        _subscriptionRepository = subscriptionRepository;
        _packageRepository = packageRepository;
        _publishEndpoint = publishEndpoint;
    }

    public async Task<Guid> Handle(GrantSubscriptionCommand request, CancellationToken cancellationToken)
    {
        var package = await _packageRepository.GetByIdAsync(request.PackageId);
        if (package == null) throw new Exception("Package not found");

        var startDate = request.StartDate ?? DateTime.UtcNow;
        DateTime endDate;
        
        if (request.EndDate.HasValue)
        {
            endDate = request.EndDate.Value;
        }
        else
        {
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
        }

        string features = package.Description; 
        
        var subscription = new UserSubscription(
            request.UserId, 
            request.AppId, 
            request.PackageId, 
            startDate, 
            endDate, 
            package.Price - package.Discount, 
            features
        );

        await _subscriptionRepository.AddAsync(subscription);

        await _publishEndpoint.Publish(new SubscriptionGrantedEvent
        {
            UserId = request.UserId,
            AppId = request.AppId,
            PackageId = request.PackageId,
            StartDate = startDate,
            EndDate = endDate,
            IsActive = true
        }, cancellationToken);

        return subscription.Id;
    }
}
