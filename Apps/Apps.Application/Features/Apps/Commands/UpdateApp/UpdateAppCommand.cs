using MediatR;

namespace Apps.Application.Features.Apps.Commands.UpdateApp;

public record UpdateAppCommand(Guid Id, string Name, string Description, string BaseUrl, string? ThemeJson = null) : IRequest<bool>;
