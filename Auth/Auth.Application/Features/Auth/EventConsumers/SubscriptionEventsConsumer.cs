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

        var user = await _userRepository.GetUserWithRolesAsync(msg.UserId);
        if (user == null) 
        {
            _logger.LogWarning("User {UserId} not found for subscription grant.", msg.UserId);
            return;
        }

        var membership = user.Memberships.FirstOrDefault(m => m.AppId == msg.AppId);
        if (membership == null)
        {
             _logger.LogWarning("User {UserId} has no membership in App {AppId}. Cannot attach subscription.", msg.UserId, msg.AppId);
             // Should we create membership? Usually access implies membership. 
             // Admin granting subscription implies they want the user to have access?
             // But Role? We don't know the Role. 
             // Assume explicit membership exists.
             return;
        }

        // Update Expiry
        membership.UpdateSubscriptionExpiry(msg.EndDate);
        
        // Ensure Active Status if previously pending? 
        // Admin Grant -> Should probably ensure Active Status.
        if (membership.Status != AppUserStatus.Active && membership.Status != AppUserStatus.Banned)
        {
            membership.Activate();
        }

        // Also, if the Package is VIP, we might want to ensure they have VIP role?
        // But Role is separate from Subscription in this design (Role = Permission Set).
        // The implementation in LoginHandler *checks* Subscription to *allow* Role.
        // So we just need to update the date. 
        // However, if the user doesn't HAVE the VIP role, they won't get VIP features even if subscribed?
        // The Prompt says: "grant subscription to users".
        // If the user is separate from Role, then we just update Date.
        // If the user has "Visitor" role, and gets "VIP Subscription", do they get "VIP" role?
        // Plan: Just update Date. Role assignment is separate (AssignRoles permission).
        // Or should we Upgrade Role? 
        // "When vip signs in... role will be checked". This implies they HAVE the role.
        // So Admin should probably assign Role too if they want full effect.
        // But for now, focus on Subscription Date logic.

        await _userRepository.UpdateAsync(user);
    }

    public async Task Consume(ConsumeContext<SubscriptionStatusChangedEvent> context)
    {
        var msg = context.Message;
        _logger.LogInformation("Processing Subscription Status Change for User {UserId}", msg.UserId);

        var user = await _userRepository.GetUserWithRolesAsync(msg.UserId);
        if (user == null) return;

        var membership = user.Memberships.FirstOrDefault(m => m.AppId == msg.AppId);
        if (membership != null)
        {
            membership.UpdateSubscriptionExpiry(msg.NewExpiry);
            await _userRepository.UpdateAsync(user);
        }
    }
}
