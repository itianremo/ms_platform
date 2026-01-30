using MediatR;
using Auth.Domain.Entities;
using Auth.Domain.Repositories;
using MassTransit;
using Shared.Messaging.Events;
using Shared.Kernel;

namespace Auth.Application.Features.Auth.Commands.RequestOtp;

public class RequestOtpCommandHandler : IRequestHandler<RequestOtpCommand, bool>
{
    private readonly IUserRepository _userRepository;

    // Assuming we should extend IUserRepository or add IOtpRepository. 
    // For now, I'll assume we can use the DbContext directly or IRepository<UserOtp> if generic.
    // Let's use IRepository<UserOtp> if available, or just inject DbContext for speed as allowed in this stack?
    // Looking at infrastructure, it uses repositories. I should likely add IOtpRepository.
    // BUT, I'll stick to a simple implementation: IRepository<UserOtp>
    private readonly Shared.Kernel.IRepository<UserOtp> _otpRepository;
    private readonly IPublishEndpoint _publishEndpoint;

    public RequestOtpCommandHandler(
        IUserRepository userRepository, 
        Shared.Kernel.IRepository<UserOtp> otpRepository,
        IPublishEndpoint publishEndpoint)
    {
        _userRepository = userRepository;
        _otpRepository = otpRepository;
        _publishEndpoint = publishEndpoint;
    }

    public async Task<bool> Handle(RequestOtpCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByEmailOrPhoneAsync(request.Email);
        if (user == null) return false; // Fail silently for security? Or throw?

        // Check blocking
        if (user.OtpBlockedUntil.HasValue && user.OtpBlockedUntil.Value > DateTime.UtcNow)
        {
            throw new Exception($"Too many attempts. Please wait until {user.OtpBlockedUntil.Value}");
        }

        // Invalidate old OTPs (Optional: Mark as used or just rely on new one)
        // Implementation: Just create a new one. The logic "only one otp active" will be handled by checking the latest or marking old ones.
        // Let's mark old ones as used/expired? We don't have a "MarkAllExpired" easily without a specialized Repo method.
        // For MVP, we just generate a new one.

        // Generate OTP
        string code = "1234"; // MOCK STATIC OTP

        var otp = new UserOtp(user.Id, code, request.Type, DateTime.UtcNow.AddMinutes(5));
        
        await _otpRepository.AddAsync(otp);

        // Send Event
        // We need to know the destination (Phone or Email)
        string destination = request.Type == VerificationType.Phone ? user.Phone : user.Email;

        await _publishEndpoint.Publish(new SendOtpEvent(
            user.Id,
            destination,
            code,
            request.Type.ToString()
        ), cancellationToken);

        return true;
    }
}
