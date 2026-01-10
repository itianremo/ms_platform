using MediatR;
using Users.Domain.Entities;

namespace Users.Application.Features.Users.Queries.GetProfile;

public record GetProfileQuery(Guid UserId, Guid AppId) : IRequest<UserProfile?>;
