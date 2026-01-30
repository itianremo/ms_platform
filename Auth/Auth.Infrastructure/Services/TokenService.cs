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

    public (string AccessToken, int ExpiresIn) GenerateAccessToken(User user, Guid? appId = null, Guid? sessionId = null)
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

            // Add Roles and Permissions for this App
            if (user.Memberships != null)
            {
                var appMembership = user.Memberships.FirstOrDefault(m => m.AppId == appId.Value);
                if (appMembership != null && appMembership.Role != null)
                {
                    claims.Add(new Claim(ClaimTypes.Role, appMembership.Role.Name));
                    
                    if (appMembership.Role.Permissions != null)
                    {
                        foreach (var perm in appMembership.Role.Permissions)
                        {
                            claims.Add(new Claim("permission", perm.Name));
                        }
                    }
                }
                
                // Also check for SuperAdmin (Global Access) - User requested removal of global admin column dependency.
                // But we have SuperAdmin role in SystemApp.
                // Does SuperAdmin get access to ALL apps?
                // Usually yes. If user has SuperAdmin role on SystemApp, they might expect to be admin everywhere?
                // User said: "depend on the super admin role with its permission access all"
                // If I am logging into App X, and I am SuperAdmin (on SystemApp), do I get SuperAdmin role in token?
                // Probably yes.
                var systemAppId = Guid.Parse("00000000-0000-0000-0000-000000000001");
                var superAdminMembership = user.Memberships.FirstOrDefault(m => m.AppId == systemAppId && m.Role.Name == "SuperAdmin");
                if (superAdminMembership != null)
                {
                     // Add SuperAdmin role if not already added
                     if (appMembership?.Role?.Name != "SuperAdmin")
                     {
                         claims.Add(new Claim(ClaimTypes.Role, "SuperAdmin"));
                         // AccessAll permission
                         claims.Add(new Claim("permission", "AccessAll"));
                     }
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
        
        // Return seconds or minutes? Usually seconds for OAuth response.
        // ExpiresIn is standardly Seconds.
        var expiresInSeconds = _jwtSettings.ExpiryMinutes * 60;
        
        return (encodedToken, expiresInSeconds);
    }

    public string GenerateRefreshToken()
    {
        // Simple random string for now, could be improved with cryptographic RNG
        return Convert.ToBase64String(Guid.NewGuid().ToByteArray());
    }
}
