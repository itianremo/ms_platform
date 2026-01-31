using MediatR;
using Auth.Domain.Repositories;
using Microsoft.Extensions.Logging;

namespace Auth.Application.Features.Auth.Commands.LogoutUser;

public class LogoutUserCommandHandler : IRequestHandler<LogoutUserCommand>
{
    private readonly IUserRepository _userRepository;
    private readonly ILogger<LogoutUserCommandHandler> _logger;

    public LogoutUserCommandHandler(IUserRepository userRepository, ILogger<LogoutUserCommandHandler> logger)
    {
        _userRepository = userRepository;
        _logger = logger;
    }

    public async Task Handle(LogoutUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetUserWithSessionsAsync(request.UserId);
        if (user == null)
        {
            _logger.LogWarning("Logout failed: User {UserId} not found.", request.UserId);
            return;
        }

        // Remove the specific session
        user.RemoveSession(request.SessionId);

        // Opportunistic cleanup of expired sessions
        user.ClearExpiredSessions();

        await _userRepository.UpdateAsync(user);
        
        _logger.LogInformation("User {UserId} logged out. Session {SessionId} removed. Expired sessions cleaned.", request.UserId, request.SessionId);
    }
}
