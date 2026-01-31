using Auth.Domain.Repositories;
using MediatR;
using Shared.Kernel.Interfaces;

namespace Auth.Application.Features.Auth.Commands.RevokeSession;

public class RevokeSessionCommandHandler : IRequestHandler<RevokeSessionCommand, bool>
{
    private readonly IUserRepository _userRepository;
    private readonly ICacheService _cache;

    public RevokeSessionCommandHandler(IUserRepository userRepository, ICacheService cache)
    {
        _userRepository = userRepository;
        _cache = cache;
    }

    public async Task<bool> Handle(RevokeSessionCommand request, CancellationToken cancellationToken)
    {
        var session = await _userRepository.GetSessionByIdAsync(request.SessionId);
        if (session == null || session.UserId != request.UserId) return false;
        
        session.Revoke();
        await _userRepository.UpdateSessionAsync(session);

        // Blacklist Session in Redis for duration of Access Token (e.g. 60 mins)
        // We use a safe margin.
        await _cache.SetAsync($"blacklist:session:{request.SessionId}", true, TimeSpan.FromMinutes(65), cancellationToken);
        
        return true;
    }
}
