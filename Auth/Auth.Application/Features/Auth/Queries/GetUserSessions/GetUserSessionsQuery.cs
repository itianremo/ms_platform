using Auth.Domain.Repositories;
using MediatR;
using System.Collections.Generic;

namespace Auth.Application.Features.Auth.Queries.GetUserSessions;

public record GetUserSessionsQuery(Guid UserId) : IRequest<List<UserSessionDto>>;
