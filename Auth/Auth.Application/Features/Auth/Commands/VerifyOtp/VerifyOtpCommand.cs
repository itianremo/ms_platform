using MediatR;
using Shared.Kernel;

namespace Auth.Application.Features.Auth.Commands.VerifyOtp;

public record VerifyOtpCommand(string Email, string Code, VerificationType Type) : IRequest<global::Auth.Application.Features.Auth.DTOs.AuthResponse>;
