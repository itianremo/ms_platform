using MediatR;
using Shared.Kernel;

namespace Auth.Application.Features.Auth.Commands.RequestOtp;

public record RequestOtpCommand(string Email, VerificationType Type) : IRequest<bool>;
