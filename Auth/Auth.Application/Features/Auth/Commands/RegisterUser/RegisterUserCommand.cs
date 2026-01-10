using MediatR;

namespace Auth.Application.Features.Auth.Commands.RegisterUser;

public record RegisterUserCommand(string Email, string Phone, string Password, Guid? AppId = null) : IRequest<Guid>;
