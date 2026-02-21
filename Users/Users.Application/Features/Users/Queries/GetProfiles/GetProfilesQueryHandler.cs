using MediatR;
using Users.Domain.Repositories;
using Users.Application.Features.Users.DTOs;

namespace Users.Application.Features.Users.Queries.GetProfiles;

public class GetProfilesQueryHandler : IRequestHandler<GetProfilesQuery, List<UserProfileDto>>
{
    private readonly IUserProfileRepository _repository;

    public GetProfilesQueryHandler(IUserProfileRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<UserProfileDto>> Handle(GetProfilesQuery request, CancellationToken cancellationToken)
    {
        // For simplicity, we just return profiles for the requested AppId.
        // If AppId is not provided (e.g. System Admin?), we might want logic to return all.
        // But the query enforces AppId.
        
        var profiles = await _repository.GetAllProfilesAsync(request.AppId);
        
        return profiles.Select(p => {
            var images = new List<string>();

            // AvatarUrl could be considered an image, but the strict rule is a verified/approved active photo. 
            // The prompt says "at least one admin approved or active or verified photo". 
            // So we will parse the CustomDataJson first.

            if (!string.IsNullOrEmpty(p.CustomDataJson))
            {
                try 
                {
                    using var doc = System.Text.Json.JsonDocument.Parse(p.CustomDataJson);
                    if (doc.RootElement.TryGetProperty("photos", out var photosElement) && photosElement.ValueKind == System.Text.Json.JsonValueKind.Array)
                    {
                        foreach (var photo in photosElement.EnumerateArray())
                        {
                            var isVerified = photo.TryGetProperty("isVerified", out var v) && v.GetBoolean();
                            var isApproved = photo.TryGetProperty("isApproved", out var a) && a.GetBoolean();
                            var isActive = photo.TryGetProperty("isActive", out var i) && i.GetBoolean();

                            if ((isVerified || isApproved) && isActive && photo.TryGetProperty("url", out var urlElement))
                            {
                                var url = urlElement.GetString();
                                if (!string.IsNullOrEmpty(url))
                                    images.Add(url);
                            }
                        }
                    }
                }
                catch { /* Ignore JSON parse errors */ }
            }

            return new UserProfileDto(
                p.UserId,
                GetEffectiveDisplayName(p.DisplayName),
                p.AvatarUrl,
                p.Bio,
                p.DateOfBirth,
                p.Gender,
                p.CustomDataJson,
                images
            );
        })
        .Where(dto => dto.Images != null && dto.Images.Any())
        .ToList();
    }

    private static string GetEffectiveDisplayName(string? displayName)
    {
        if (!string.IsNullOrWhiteSpace(displayName) && displayName != "N/A" && displayName != "n/a" && displayName != "Unknown")
            return displayName;
        
        return "Unknown User";
    }
}
