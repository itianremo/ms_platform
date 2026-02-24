using MediatR;
using Users.Domain.Enums;

namespace Users.Application.Features.Users.Queries.GetAuthProfile;

public class AuthProfileDto
{
    public Guid RoleId { get; set; }
    public AppUserStatus Status { get; set; }
}

public record GetAuthProfileQuery(Guid UserId, Guid AppId) : IRequest<AuthProfileDto>;
