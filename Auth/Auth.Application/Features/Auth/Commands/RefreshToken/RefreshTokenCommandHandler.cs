using Auth.Application.Common.Interfaces;
using Auth.Application.Features.Auth.DTOs;
using Auth.Domain.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Auth.Application.Features.Auth.Commands.RefreshToken;

public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, AuthResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly ITokenService _tokenService;
    private readonly ILogger<RefreshTokenCommandHandler> _logger;

    public RefreshTokenCommandHandler(IUserRepository userRepository, ITokenService tokenService, ILogger<RefreshTokenCommandHandler> logger)
    {
        _userRepository = userRepository;
        _tokenService = tokenService;
        _logger = logger;
    }

    public async Task<AuthResponse> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var session = await _userRepository.GetSessionByRefreshTokenAsync(request.Token);

        if (session == null)
        {
            _logger.LogWarning("Refresh Token Attempt Failed: Token not found.");
            throw new UnauthorizedAccessException("Invalid Refresh Token");
        }

        if (session.IsRevoked)
        {
            _logger.LogWarning("Refresh Token Attempt Failed: Session Revoked. User: {UserId}", session.UserId);
            throw new UnauthorizedAccessException("Session is revoked");
        } // This is the "Kick Out" mechanism

        if (session.ExpiresAt < DateTime.UtcNow)
        {
             _logger.LogWarning("Refresh Token Attempt Failed: Token Expired. User: {UserId}", session.UserId);
             throw new UnauthorizedAccessException("Refresh Token Expired");
        }
        
        // Generate NEW Access Token
        // Session.User should be populated due to .Include in Repository
        // Generate NEW Access Token
        // Session.User should be populated due to .Include in Repository
        var (accessToken, expiresIn) = _tokenService.GenerateAccessToken(session.User, request.AppId, session.Id);
        
        // Return same refresh token? Or rotate?
        // Using same for now to minimize DB writes on every refresh (performance).
        
        return new AuthResponse(accessToken, request.Token, expiresIn);
    }
}
