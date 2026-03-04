using System.Text.Json;
using System.Text.Json.Serialization;
using Shared.Kernel; // For IDomainEvent if needed

namespace Apps.Application.Features.Apps.Queries.GetAllApps;

public class AppDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = string.Empty;
    // We intentionally map ExternalAuthProvidersJson, PrivacyPolicy, TermsAndConditions to strings 
    // or as actual objects if preferred. We'll leave them as is or let Json handle it.
    public string ExternalAuthProvidersJson { get; set; } = "[]";
    public string PrivacyPolicy { get; set; } = string.Empty;
    public string TermsAndConditions { get; set; } = string.Empty;
    
    public bool IsActive { get; set; }
    public int VerificationType { get; set; }
    public bool RequiresAdminApproval { get; set; }

    [JsonExtensionData]
    public Dictionary<string, JsonElement> DynamicData { get; set; } = new();

    public List<string> DomainEvents { get; set; } = new(); // Mock representation for exact response
}
