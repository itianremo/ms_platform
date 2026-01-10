using MediatR;
using Auth.Application.Common.Interfaces;
using Auth.Domain.Repositories;

namespace Auth.Application.Features.Auth.Commands.LoginUser;

public class LoginUserCommandHandler : IRequestHandler<LoginUserCommand, string>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenService _tokenService;

    public LoginUserCommandHandler(
        IUserRepository userRepository, 
        IPasswordHasher passwordHasher,
        ITokenService tokenService)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
    }

    public async Task<string> Handle(LoginUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email);
        
        if (user == null || !_passwordHasher.Verify(request.Password, user.PasswordHash))
        {
            throw new UnauthorizedAccessException("Invalid credentials.");
        }

        if (user.Status == Domain.Entities.GlobalUserStatus.SoftDeleted)
        {
            throw new Common.Exceptions.UserSoftDeletedException("Account is soft-deleted. Reactivation required.");
        }

        if (user.Status == Domain.Entities.GlobalUserStatus.Banned)
        {
            throw new UnauthorizedAccessException("Account is banned.");
        }

        // Generate Token
        var token = _tokenService.GenerateAccessToken(user);
        
        return token;
    }
}
