namespace Auth.Application.Common.DTOs;

public record UserDto(Guid Id, string Email, string Phone, string FirstName, string LastName, bool IsActive, int Status, List<string> Roles, bool IsEmailVerified, bool IsPhoneVerified, List<UserAppMembershipDto> Memberships, List<string> LinkedProviders, DateTime? LastLoginUtc, Guid? LastLoginAppId);

public record UserAppMembershipDto(Guid AppId, Guid RoleId, string RoleName, int Status, DateTime? LastLogin);
