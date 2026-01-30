using MediatR;
using Users.Application.Features.Users.DTOs;

namespace Users.Application.Features.Users.Queries.GetProfiles;

public record GetProfilesQuery(Guid AppId) : IRequest<List<UserProfileDto>>;
