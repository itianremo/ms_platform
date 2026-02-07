using Auth.Domain.Entities;
using Auth.Domain.Repositories;
using Auth.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Shared.Kernel;
using System.Linq.Expressions;
using Microsoft.Extensions.Logging;

namespace Auth.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly AuthDbContext _context;
    private readonly Microsoft.Extensions.Logging.ILogger<UserRepository> _logger;

    public UserRepository(AuthDbContext context, Microsoft.Extensions.Logging.ILogger<UserRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<User?> GetByIdAsync(Guid id)
    {
        return await _context.Users.FindAsync(id);
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        return await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task<User?> GetByEmailOrPhoneAsync(string emailOrPhone)
    {
        return await _context.Users
            .Include(u => u.Memberships)
            .Include(u => u.Sessions)
            .FirstOrDefaultAsync(u => u.Email == emailOrPhone || u.Phone == emailOrPhone);
    }

    public async Task<User?> GetByLoginAsync(string loginProvider, string providerKey)
    {
        return await _context.Users
            .Include(u => u.Logins)
            .FirstOrDefaultAsync(u => u.Logins.Any(l => l.LoginProvider == loginProvider && l.ProviderKey == providerKey));
    }

    public async Task<User?> GetUserWithRolesAsync(Guid userId)
    {
        return await _context.Users
            .Include(u => u.Memberships)
            .ThenInclude(m => m.Role)
            .ThenInclude(r => r.Permissions)
            .FirstOrDefaultAsync(u => u.Id == userId);
    }

    public async Task<List<User>> ListWithRolesAsync()
    {
        return await _context.Users
            .Include(u => u.Memberships)
            .ThenInclude(m => m.Role)
            .Include(u => u.Logins)
            .ToListAsync();
    }

    public async Task<List<User>> ListAsync()
    {
        return await _context.Users.ToListAsync();
    }

    public async Task<List<User>> ListAsync(Expression<Func<User, bool>> predicate)
    {
        return await _context.Users.Where(predicate).ToListAsync();
    }

    public async Task<User> AddAsync(User entity)
    {
        await _context.Users.AddAsync(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task UpdateAsync(User entity)
    {
        _context.Users.Update(entity);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(User entity)
    {
        _context.Users.Remove(entity);
        await _context.SaveChangesAsync();
    }
    public async Task<List<AppRequirement>> GetMemberAppRequirementsAsync(Guid userId)
    {
        try 
        {
            // Cross-Database Query
            var sql = @"
                SELECT m.AppId, CAST(a.VerificationType AS int) AS VerificationType, a.RequiresAdminApproval, CAST(m.Status AS int) AS MembershipStatus
                FROM [AppsDb].[dbo].[Apps] a
                JOIN [AuthDb].[dbo].[UserAppMemberships] m ON a.Id = m.AppId
                WHERE m.UserId = {0}";

            return await _context.Database
                .SqlQueryRaw<AppRequirement>(sql, userId)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch app requirements for User {UserId}", userId);
            return new List<AppRequirement>();
        }
    }

    public async Task<bool> IsAppActiveAsync(Guid appId)
    {
        try
        {
            var sql = "SELECT IsActive FROM [AppsDb].[dbo].[Apps] WHERE Id = {0}";
            // Execute as scalar? EF Core Raw Query for primitive types:
            // .SqlQueryRaw<bool> returns an observable.
            var result = await _context.Database
                .SqlQueryRaw<bool>(sql, appId)
                .ToListAsync();
            
            return result.FirstOrDefault();
        }
        catch (Exception)
        {
            return false;
        }
    }

    public async Task<Role?> GetRoleByNameAsync(Guid appId, string roleName)
    {
        return await _context.Roles.FirstOrDefaultAsync(r => r.AppId == appId && r.Name == roleName);
    }

    public async Task<Role?> GetRoleByIdAsync(Guid roleId)
    {
        return await _context.Roles.FirstOrDefaultAsync(r => r.Id == roleId);
    }

    public async Task<User?> GetUserWithSessionsAsync(Guid userId)
    {
        return await _context.Users
            .Include(u => u.Sessions)
            .FirstOrDefaultAsync(u => u.Id == userId);
    }

    public async Task<User?> GetUserWithLoginsAsync(Guid userId)
    {
        return await _context.Users
            .Include(u => u.Logins)
            .FirstOrDefaultAsync(u => u.Id == userId);
    }

    public async Task AddRoleAsync(Role role)
    {
        await _context.Roles.AddAsync(role);
        await _context.SaveChangesAsync();
    }

    public async Task<AppRequirement?> GetAppVerificationConfigAsync(Guid appId)
    {
        try
        {
            var sql = "SELECT Id as AppId, CAST(VerificationType AS int) AS VerificationType, RequiresAdminApproval, 0 AS MembershipStatus FROM [AppsDb].[dbo].[Apps] WHERE Id = {0}";
            var result = await _context.Database
                .SqlQueryRaw<AppRequirement>(sql, appId)
                .ToListAsync();
            
            return result.FirstOrDefault();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch app verification config for App {AppId}", appId);
            return null;
        }
    }

    public async Task<List<UserSessionDto>> GetSessionsAsync(Guid userId)
    {
        // Cross-Database Join to get AppName
        var sql = @"
            SELECT 
                s.Id, 
                s.AppId, 
                a.Name as AppName, 
                s.IpAddress, 
                s.UserAgent,
                s.DeviceInfo, 
                CAST(0 AS bit) as IsCurrent,
                s.IsRevoked,
                s.CreatedAt, 
                s.ExpiresAt
            FROM [AuthDb].[dbo].[UserSessions] s
            LEFT JOIN [AppsDb].[dbo].[Apps] a ON s.AppId = a.Id
            WHERE s.UserId = {0} AND s.IsRevoked = 0 AND s.ExpiresAt > GETUTCDATE()
            ORDER BY s.CreatedAt DESC";

        return await _context.Database
            .SqlQueryRaw<UserSessionDto>(sql, userId)
            .ToListAsync();
    }

    public async Task<UserSession?> GetSessionByIdAsync(Guid sessionId)
    {
        return await _context.UserSessions.FindAsync(sessionId);
    }

    public async Task<UserSession?> GetSessionByRefreshTokenAsync(string refreshToken)
    {
        return await _context.UserSessions
            .Include(s => s.User) // Include User for token generation
            .ThenInclude(u => u.Memberships)
            .ThenInclude(m => m.Role)
            .ThenInclude(r => r.Permissions)
            .Include(s => s.User.Sessions) // Include Sessions for cleanup
            .FirstOrDefaultAsync(s => s.RefreshToken == refreshToken);
    }

    public async Task UpdateSessionAsync(UserSession session)
    {
        _context.UserSessions.Update(session);
        await _context.SaveChangesAsync();
    }

    public async Task AddSessionAsync(UserSession session)
    {
        await _context.UserSessions.AddAsync(session);
        await _context.SaveChangesAsync();
    }

    public async Task<List<Role>> GetRolesAsync(Guid? appId = null)
    {
        var query = _context.Roles.Include(r => r.Permissions).AsQueryable();
        
        if (appId.HasValue)
        {
            query = query.Where(r => r.AppId == appId.Value);
        }
        
        return await query.ToListAsync();
    }
}

