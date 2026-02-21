using Recommendation.Application.DTOs;

namespace Recommendation.Application.Common.Interfaces;

public interface IUsersService
{
    Task<List<UserProfileDto>> GetProfilesAsync();
}
