using MediatR;

namespace Apps.Application.Features.Apps.Commands.CreateApp;

public record CreateAppCommand(string Name, string Description, string BaseUrl) : IRequest<Guid>;
