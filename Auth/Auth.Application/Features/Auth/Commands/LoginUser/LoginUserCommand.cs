using Auth.Application.Features.Auth.DTOs;
using MediatR;

namespace Auth.Application.Features.Auth.Commands.LoginUser;

public record LoginUserCommand(string Email, string Password, Guid? AppId = null, string? IpAddress = null, string? UserAgent = null) : IRequest<AuthResponse>;
