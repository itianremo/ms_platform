using MediatR;
using Auth.Application.Common.Interfaces;
using Auth.Domain.Repositories;
using Auth.Domain.Entities;
using MassTransit;
using Shared.Messaging.Events;
using Shared.Kernel;

namespace Auth.Application.Features.Auth.Commands.ForgotPassword;

public record ForgotPasswordCommand(string Email) : IRequest;

public class ForgotPasswordCommandHandler : IRequestHandler<ForgotPasswordCommand>
{
    private readonly IUserRepository _userRepository;
    private readonly IRepository<UserOtp> _otpRepository;
    private readonly IPublishEndpoint _publishEndpoint;

    public ForgotPasswordCommandHandler(
        IUserRepository userRepository, 
        IRepository<UserOtp> otpRepository,
        IPublishEndpoint publishEndpoint)
    {
        _userRepository = userRepository;
        _otpRepository = otpRepository;
        _publishEndpoint = publishEndpoint;
    }

    public async Task Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email);
        if (user == null) return; // Silent success

        if (user.Status == GlobalUserStatus.Banned || user.Status == GlobalUserStatus.SoftDeleted)
            return; 

        // Generate Code (6 digits)
        string code = Random.Shared.Next(100000, 999999).ToString();
        
        var otp = new UserOtp(user.Id, code, VerificationType.PasswordReset, DateTime.UtcNow.AddMinutes(15));
        await _otpRepository.AddAsync(otp);

        // Publish Event
        await _publishEndpoint.Publish(new SendOtpEvent(
            user.Id,
            user.Email,
            code,
            VerificationType.PasswordReset.ToString() 
        ), cancellationToken);
    }
}
