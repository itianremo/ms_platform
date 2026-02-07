using MediatR;
using Auth.Domain.Repositories;
using Auth.Application.Common.DTOs;

namespace Auth.Application.Features.Auth.Queries.GetAllUsers;

public class GetAllUsersQueryHandler : IRequestHandler<GetAllUsersQuery, List<UserDto>>
{
    private readonly IUserRepository _repository;

    public GetAllUsersQueryHandler(IUserRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<UserDto>> Handle(GetAllUsersQuery request, CancellationToken cancellationToken)
    {
        var users = await _repository.ListWithRolesAsync();
        
        return users.Select(u => {
            var (firstName, lastName) = DeriveNameFromEmail(u.Email);
            return new UserDto(
                u.Id,
                u.Email,
                u.Phone, 
                firstName, 
                lastName, 
                u.Status == global::Auth.Domain.Entities.GlobalUserStatus.Active,
                (int)u.Status,
                u.Memberships.Select(m => m.Role.Name).Distinct().ToList(),
                u.IsEmailVerified,
                u.IsPhoneVerified,
                u.Memberships.Select(m => new UserAppMembershipDto(m.AppId, m.RoleId, m.Role.Name, (int)m.Status, m.LastLoginUtc)).ToList(),
                u.Logins.Select(l => l.LoginProvider).ToList(),
                u.LastLoginUtc,
                u.LastLoginAppId
            );
        }).ToList();
    }

    private static (string First, string Last) DeriveNameFromEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email)) return ("Unknown", "User");
        
        try 
        {
            var parts = email.Split('@');
            if (parts.Length == 0) return ("Unknown", "User");
            
            var namePart = parts[0];
            var nameSegments = namePart.Split(new[] { '.', '_', '-', '+' }, StringSplitOptions.RemoveEmptyEntries);
            
            var textInfo = System.Globalization.CultureInfo.CurrentCulture.TextInfo;
            
            if (nameSegments.Length == 1)
            {
                return (textInfo.ToTitleCase(nameSegments[0]), "");
            }
            
            var first = textInfo.ToTitleCase(nameSegments[0]);
            var last = textInfo.ToTitleCase(string.Join(" ", nameSegments.Skip(1)));
            
            return (first, last);
        }
        catch
        {
            return ("Unknown", "User");
        }
    }
}
