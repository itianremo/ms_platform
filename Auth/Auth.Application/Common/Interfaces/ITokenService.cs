using Auth.Domain.Entities;

namespace Auth.Application.Common.Interfaces;

public interface ITokenService
{
    (string AccessToken, int ExpiresIn) GenerateAccessToken(User user, Guid? appId = null, Guid? sessionId = null);
    string GenerateRefreshToken();
}
