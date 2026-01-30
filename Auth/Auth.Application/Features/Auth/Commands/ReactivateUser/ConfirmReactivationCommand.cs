using MediatR;
using Auth.Application.Common.Interfaces;
using Auth.Domain.Repositories;

using Auth.Domain.Entities;

namespace Auth.Application.Features.Auth.Commands.ReactivateUser;

public record ConfirmReactivationCommand(string Email, string Otp) : IRequest<bool>;

public class ConfirmReactivationCommandHandler : IRequestHandler<ConfirmReactivationCommand, bool>
{
    private readonly IUserRepository _userRepository;
    private readonly Shared.Kernel.IRepository<UserOtp> _otpRepository;

    public ConfirmReactivationCommandHandler(IUserRepository userRepository, Shared.Kernel.IRepository<UserOtp> otpRepository)
    {
        _userRepository = userRepository;
        _otpRepository = otpRepository;
    }

    public async Task<bool> Handle(ConfirmReactivationCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email);
        if (user == null) return false;

        if (user.Status != Domain.Entities.GlobalUserStatus.SoftDeleted)
        {
            // If already active, just return true (idempotency) or false?
            // Prompt says: "re-activate the account".
            return false; 
        }

        // Verify OTP (Token)
        var otps = await _otpRepository.ListAsync(o => 
            o.UserId == user.Id && 
            o.Type == Shared.Kernel.VerificationType.Email && 
            !o.IsUsed && 
            o.ExpiresAt > DateTime.UtcNow);
        
        var validOtp = otps
            .OrderByDescending(o => o.ExpiresAt)
            .FirstOrDefault();

        // Exact match check
        if (validOtp == null || validOtp.Code != request.Otp)
        {
            return false;
        }

        validOtp.MarkAsUsed();
        await _otpRepository.UpdateAsync(validOtp);

        // Restore User
        
        // 1. Verify Email (since they clicked the link)
        user.SetEmailVerified(true);

        // 2. Set Status
        user.Activate(); // Sets GlobalUserStatus.Active

        // 3. Downgrade if Phone is missing (Identity Consistency)
        if (!string.IsNullOrEmpty(user.Phone) && !user.IsPhoneVerified)
        {
            user.SetStatus(Domain.Entities.GlobalUserStatus.PendingMobileVerification);
        }

        // Note: App-specific statuses are preserved (Memberships not touched).
        
        await _userRepository.UpdateAsync(user);
        
        return true;
    }
}
