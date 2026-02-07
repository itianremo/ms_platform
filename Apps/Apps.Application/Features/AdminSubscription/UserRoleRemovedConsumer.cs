using MassTransit;
using Apps.Domain.Repositories;
using Shared.Messaging.Events;

namespace Apps.Application.Features.AdminSubscription;

public class UserRoleRemovedConsumer : IConsumer<UserRoleRemovedEvent>
{
    private readonly ISubscriptionPackageRepository _packageRepository;
    private readonly IUserSubscriptionRepository _userSubscriptionRepository;

    public UserRoleRemovedConsumer(ISubscriptionPackageRepository packageRepository, IUserSubscriptionRepository userSubscriptionRepository)
    {
        _packageRepository = packageRepository;
        _userSubscriptionRepository = userSubscriptionRepository;
    }

    public async Task Consume(ConsumeContext<UserRoleRemovedEvent> context)
    {
        var msg = context.Message;

        // Revoke VIP Unlimited if user is removed from App
        var packages = await _packageRepository.GetByAppIdAsync(msg.AppId);
        var unlimitedPkg = packages.FirstOrDefault(p => p.Name.Contains("Unlimited", StringComparison.OrdinalIgnoreCase));

        if (unlimitedPkg != null)
        {
            var subscriptions = await _userSubscriptionRepository.GetByUserIdAsync(msg.AppId, msg.UserId);
            var vipSub = subscriptions.FirstOrDefault(s => s.PackageId == unlimitedPkg.Id && s.IsActive);
            
            if (vipSub != null)
            {
                vipSub.ExpireNow();
                await _userSubscriptionRepository.UpdateAsync(vipSub);
            }
        }
    }
}
