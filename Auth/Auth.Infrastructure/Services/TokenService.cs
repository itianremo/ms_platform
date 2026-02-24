using Auth.Application.Common.Interfaces;
using Auth.Domain.Entities;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Auth.Infrastructure.Services;

public class TokenService : ITokenService
{
    private readonly JwtSettings _jwtSettings;

    public TokenService(IOptions<JwtSettings> jwtOptions)
    {
        _jwtSettings = jwtOptions.Value;
    }

    public (string AccessToken, int ExpiresIn) GenerateAccessToken(User user, Guid? appId = null, Guid? sessionId = null, bool suppressRoles = false, Role? appRole = null, Role? superAdminRole = null)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Secret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),

            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim("phone", user.Phone ?? ""),
            new Claim("email_verified", user.IsEmailVerified.ToString().ToLower()),
            new Claim("phone_verified", user.IsPhoneVerified.ToString().ToLower())
        };

        if (sessionId.HasValue)
        {
            // Standard Claim for Session ID is 'sid'
            claims.Add(new Claim("sid", sessionId.Value.ToString()));
        }

        if (appId.HasValue)
        {
            claims.Add(new Claim("app_id", appId.Value.ToString()));

            // Add Roles and Permissions for this App (ONLY IF NOT SUPPRESSED)
            if (!suppressRoles)
            {
                if (appRole != null)
                {
                    claims.Add(new Claim(ClaimTypes.Role, appRole.Name));
                    
                    if (appRole.Permissions != null)
                    {
                        foreach (var perm in appRole.Permissions)
                        {
                            claims.Add(new Claim("permission", perm.Name));
                        }
                    }
                }
                
                if (superAdminRole != null && appRole?.Name != "SuperAdmin")
                {
                    claims.Add(new Claim(ClaimTypes.Role, "SuperAdmin"));
                    claims.Add(new Claim("permission", "AccessAll"));
                }
            }
        }

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_jwtSettings.ExpiryMinutes),
            signingCredentials: creds
        );

        var tokenHandler = new JwtSecurityTokenHandler();
        var encodedToken = tokenHandler.WriteToken(token);
        
        var expiresInSeconds = _jwtSettings.ExpiryMinutes * 60;
        
        return (encodedToken, expiresInSeconds);
    }

    public string GenerateRefreshToken()
    {
        // Simple random string for now, could be improved with cryptographic RNG
        return Convert.ToBase64String(Guid.NewGuid().ToByteArray());
    }
}
