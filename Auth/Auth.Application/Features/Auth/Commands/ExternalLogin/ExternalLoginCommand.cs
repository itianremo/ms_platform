using MediatR;
using Auth.Application.Common.Interfaces;
using Auth.Domain.Entities;
using Auth.Domain.Repositories;

namespace Auth.Application.Features.Auth.Commands.ExternalLogin;

public record ExternalLoginCommand(string LoginProvider, string ProviderKey, string Email, string? DisplayName) : IRequest<string>;

public class ExternalLoginCommandHandler : IRequestHandler<ExternalLoginCommand, string>
{
    private readonly IUserRepository _userRepository;
    private readonly ITokenService _tokenService;

    public ExternalLoginCommandHandler(IUserRepository userRepository, ITokenService tokenService)
    {
        _userRepository = userRepository;
        _tokenService = tokenService;
    }

    public async Task<string> Handle(ExternalLoginCommand request, CancellationToken cancellationToken)
    {
        // 1. Check if user exists by Login Provider
        var user = await _userRepository.GetByLoginAsync(request.LoginProvider, request.ProviderKey);

        if (user != null)
        {
            // Login existing user
            // Optional: Activate if Pending? Social Logins usually verify email.
            if (user.Status == GlobalUserStatus.SoftDeleted)
                throw new Common.Exceptions.UserSoftDeletedException("Account is soft-deleted.");
            
            if (user.Status == GlobalUserStatus.Banned)
                throw new UnauthorizedAccessException("Account is banned.");

            var (token, _) = _tokenService.GenerateAccessToken(user);
            return token;
        }

        // 2. Check if user exists by Email (Link Account)
        user = await _userRepository.GetByEmailAsync(request.Email);
        if (user != null)
        {
            // Link new provider to existing account
            user.AddLogin(new UserLogin(user.Id, request.LoginProvider, request.ProviderKey, request.DisplayName));
            await _userRepository.UpdateAsync(user);
            
            var (token, _) = _tokenService.GenerateAccessToken(user);
            return token;
        }

        // 3. Register new user
        // Generate a random password or null (if allowed, but User entity might enforce it)
        // For now, using a placeholder secure-ish random string since they login via social
        var dummyPasswordHash = "SOCIAL_LOGIN_" + Guid.NewGuid(); 
        
        user = new User(request.Email, "0000000000", dummyPasswordHash); // Phone is required in entity constructor currently
        
        // Auto-activate for social login? Or keep Pending? 
        // Let's Activate since Email is usually verified by provider.
        user.Activate(); 

        user.AddLogin(new UserLogin(user.Id, request.LoginProvider, request.ProviderKey, request.DisplayName));
        
        await _userRepository.AddAsync(user);

        var (newToken, _) = _tokenService.GenerateAccessToken(user);
        return newToken;
    }
}
