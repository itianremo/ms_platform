using MediatR;
using Auth.Application.Common.Interfaces;
using Auth.Domain.Repositories;
using Auth.Domain.Entities;
using Shared.Kernel;

namespace Auth.Application.Features.Auth.Commands.ResetPassword;

public record ResetPasswordCommand(string Email, string Code, string NewPassword) : IRequest<bool>;

public class ResetPasswordCommandHandler : IRequestHandler<ResetPasswordCommand, bool>
{
    private readonly IUserRepository _userRepository;
    private readonly IRepository<UserOtp> _otpRepository;
    private readonly IPasswordHasher _passwordHasher;

    public ResetPasswordCommandHandler(
        IUserRepository userRepository, 
        IRepository<UserOtp> otpRepository,
        IPasswordHasher passwordHasher)
    {
        _userRepository = userRepository;
        _otpRepository = otpRepository;
        _passwordHasher = passwordHasher;
    }

    public async Task<bool> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email);
        if (user == null) throw new global::Auth.Domain.Exceptions.NotFoundException("User not found.");

        var otps = await _otpRepository.ListAsync(o => 
            o.UserId == user.Id && 
            o.Code == request.Code && 
            o.Type == VerificationType.PasswordReset && 
            !o.IsUsed &&
            o.ExpiresAt > DateTime.UtcNow);

        var validOtp = otps.MaxBy(o => o.ExpiresAt); // Get standard latest valid

        if (validOtp == null)
        {
            throw new Exception("Invalid or expired code.");
        }

        // Use OTP
        validOtp.MarkAsUsed();
        await _otpRepository.UpdateAsync(validOtp);

        // Update Password
        string hash = _passwordHasher.Hash(request.NewPassword);
        user.UpdatePassword(hash);
        
        // Unlock Account if locked
        user.ResetAccessFailedCount();

        await _userRepository.UpdateAsync(user);

        return true;
    }
}
