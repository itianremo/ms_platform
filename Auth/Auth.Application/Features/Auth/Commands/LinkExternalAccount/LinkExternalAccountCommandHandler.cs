using Auth.Domain.Entities;
using Auth.Domain.Repositories;
using MediatR;
using Shared.Kernel.Interfaces; // If needed

namespace Auth.Application.Features.Auth.Commands.LinkExternalAccount;

public class LinkExternalAccountCommandHandler : IRequestHandler<LinkExternalAccountCommand, bool>
{
    private readonly IUserRepository _userRepository;

    public LinkExternalAccountCommandHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<bool> Handle(LinkExternalAccountCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetUserWithLoginsAsync(request.UserId);
        if (user == null) return false;

        // Check if already linked
        if (user.Logins.Any(l => l.LoginProvider == request.Provider && l.ProviderKey == request.ProviderKey))
        {
            return true; // Already linked
        }

        user.AddLogin(new UserLogin(user.Id, request.Provider, request.ProviderKey, request.DisplayName));
        
        await _userRepository.UpdateAsync(user);
        return true;
    }
}
