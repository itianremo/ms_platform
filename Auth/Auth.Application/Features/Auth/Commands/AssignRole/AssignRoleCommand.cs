using MediatR;

namespace Auth.Application.Features.Auth.Commands.AssignRole;

public record AssignRoleCommand(Guid UserId, Guid RoleId) : IRequest
{
    public Guid AppId { get; init; }
};
