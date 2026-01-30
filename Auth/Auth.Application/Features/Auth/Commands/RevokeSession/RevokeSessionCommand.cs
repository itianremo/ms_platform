using MediatR;

namespace Auth.Application.Features.Auth.Commands.RevokeSession;

public record RevokeSessionCommand(Guid UserId, Guid SessionId) : IRequest<bool>;
