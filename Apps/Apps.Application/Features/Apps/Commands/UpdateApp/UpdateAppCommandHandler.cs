using System.Text.Json;
using MediatR;
using Apps.Domain.Repositories;
using Apps.Application.Features.Apps.Queries;
using Apps.Application.Features.Apps.Queries.GetAllApps;
using Shared.Kernel;

namespace Apps.Application.Features.Apps.Commands.UpdateApp;

public class UpdateAppCommandHandler : IRequestHandler<UpdateAppCommand, AppDto?>
{
    private readonly IAppRepository _repository;

    public UpdateAppCommandHandler(IAppRepository repository)
    {
        _repository = repository;
    }

    public async Task<AppDto?> Handle(UpdateAppCommand request, CancellationToken cancellationToken)
    {
        var app = await _repository.GetByIdAsync(request.Id);
        if (app == null) return null;

        app.UpdateDetails(request.Name, request.Description, request.BaseUrl);
        
        app.UpdateExternalAuthProviders(request.ExternalAuthProvidersJson ?? "[]");
        app.UpdatePrivacyPolicy(request.PrivacyPolicy ?? "");
        app.UpdateTermsAndConditions(request.TermsAndConditions ?? "");

        app.UpdateVerificationType((VerificationType)request.VerificationType);
        app.UpdateRequirements(request.RequiresAdminApproval);
        
        if (request.IsActive) 
            app.Activate(); 
        else 
            app.Deactivate();

        var dynamicData = request.DynamicData ?? new Dictionary<string, JsonElement>();

        var sysConfig = request.SysConfig ?? new SysConfigDto();
        dynamicData["sysConfig"] = JsonSerializer.SerializeToElement(sysConfig);

        var notifications = request.Notifications ?? new NotificationsDto();
        dynamicData["notifications"] = JsonSerializer.SerializeToElement(notifications);

        var dynamicJson = JsonSerializer.Serialize(dynamicData);
        app.UpdateDefaultUserProfile(dynamicJson);

        await _repository.UpdateAsync(app);
        
        return app.ToDto();
    }
}
