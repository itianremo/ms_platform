namespace Auth.Application.Common.DTOs;

public record RoleDto(Guid Id, string Name, List<string> Permissions);
