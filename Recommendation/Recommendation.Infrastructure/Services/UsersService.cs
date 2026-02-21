using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Recommendation.Application.Common.Interfaces;
using Recommendation.Application.DTOs;

namespace Recommendation.Infrastructure.Services;

public class UsersService : IUsersService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<UsersService> _logger;

    public UsersService(HttpClient httpClient, IConfiguration configuration, ILogger<UsersService> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<List<UserProfileDto>> GetProfilesAsync()
    {
        try
        {
            var baseUrl = _configuration["Services:UsersApi"];
            if (string.IsNullOrEmpty(baseUrl))
            {
                _logger.LogError("UsersApi URL is not configured.");
                return new List<UserProfileDto>();
            }

            // Hardcoded AppId for Wissler for now, or pass it via method
            var wisslerAppId = "22222222-2222-2222-2222-222222222222"; 
            var response = await _httpClient.GetAsync($"{baseUrl}/api/users/profiles?appId={wisslerAppId}");

            if (response.IsSuccessStatusCode)
            {
                var profiles = await response.Content.ReadFromJsonAsync<List<UserProfileDto>>();
                return profiles ?? new List<UserProfileDto>();
            }
            else
            {
                _logger.LogWarning("Failed to fetch profiles from Users API. Status: {StatusCode}", response.StatusCode);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching profiles from Users API");
        }

        return new List<UserProfileDto>();
    }
}
