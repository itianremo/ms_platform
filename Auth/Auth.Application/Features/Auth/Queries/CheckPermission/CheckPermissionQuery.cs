using MediatR;

namespace Auth.Application.Features.Auth.Queries.CheckPermission;

public record CheckPermissionQuery(Guid UserId, Guid AppId, string PermissionName) : IRequest<bool>;
