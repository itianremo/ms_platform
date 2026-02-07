using MediatR;
using Auth.Application.Common.Interfaces;
using Auth.Domain.Repositories;
using Auth.Domain.Entities;
using Shared.Kernel;
using Shared.Messaging.Events;
using MassTransit;

namespace Auth.Application.Features.Auth.Commands.ReactivateUser;

public record InitiateReactivationCommand(string OldEmail, string NewEmail) : IRequest;

public class InitiateReactivationCommandHandler : IRequestHandler<InitiateReactivationCommand>
{
    private readonly IUserRepository _userRepository;
    private readonly IRepository<UserOtp> _otpRepository;
    private readonly IPublishEndpoint _publishEndpoint;

    public InitiateReactivationCommandHandler(
        IUserRepository userRepository, 
        IRepository<UserOtp> otpRepository,
        IPublishEndpoint publishEndpoint)
    {
        _userRepository = userRepository;
        _otpRepository = otpRepository;
        _publishEndpoint = publishEndpoint;
    }

    public async Task Handle(InitiateReactivationCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByEmailAsync(request.OldEmail);
        
        // Security: If user not found, do nothing (or generic error if needed, but existing flow implies we might redirect to masking)
        // If the user reached the "Complete your email" screen, they likely tried logging in and got the "SoftDeleted" error.
        // So the user exists.
        if (user == null) return; 

        if (user.Status != GlobalUserStatus.SoftDeleted)
            throw new Exception("Account is not in a state that requires reactivation.");

        // Check uniqueness of New Email if it's different
        if (!string.Equals(request.OldEmail, request.NewEmail, StringComparison.OrdinalIgnoreCase))
        {
            var existingUser = await _userRepository.GetByEmailAsync(request.NewEmail);
            if (existingUser != null)
            {
                throw new global::Auth.Domain.Exceptions.ConflictException("Email already in use.");
            }
        }

        // Update Email and Status
        user.UpdateEmail(request.NewEmail);
        user.SetStatus(GlobalUserStatus.PendingEmailVerification);

        // Generate OTP
        string code = Random.Shared.Next(100000, 999999).ToString();
        var otp = new UserOtp(user.Id, code, VerificationType.Reactivation, DateTime.UtcNow.AddMinutes(15));
        
        await _otpRepository.AddAsync(otp);
        await _userRepository.UpdateAsync(user);

        // Publish Event
        await _publishEndpoint.Publish(new SendOtpEvent(
            user.Id,
            user.Email,
            code,
            VerificationType.Reactivation.ToString()
        ), cancellationToken);
    }
}
