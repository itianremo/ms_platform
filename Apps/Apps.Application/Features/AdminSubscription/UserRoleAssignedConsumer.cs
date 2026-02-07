using MassTransit;
using MediatR;
using Shared.Messaging.Events;
using Apps.Application.Features.Subscriptions.Commands.GrantSubscription;
using Apps.Domain.Repositories; // Assuming this exists or we query via Mediator? 
// Actually we need to find the Unlimited Package. 
// Can we do that via Mediator? GetPackagesQuery?
// Or assume a convention.

namespace Apps.Application.Features.AdminSubscription;

public class UserRoleAssignedConsumer : IConsumer<UserRoleAssignedEvent>
{
    private readonly IMediator _mediator;
    private readonly ISubscriptionPackageRepository _packageRepository;
    private readonly IUserSubscriptionRepository _userSubscriptionRepository;

    public UserRoleAssignedConsumer(IMediator mediator, ISubscriptionPackageRepository packageRepository, IUserSubscriptionRepository userSubscriptionRepository)
    {
        _mediator = mediator;
        _packageRepository = packageRepository;
        _userSubscriptionRepository = userSubscriptionRepository;
    }

    public async Task Consume(ConsumeContext<UserRoleAssignedEvent> context)
    {
        var msg = context.Message;
        
        if (msg.RoleName.Contains("Admin", StringComparison.OrdinalIgnoreCase) || 
            msg.RoleName.Contains("Manager", StringComparison.OrdinalIgnoreCase))
        {
            // Find "Unlimited" package for this App
            var packages = await _packageRepository.GetByAppIdAsync(msg.AppId);
            var unlimitedPkg = packages.FirstOrDefault(p => p.Name.Contains("Unlimited", StringComparison.OrdinalIgnoreCase));
            
            if (unlimitedPkg != null)
            {
                var command = new GrantSubscriptionCommand(msg.AppId, msg.UserId, unlimitedPkg.Id, null, null);
                await _mediator.Send(command);
            }
        }
        else
        {
            // Revoke VIP if Demoted (Role is no longer Admin/Manager)
            var subscriptions = await _userSubscriptionRepository.GetByUserIdAsync(msg.AppId, msg.UserId);
            
            // Find active VIP Unlimited subscription
            // We need to look up the package name for these subscriptions to be sure, OR checks package Ids.
            // Since we don't have navigation property to Package in UserSubscription (only PackageId), 
            // we first need to find the Unlimited Package ID for this App.
            
            var packages = await _packageRepository.GetByAppIdAsync(msg.AppId);
            var unlimitedPkg = packages.FirstOrDefault(p => p.Name.Contains("Unlimited", StringComparison.OrdinalIgnoreCase));

            if (unlimitedPkg != null)
            {
                var vipSub = subscriptions.FirstOrDefault(s => s.PackageId == unlimitedPkg.Id && s.IsActive);
                if (vipSub != null)
                {
                    vipSub.ExpireNow();
                    await _userSubscriptionRepository.UpdateAsync(vipSub);
                }
            }
        }
    }
}
