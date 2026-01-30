using Auth.Domain.Repositories;
using MediatR;
 
// Better add FindSessionAsync to Repo or use direct DbSet if allowed in Handler.
// But we should stick to Repo. User Repository deals with Users. User Aggregate contains Sessions.
// So: Fetch User (Include Sessions), find session, revoke, save.
// OR: Add GetSessionByIdAsync to Repo.

namespace Auth.Application.Features.Auth.Commands.RevokeSession;

public class RevokeSessionCommandHandler : IRequestHandler<RevokeSessionCommand, bool>
{
    private readonly IUserRepository _userRepository;

    // Let's use Repo. But Repo needs a method to fetch specific session or user with specific session.
    
    // We can fetch user with sessions.
    
    public RevokeSessionCommandHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<bool> Handle(RevokeSessionCommand request, CancellationToken cancellationToken)
    {
        // 1. Fetch User (we verify ownership implicitly by fetching user and checking their sessions)
        // Or simpler: We can implement direct session revocation in Repo if we want to be efficient.
        // But Domain Model -> User.RevokeSession(sessionId).
        
        var user = await _userRepository.GetUserWithRolesAsync(request.UserId); // Need Include Sessions? GetUserWithRoles might not include Sessions.
        // We might need a new method or Update GetUserWithRolesAsync to include Sessions?
        // Or just load user and explicit load sessions?
        // Let's add GetUserWithSessionsAsync to Repo?
        // Or just iterate.
        
        // Actually, for performance, we should just fetch the session and check UserId.
        // Repository should have GetSessionAsync(id).
        
        // BUT, I can't change Repo interface too much.
        // Let's try to fetch User and define a new method `RevokeSession` on User.
        // But `Authentication` logic is split.
        
        // Let's assume we can add GetSessionAsync to Repo or just use DbContext if we are lazy (but not recommended).
        // Best approach: Add GetUserSessionsAsync in Repo (already done, but that returns DTOs).
        
        // Let's add `RevokeSessionAsync` to `IUserRepository`? No, that's business logic hiding.
        
        // Okay, let's fetch User with Sessions.
        // I will add `GetUserWithSessionsAsync` to IUserRepository.
        
        // WAIT, I haven't added `GetUserWithSessionsAsync` yet.
        // I'll modify IUserRepository to add it or just use the DbContext directly in Repo logic if I add a method there.
        
        // Simpler: Just Fetch Session by ID using a new method in Repo: `GetSessionByIdAsync`.
        
        var session = await _userRepository.GetSessionByIdAsync(request.SessionId);
        if (session == null || session.UserId != request.UserId) return false;
        
        session.Revoke();
        await _userRepository.UpdateSessionAsync(session); // Need this method too.
        
        return true;
    }
}
