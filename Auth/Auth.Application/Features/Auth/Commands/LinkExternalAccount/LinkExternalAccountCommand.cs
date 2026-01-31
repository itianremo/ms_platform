using MediatR;
using Shared.Kernel; // For Result<T> if used, or just void/Unit

namespace Auth.Application.Features.Auth.Commands.LinkExternalAccount;

public record LinkExternalAccountCommand(
    Guid UserId,
    string Provider,
    string ProviderKey,
    string DisplayName
) : IRequest<bool>;
