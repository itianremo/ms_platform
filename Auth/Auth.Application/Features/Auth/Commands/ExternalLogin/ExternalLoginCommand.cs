using MediatR;
using Auth.Application.Common.Interfaces;
using Auth.Domain.Entities;
using Auth.Domain.Repositories;

namespace Auth.Application.Features.Auth.Commands.ExternalLogin;

public record ExternalLoginCommand(string LoginProvider, string ProviderKey, string Email, string? DisplayName, string? Phone) : IRequest<string>;

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
                throw new global::Auth.Domain.Exceptions.UserSoftDeletedException("Account is soft-deleted.");
            
            if (user.Status == GlobalUserStatus.Banned)
                throw new UnauthorizedAccessException("Account is banned.");

            var (token, _) = _tokenService.GenerateAccessToken(user);
            return token;
        }

        // 2. Check if user exists by Email (Link Account)
        if (!string.IsNullOrEmpty(request.Email))
        {
            user = await _userRepository.GetByEmailAsync(request.Email);
        }

        // 3. Check if user exists by Phone (Link Account) if not found by email
        if (user == null && !string.IsNullOrEmpty(request.Phone)) 
        {
            // Assuming GetByEmailOrPhoneAsync checks phone column too
            user = await _userRepository.GetByEmailOrPhoneAsync(request.Phone);
        }

        if (user != null)
        {
            // Link new provider to existing account
            user.AddLogin(new UserLogin(user.Id, request.LoginProvider, request.ProviderKey, request.DisplayName));
            await _userRepository.UpdateAsync(user);
            
            var (token, _) = _tokenService.GenerateAccessToken(user);
            return token;
        }

        // 4. Register new user
        // Generate a random password or null (if allowed, but User entity might enforce it)
        // For now, using a placeholder secure-ish random string since they login via social
        var dummyPasswordHash = "SOCIAL_LOGIN_" + Guid.NewGuid(); 
        
        // Use provided phone or placeholder
        var phone = !string.IsNullOrEmpty(request.Phone) ? request.Phone : "0000000000";

        user = new User(request.Email, phone, dummyPasswordHash); 

        user.AddLogin(new UserLogin(user.Id, request.LoginProvider, request.ProviderKey, request.DisplayName));
        
        await _userRepository.AddAsync(user);

        var (newToken, _) = _tokenService.GenerateAccessToken(user);
        return newToken;
    }
}
