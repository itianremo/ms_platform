using System.Text.Json;
using Apps.Domain.Entities;
using Apps.Application.Features.Apps.Queries.GetAllApps;

namespace Apps.Application.Features.Apps.Queries;

public static class AppMapper
{
    public static AppDto ToDto(this AppConfig app)
    {
        var dto = new AppDto
        {
            Id = app.Id,
            Name = app.Name,
            Description = app.Description,
            BaseUrl = app.BaseUrl,
            ExternalAuthProvidersJson = app.ExternalAuthProvidersJson,
            PrivacyPolicy = app.PrivacyPolicy,
            TermsAndConditions = app.TermsAndConditions,
            IsActive = app.IsActive,
            VerificationType = (int)app.VerificationType,
            RequiresAdminApproval = app.RequiresAdminApproval,
            DomainEvents = app.DomainEvents.Select(e => e.GetType().Name).ToList()
        };

        if (!string.IsNullOrWhiteSpace(app.DefaultUserProfileJson) && app.DefaultUserProfileJson != "{}")
        {
            try
            {
                // The JSON should be an object at the root now (after we fix the DbInitializer parsing logic). 
                // However, if the seed wrapped it in an array like [{"AppName": { ... }}], the DB initializer 
                // will have extracted the inner object "{ sysConfig:..., notifications:... }".
                var dynamicProps = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(app.DefaultUserProfileJson);
                if (dynamicProps != null)
                {
                    dto.DynamicData = dynamicProps;
                }
            }
            catch
            {
                // Keep quiet on bad json, fallback to empty dynamic dictionary.
            }
        }

        return dto;
    }
}
