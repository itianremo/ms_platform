using MediatR;
using Apps.Domain.Repositories;
using MassTransit;
using Shared.Messaging.Events;

namespace Apps.Application.Features.Subscriptions.Commands.ChangeStatus;

public record ChangeSubscriptionStatusCommand(Guid SubscriptionId, bool IsActive) : IRequest<bool>;

public class ChangeSubscriptionStatusCommandHandler : IRequestHandler<ChangeSubscriptionStatusCommand, bool>
{
    private readonly IUserSubscriptionRepository _repository;
    private readonly IPublishEndpoint _publishEndpoint;

    public ChangeSubscriptionStatusCommandHandler(IUserSubscriptionRepository repository, IPublishEndpoint publishEndpoint)
    {
        _repository = repository;
        _publishEndpoint = publishEndpoint;
    }

    public async Task<bool> Handle(ChangeSubscriptionStatusCommand request, CancellationToken cancellationToken)
    {
        var subscription = await _repository.GetByIdAsync(request.SubscriptionId);
        if (subscription == null) return false;

        if (request.IsActive) 
        {
            subscription.Activate();
        }
        else 
        {
            subscription.Cancel();
        }

        await _repository.UpdateAsync(subscription);

        await _publishEndpoint.Publish(new SubscriptionStatusChangedEvent
        {
            SubscriptionId = subscription.Id,
            UserId = subscription.UserId,
            AppId = subscription.AppId,
            IsActive = subscription.IsActive,
            NewExpiry = subscription.IsActive ? subscription.EndDate : null 
        }, cancellationToken);

        return true;
    }
}
