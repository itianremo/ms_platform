using Auth.Application.Features.Auth.DTOs;
using MediatR;

namespace Auth.Application.Features.Auth.Commands.LoginUser;

public record LoginUserCommand(string Email, string Password) : IRequest<AuthResponse>
{
    public Guid? AppId { get; init; }
    public string? IpAddress { get; init; }
    public string? UserAgent { get; init; }
};
