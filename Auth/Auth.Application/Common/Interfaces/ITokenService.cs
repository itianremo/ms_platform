using Auth.Domain.Entities;

namespace Auth.Application.Common.Interfaces;

public interface ITokenService
{
    (string AccessToken, int ExpiresIn) GenerateAccessToken(Auth.Domain.Entities.User user, Guid? appId = null, Guid? sessionId = null, bool suppressRoles = false);
    string GenerateRefreshToken();
}
