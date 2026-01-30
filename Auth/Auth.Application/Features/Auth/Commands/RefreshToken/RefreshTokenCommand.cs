using MediatR;
using Auth.Application.Features.Auth.DTOs;

namespace Auth.Application.Features.Auth.Commands.RefreshToken;

public record RefreshTokenCommand(string Token, Guid? AppId) : IRequest<AuthResponse>;
