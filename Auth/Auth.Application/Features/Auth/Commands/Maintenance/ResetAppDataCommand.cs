using MediatR;
using Auth.Application.Common.Interfaces;

namespace Auth.Application.Features.Auth.Commands.Maintenance;

public record ResetAppDataCommand(Guid AppId) : IRequest;

public class ResetAppDataCommandHandler : IRequestHandler<ResetAppDataCommand>
{
    private readonly IMaintenanceService _maintenanceService;

    public ResetAppDataCommandHandler(IMaintenanceService maintenanceService)
    {
        _maintenanceService = maintenanceService;
    }

    public async Task Handle(ResetAppDataCommand request, CancellationToken cancellationToken)
    {
        await _maintenanceService.ResetAppDataAsync(request.AppId);
    }
}
