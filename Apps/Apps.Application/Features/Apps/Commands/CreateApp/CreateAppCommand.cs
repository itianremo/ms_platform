using MediatR;
using System.Text.Json;
using System.Text.Json.Serialization;
using Apps.Application.Features.Apps.Commands;
using Apps.Application.Features.Apps.Queries.GetAllApps;

namespace Apps.Application.Features.Apps.Commands.CreateApp;

public class CreateAppCommand : IRequest<AppDto>
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = string.Empty;
    public string? ExternalAuthProvidersJson { get; set; }
    public string? PrivacyPolicy { get; set; }
    public string? TermsAndConditions { get; set; }
    public bool IsActive { get; set; } = true;
    public int VerificationType { get; set; } = 0;
    public bool RequiresAdminApproval { get; set; } = false;
    public SysConfigDto? SysConfig { get; set; }
    public NotificationsDto? Notifications { get; set; }

    [JsonExtensionData]
    public Dictionary<string, JsonElement>? DynamicData { get; set; }
}
