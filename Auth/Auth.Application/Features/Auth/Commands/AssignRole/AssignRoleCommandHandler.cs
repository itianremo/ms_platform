using MediatR;
using Auth.Domain.Entities;
using Auth.Domain.Repositories;

namespace Auth.Application.Features.Auth.Commands.AssignRole;

public class AssignRoleCommandHandler : IRequestHandler<AssignRoleCommand>
{
    private readonly IUserRepository _userRepository;

    public AssignRoleCommandHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task Handle(AssignRoleCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId);
        if (user == null)
            throw new KeyNotFoundException("User not found");

        // Logic to verify RoleId exists would typically go here (requiring IRoleRepository)
        // For now, we assume RoleId is valid or checked at Database Constraint level

        var membership = new UserAppMembership(request.UserId, request.AppId, request.RoleId);
        
        user.AddMembership(membership);
        
        await _userRepository.UpdateAsync(user);
    }
}
