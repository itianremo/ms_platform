using MediatR;

namespace Auth.Application.Features.Auth.Commands.AssignRole;

public record AssignRoleCommand(Guid UserId, Guid RoleId, Guid AppId) : IRequest;
