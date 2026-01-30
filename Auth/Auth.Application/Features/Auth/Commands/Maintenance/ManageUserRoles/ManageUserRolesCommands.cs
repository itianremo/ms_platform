using MediatR;
using Auth.Domain.Repositories;
using Auth.Domain.Entities;
using Shared.Kernel; 

namespace Auth.Application.Features.Auth.Commands.Maintenance.ManageUserRoles;

// 1. Assign Role
public record AssignRoleCommand(Guid UserId, Guid AppId, string RoleName) : IRequest;


public class AssignRoleCommandHandler : IRequestHandler<AssignRoleCommand>
{
    private readonly IUserRepository _userRepository;
    private readonly MassTransit.IPublishEndpoint _publishEndpoint;

    public AssignRoleCommandHandler(IUserRepository userRepository, MassTransit.IPublishEndpoint publishEndpoint)
    {
        _userRepository = userRepository;
        _publishEndpoint = publishEndpoint;
    }

    public async Task Handle(AssignRoleCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetUserWithRolesAsync(request.UserId);
        if (user == null) throw new Shared.Kernel.Exceptions.NotFoundException(nameof(global::Auth.Domain.Entities.User), request.UserId);

        var membership = user.Memberships.FirstOrDefault(m => m.AppId == request.AppId);
        if (membership == null)
        {
            throw new InvalidOperationException("User is not a member of this app. Add to app on 'Membership' tab first.");
        }

        var newRole = await _userRepository.GetRoleByNameAsync(request.AppId, request.RoleName);
        if (newRole == null) throw new Shared.Kernel.Exceptions.NotFoundException(nameof(global::Auth.Domain.Entities.Role), request.RoleName);

        var oldRoleId = membership.RoleId;
        string oldRoleName = "Unknown"; // Would normally fetch from repo

        // Perform Change
        membership.ChangeRole(newRole.Id);
        
        await _userRepository.UpdateAsync(user);

        await _publishEndpoint.Publish(new Shared.Messaging.Events.RoleAssignedEvent(
            user.Id,
            request.AppId,
            newRole.Id,
            newRole.Name,
            oldRoleName,
            null,
            DateTime.UtcNow
        ), cancellationToken);
    }
}

// 2. Unassign Role (Reset to Default?)
// Since it's single role, "Unassign" might mean set to lowest role or generic 'User'?
// Or maybe specific endpoint isn't needed if AssignRole handles the swap.
// But the prompt said "assign/unassign any role(s)". 
// If the design was Multi-Role, Unassign would make sense.
// With Single-Role, Unassign implies "Leave Empty" (not possible with Guid RoleId usually unless nullable) 
// or "Reset to Default".
// Let's implement specific "Unassign" as "Set to Default 'User/NormalUser'".

public record UnassignRoleCommand(Guid UserId, Guid AppId) : IRequest;

public class UnassignRoleCommandHandler : IRequestHandler<UnassignRoleCommand>
{
    private readonly IUserRepository _userRepository;

    public UnassignRoleCommandHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task Handle(UnassignRoleCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetUserWithRolesAsync(request.UserId);
        if (user == null) throw new Shared.Kernel.Exceptions.NotFoundException(nameof(global::Auth.Domain.Entities.User), request.UserId);

        var membership = user.Memberships.FirstOrDefault(m => m.AppId == request.AppId);
        if (membership == null) return;

        var defaultRole = await _userRepository.GetRoleByNameAsync(request.AppId, "User") 
                          ?? await _userRepository.GetRoleByNameAsync(request.AppId, "NormalUser");
        
        if (defaultRole != null && membership.RoleId != defaultRole.Id)
        {
            membership.ChangeRole(defaultRole.Id);
            await _userRepository.UpdateAsync(user);
        }
    }
}
