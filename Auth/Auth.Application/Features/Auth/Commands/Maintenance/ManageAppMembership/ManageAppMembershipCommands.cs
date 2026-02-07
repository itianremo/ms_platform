using MediatR;
using Auth.Domain.Repositories;
using Auth.Domain.Entities;
using Shared.Kernel; 

namespace Auth.Application.Features.Auth.Commands.Maintenance.ManageAppMembership;

// 1. Add User to App
public record AddUserToAppCommand(Guid UserId, Guid AppId, Guid? RoleId = null) : IRequest;

public class AddUserToAppCommandHandler : IRequestHandler<AddUserToAppCommand>
{
    private readonly IUserRepository _userRepository;

    public AddUserToAppCommandHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task Handle(AddUserToAppCommand request, CancellationToken cancellationToken)
    {
        var targetUser = await _userRepository.GetUserWithRolesAsync(request.UserId);
        if (targetUser == null) throw new Shared.Kernel.Exceptions.NotFoundException(nameof(global::Auth.Domain.Entities.User), request.UserId);

        if (targetUser.Memberships.Any(m => m.AppId == request.AppId))
        {
            throw new InvalidOperationException("User is already a member of this app.");
        }

        Guid roleId = request.RoleId ?? Guid.Empty;
        if (roleId == Guid.Empty)
        {
            var defaultRole = await _userRepository.GetRoleByNameAsync(request.AppId, "Visitor");
            if (defaultRole == null) 
            {
                 // Fallback if Visitor doesn't exist, though it should.
                 defaultRole = await _userRepository.GetRoleByNameAsync(request.AppId, "User") 
                               ?? await _userRepository.GetRoleByNameAsync(request.AppId, "NormalUser");
            }
            
            if (defaultRole == null) throw new InvalidOperationException("Default role (Visitor) not found for app.");
            roleId = defaultRole.Id;
        }

        var membership = new UserAppMembership(targetUser.Id, request.AppId, roleId);
        targetUser.AddMembership(membership);

        await _userRepository.UpdateAsync(targetUser);
    }
}

// 2. Update Membership Status
public record UpdateAppMembershipStatusCommand(Guid UserId, Guid AppId, AppUserStatus NewStatus) : IRequest;

public class UpdateAppMembershipStatusCommandHandler : IRequestHandler<UpdateAppMembershipStatusCommand>
{
    private readonly IUserRepository _userRepository;

    public UpdateAppMembershipStatusCommandHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task Handle(UpdateAppMembershipStatusCommand request, CancellationToken cancellationToken)
    {
        var targetUser = await _userRepository.GetUserWithRolesAsync(request.UserId);
        if (targetUser == null) throw new Shared.Kernel.Exceptions.NotFoundException(nameof(global::Auth.Domain.Entities.User), request.UserId);

        targetUser.UpdateMembershipStatus(request.AppId, request.NewStatus);
        
        await _userRepository.UpdateAsync(targetUser);
    }
}

// 3. Remove User From App (Hard Delete Membership or Ban?)
// Prompt says "remove a user from app". Let's assume hard delete of membership for now, or soft delete if we had it.
// Entity doesn't show SoftDelete on Membership. Let's Ban instead or actually remove?
// "Remove" usually implies taking them out of the list.
// But EF Core removal from collection requires careful handling.
// Let's implement Remove as "Remove from Collection".
public record RemoveUserFromAppCommand(Guid UserId, Guid AppId) : IRequest;

public class RemoveUserFromAppCommandHandler : IRequestHandler<RemoveUserFromAppCommand>
{
    private readonly IUserRepository _userRepository;
    private readonly MassTransit.IPublishEndpoint _publishEndpoint;

    public RemoveUserFromAppCommandHandler(IUserRepository userRepository, MassTransit.IPublishEndpoint publishEndpoint)
    {
        _userRepository = userRepository;
        _publishEndpoint = publishEndpoint;
    }

    public async Task Handle(RemoveUserFromAppCommand request, CancellationToken cancellationToken)
    {
         var targetUser = await _userRepository.GetUserWithRolesAsync(request.UserId);
         if (targetUser == null) throw new Shared.Kernel.Exceptions.NotFoundException(nameof(global::Auth.Domain.Entities.User), request.UserId);
         
         var membership = targetUser.Memberships.FirstOrDefault(m => m.AppId == request.AppId);
         if (membership != null)
         {
             var roleId = membership.RoleId;
             // Try to get Role Name if loaded, otherwise empty
             var roleName = membership.Role?.Name ?? "Unknown";

             targetUser.RemoveMembership(request.AppId);
             
             await _userRepository.UpdateAsync(targetUser);

             await _publishEndpoint.Publish(new Shared.Messaging.Events.UserRoleRemovedEvent(request.UserId, request.AppId, roleId, roleName), cancellationToken);
         }
    }
}
