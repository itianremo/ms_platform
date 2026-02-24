using Auth.Domain.Entities;
using Shared.Kernel;

namespace Auth.Domain.Repositories;

public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByEmailAsync(string email);
    Task<User?> GetByEmailOrPhoneAsync(string emailOrPhone);
    Task<User?> GetByLoginAsync(string loginProvider, string providerKey);
    Task<User?> GetUserWithRolesAsync(Guid userId);
    Task<List<User>> ListWithRolesAsync();
    Task<List<AppRequirement>> GetMemberAppRequirementsAsync(Guid userId);
    Task<AppRequirement?> GetAppVerificationConfigAsync(Guid appId);
    Task<bool> IsAppActiveAsync(Guid appId);
    Task<Role?> GetRoleByNameAsync(Guid appId, string roleName);
    Task<Role?> GetRoleByIdAsync(Guid roleId);
    Task AddRoleAsync(Role role);
    Task<List<UserSessionDto>> GetSessionsAsync(Guid userId);
    Task<UserSession?> GetSessionByIdAsync(Guid sessionId);
    Task<UserSession?> GetSessionByRefreshTokenAsync(string refreshToken);
    Task UpdateSessionAsync(UserSession session);
    Task AddSessionAsync(UserSession session);
    Task<List<Role>> GetRolesAsync(Guid? appId = null);
    Task<User?> GetUserWithSessionsAsync(Guid userId);
    Task<User?> GetUserWithLoginsAsync(Guid userId);
}

public record AppRequirement(Guid AppId, VerificationType VerificationType, bool RequiresAdminApproval)
{
    public int MembershipStatus { get; set; }
}
public record UserSessionDto(Guid Id, Guid? AppId, string? AppName, string? IpAddress, string? UserAgent, string? DeviceInfo, bool IsCurrent, bool IsRevoked, DateTime CreatedAt, DateTime ExpiresAt);
