using MediatR;
using Auth.Application.Common.Interfaces;
using Auth.Domain.Repositories;

using Auth.Domain.Entities;

namespace Auth.Application.Features.Auth.Commands.ReactivateUser;

public record RequestReactivationCommand(string Email) : IRequest;

public class RequestReactivationCommandHandler : IRequestHandler<RequestReactivationCommand>
{
    private readonly IUserRepository _userRepository;
    private readonly Shared.Kernel.IRepository<UserOtp> _otpRepository;
    private readonly MassTransit.IPublishEndpoint _publishEndpoint;

    public RequestReactivationCommandHandler(
        IUserRepository userRepository, 
        Shared.Kernel.IRepository<UserOtp> otpRepository,
        MassTransit.IPublishEndpoint publishEndpoint)
    {
        _userRepository = userRepository;
        _otpRepository = otpRepository;
        _publishEndpoint = publishEndpoint;
    }

    public async Task Handle(RequestReactivationCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email);
        if (user == null) return; // Silent failure for security

        if (user.Status == Domain.Entities.GlobalUserStatus.SoftDeleted)
        {
            // Generate Reactivation Token (Link)
            string token = Guid.NewGuid().ToString(); 
            var otp = new UserOtp(user.Id, token, Shared.Kernel.VerificationType.Email, DateTime.UtcNow.AddHours(24));
            await _otpRepository.AddAsync(otp);

            // Send Event - Notification Service handles sending the email with link
            // Using "Reactivation" as purpose/type string if supported, or generic Email
            await _publishEndpoint.Publish(new Shared.Messaging.Events.SendOtpEvent(
                user.Id,
                user.Email,
                token,
                "Reactivation" 
            ), cancellationToken);
        }
    }
}
