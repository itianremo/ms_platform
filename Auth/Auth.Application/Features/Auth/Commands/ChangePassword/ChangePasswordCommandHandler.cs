using Auth.Application.Common.Interfaces;
using Auth.Domain.Repositories;
using MediatR;

namespace Auth.Application.Features.Auth.Commands.ChangePassword;

public class ChangePasswordCommandHandler : IRequestHandler<ChangePasswordCommand, bool>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;

    public ChangePasswordCommandHandler(IUserRepository userRepository, IPasswordHasher passwordHasher)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
    }

    public async Task<bool> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(Guid.Parse(request.UserId));
        if (user == null) return false;

        // Verify Old Password
        if (!_passwordHasher.Verify(request.OldPassword, user.PasswordHash))
        {
            throw new UnauthorizedAccessException("Invalid old password.");
        }

        // Hash New Password
        string newHash = _passwordHasher.Hash(request.NewPassword);
        user.UpdatePassword(newHash);

        await _userRepository.UpdateAsync(user);
        return true;
    }
}
