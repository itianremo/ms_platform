namespace Auth.Application.Common.DTOs;

public record UserDto(Guid Id, string Email, string Phone, string FirstName, string LastName, bool IsActive, List<string> Roles, bool IsEmailVerified, bool IsPhoneVerified, List<UserAppMembershipDto> Memberships);

public record UserAppMembershipDto(Guid AppId, Guid RoleId, string RoleName, int Status);
