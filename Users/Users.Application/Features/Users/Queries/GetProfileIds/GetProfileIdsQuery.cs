using MediatR;
using System.Collections.Generic;

namespace Users.Application.Features.Users.Queries.GetProfileIds;

public record ProfileIdDto(Guid UserId, Guid AppId, Guid RoleId);

public record GetProfileIdsQuery() : IRequest<List<ProfileIdDto>>;
