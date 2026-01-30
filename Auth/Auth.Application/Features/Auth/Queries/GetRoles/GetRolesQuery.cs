using MediatR;
using Auth.Application.Common.DTOs;

namespace Auth.Application.Features.Auth.Queries.GetRoles;

public record GetRolesQuery(Guid? AppId) : IRequest<List<RoleDto>>;
