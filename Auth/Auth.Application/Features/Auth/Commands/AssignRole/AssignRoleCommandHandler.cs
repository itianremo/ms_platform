using MediatR;
using Auth.Domain.Entities;
using Auth.Domain.Repositories;

namespace Auth.Application.Features.Auth.Commands.AssignRole;

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
        var user = await _userRepository.GetByIdAsync(request.UserId);
        if (user == null)
            throw new KeyNotFoundException("User not found");

        // Verify/Fetch Role Name
        var role = await _userRepository.GetRoleByIdAsync(request.RoleId);
        if (role == null)
            throw new KeyNotFoundException("Role not found");

        // Memberships and App roles are now updated via Users.API listening to the RoleAssignedEvent.

        // Publish Event if Admin or Manager
        if (role.Name.Contains("Admin", StringComparison.OrdinalIgnoreCase) || role.Name.Contains("Manager", StringComparison.OrdinalIgnoreCase))
        {
             await _publishEndpoint.Publish(new Shared.Messaging.Events.UserRoleAssignedEvent(request.UserId, request.AppId, request.RoleId, role.Name), cancellationToken);
        }
    }
}
