using MediatR;

namespace Apps.Application.Features.Apps.Commands.DeactivateApp;

public record DeactivateAppCommand(Guid Id, bool IsActive) : IRequest<bool>;
