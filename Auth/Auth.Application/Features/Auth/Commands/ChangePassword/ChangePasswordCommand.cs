using MediatR;

namespace Auth.Application.Features.Auth.Commands.ChangePassword;

public class ChangePasswordCommand : IRequest<bool>
{
    public string UserId { get; set; } = string.Empty;
    public string OldPassword { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}
