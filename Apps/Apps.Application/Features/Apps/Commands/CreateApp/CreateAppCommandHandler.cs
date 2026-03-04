using System.Text.Json;
using MediatR;
using Apps.Domain.Entities;
using Apps.Domain.Repositories;
using Apps.Application.Features.Apps.Queries;
using Apps.Application.Features.Apps.Queries.GetAllApps;
using Shared.Kernel;

namespace Apps.Application.Features.Apps.Commands.CreateApp;

public class CreateAppCommandHandler : IRequestHandler<CreateAppCommand, AppDto>
{
    private readonly IAppRepository _appRepository;

    public CreateAppCommandHandler(IAppRepository appRepository)
    {
        _appRepository = appRepository;
    }

    public async Task<AppDto> Handle(CreateAppCommand request, CancellationToken cancellationToken)
    {
        var existingApp = await _appRepository.GetByNameAsync(request.Name);
        if (existingApp != null)
        {
            throw new InvalidOperationException("App with this name already exists.");
        }

        var app = new AppConfig(request.Name, request.Description, request.BaseUrl);
        
        app.UpdateExternalAuthProviders(request.ExternalAuthProvidersJson ?? "[]");
        app.UpdatePrivacyPolicy(request.PrivacyPolicy ?? "");
        app.UpdateTermsAndConditions(request.TermsAndConditions ?? "");

        app.UpdateVerificationType((VerificationType)request.VerificationType);
        app.UpdateRequirements(request.RequiresAdminApproval);
        
        if (!request.IsActive) 
            app.Deactivate();

        var dynamicData = request.DynamicData ?? new Dictionary<string, JsonElement>();

        var sysConfig = request.SysConfig ?? new SysConfigDto();
        dynamicData["sysConfig"] = JsonSerializer.SerializeToElement(sysConfig);

        var notifications = request.Notifications ?? new NotificationsDto();
        dynamicData["notifications"] = JsonSerializer.SerializeToElement(notifications);

        var dynamicJson = JsonSerializer.Serialize(dynamicData);
        app.UpdateDefaultUserProfile(dynamicJson);

        await _appRepository.AddAsync(app);

        return app.ToDto();
    }
}
