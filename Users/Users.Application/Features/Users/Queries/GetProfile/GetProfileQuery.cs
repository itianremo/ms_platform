using MediatR;
using Users.Application.DTOs;

namespace Users.Application.Features.Users.Queries.GetProfile;

public record GetProfileQuery(Guid UserId, Guid AppId) : IRequest<UserProfileDto?>;
