using MediatR;
using Auth.Application.Common.Interfaces;
using Auth.Domain.Repositories;

namespace Auth.Application.Features.Auth.Commands.ReactivateUser;

public record ConfirmReactivationCommand(string Email, string Otp) : IRequest<bool>;

public class ConfirmReactivationCommandHandler : IRequestHandler<ConfirmReactivationCommand, bool>
{
    private readonly IUserRepository _userRepository;
    private readonly IOtpService _otpService;

    public ConfirmReactivationCommandHandler(IUserRepository userRepository, IOtpService otpService)
    {
        _userRepository = userRepository;
        _otpService = otpService;
    }

    public async Task<bool> Handle(ConfirmReactivationCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email);
        if (user == null) return false;

        if (user.Status != Domain.Entities.GlobalUserStatus.SoftDeleted)
        {
            return false; // Already active or banned
            // Or throw exception? Returning false is safer for now.
        }

        var isValid = await _otpService.VerifyOtpAsync(user.Email, "Reactivation", request.Otp);
        if (!isValid) return false;

        user.Activate();
        await _userRepository.UpdateAsync(user);
        
        return true;
    }
}
