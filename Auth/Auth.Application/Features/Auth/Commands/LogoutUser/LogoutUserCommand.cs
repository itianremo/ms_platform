using MediatR;

namespace Auth.Application.Features.Auth.Commands.LogoutUser;

public record LogoutUserCommand(Guid UserId, Guid SessionId) : IRequest;
