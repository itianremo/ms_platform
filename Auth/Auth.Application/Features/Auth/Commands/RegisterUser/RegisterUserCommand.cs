using MediatR;
using Shared.Kernel;

namespace Auth.Application.Features.Auth.Commands.RegisterUser;

public record RegisterUserCommand(string Email, string Phone, string Password, Guid? AppId = null, VerificationType VerificationType = VerificationType.Email, bool RequiresAdminApproval = false) : IRequest<Guid>;
