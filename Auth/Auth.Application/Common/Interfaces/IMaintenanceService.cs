namespace Auth.Application.Common.Interfaces;

public interface IMaintenanceService
{
    Task ResetAppDataAsync(Guid appId);
}
