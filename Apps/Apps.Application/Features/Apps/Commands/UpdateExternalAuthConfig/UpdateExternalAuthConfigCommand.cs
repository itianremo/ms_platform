using MediatR;

namespace Apps.Application.Features.Apps.Commands.UpdateExternalAuthConfig;

public record UpdateExternalAuthConfigCommand(Guid Id, string ExternalLoginsJson) : IRequest<bool>;
