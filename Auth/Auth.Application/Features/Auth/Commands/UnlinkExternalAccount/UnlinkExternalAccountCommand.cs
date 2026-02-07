using MediatR;
using Auth.Application.Common.Interfaces;
using Auth.Domain.Entities;
using Auth.Domain.Repositories;

namespace Auth.Application.Features.Auth.Commands.UnlinkExternalAccount;

public record UnlinkExternalAccountCommand(Guid UserId, string Provider) : IRequest<bool>;

public class UnlinkExternalAccountCommandHandler : IRequestHandler<UnlinkExternalAccountCommand, bool>
{
    private readonly IUserRepository _userRepository;

    public UnlinkExternalAccountCommandHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<bool> Handle(UnlinkExternalAccountCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId);
        if (user == null) return false;

        var login = user.Logins.FirstOrDefault(l => l.LoginProvider == request.Provider);
        if (login == null) return false; // Already unlinked or never linked

        // Prevent unlinking if it's the ONLY way to login and no password?
        // Logic: If PasswordHash is null AND this is the last login?
        // For now, allow unlinking. User might have locked themselves out, but Admin can reset password.

        user.RemoveLogin(login);
        await _userRepository.UpdateAsync(user);
        
        return true;
    }
}
