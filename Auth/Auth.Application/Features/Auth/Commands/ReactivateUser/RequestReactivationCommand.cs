using MediatR;
using Auth.Application.Common.Interfaces;
using Auth.Domain.Repositories;

namespace Auth.Application.Features.Auth.Commands.ReactivateUser;

public record RequestReactivationCommand(string Email) : IRequest;

public class RequestReactivationCommandHandler : IRequestHandler<RequestReactivationCommand>
{
    private readonly IUserRepository _userRepository;
    private readonly IOtpService _otpService;

    public RequestReactivationCommandHandler(IUserRepository userRepository, IOtpService otpService)
    {
        _userRepository = userRepository;
        _otpService = otpService;
    }

    public async Task Handle(RequestReactivationCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email);
        if (user == null) return; // Silent failure for security

        if (user.Status == Domain.Entities.GlobalUserStatus.SoftDeleted)
        {
            await _otpService.SendOtpAsync(user.Email, "Reactivation");
        }
    }
}
