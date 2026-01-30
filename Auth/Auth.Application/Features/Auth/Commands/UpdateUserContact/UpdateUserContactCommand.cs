using MediatR;
using Auth.Application.Common.DTOs;

namespace Auth.Application.Features.Auth.Commands.UpdateUserContact;

public record UpdateUserContactCommand(Guid UserId, string? NewEmail, string? NewPhone) : IRequest<bool>;
