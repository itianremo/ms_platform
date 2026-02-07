using MediatR;
using Auth.Application.Common.Interfaces;
using Auth.Domain.Repositories;
using Auth.Domain.Entities;
using Shared.Kernel;
using Microsoft.Extensions.Logging;

namespace Auth.Application.Features.Auth.Commands.ReactivateUser;

public record VerifyReactivationCommand(string Email, string Code) : IRequest<bool>;

public class VerifyReactivationCommandHandler : IRequestHandler<VerifyReactivationCommand, bool>
{
    private readonly IUserRepository _userRepository;
    private readonly IRepository<UserOtp> _otpRepository;
    private readonly ILogger<VerifyReactivationCommandHandler> _logger;

    public VerifyReactivationCommandHandler(
        IUserRepository userRepository, 
        IRepository<UserOtp> otpRepository,
        ILogger<VerifyReactivationCommandHandler> logger)
    {
        _userRepository = userRepository;
        _otpRepository = otpRepository;
        _logger = logger;
    }

    public async Task<bool> Handle(VerifyReactivationCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email);
        if (user == null) throw new global::Auth.Domain.Exceptions.NotFoundException("User not found.");

        var otps = await _otpRepository.ListAsync(o => 
            o.UserId == user.Id && 
            o.Type == VerificationType.Reactivation && 
            !o.IsUsed && 
            o.ExpiresAt > DateTime.UtcNow);
        
        var validOtp = otps.MaxBy(o => o.ExpiresAt);

        if (validOtp == null || validOtp.Code != request.Code)
        {
            throw new UnauthorizedAccessException("Invalid or expired OTP.");
        }

        validOtp.MarkAsUsed();
        await _otpRepository.UpdateAsync(validOtp);

        // Reactivate
        user.SetEmailVerified(true);
        user.SetStatus(GlobalUserStatus.Active);
        user.ResetAccessFailedCount();
        
        // Ensure no other blocks prevent login
        // If Phone was unverified, global status is Active, but Policy might partial block?
        // Policy checks: "If Active && !EmailVerified -> downgrade".
        // Here we verified Email.
        // If Phone is missing, Policy might not downgrade?
        // Original VerifyOtp logic checked Phone.
        // For Reactivation, if they verified Email, we restore to Active.
        
        await _userRepository.UpdateAsync(user);

        return true;
    }
}
