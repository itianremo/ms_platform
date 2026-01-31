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
        
        return users.Select(u => new UserDto(
            u.Id,
            u.Email,
            u.Phone, 
            "N/A", // FirstName placeholder
            "", // LastName placeholder
            u.Status == Domain.Entities.GlobalUserStatus.Active,
            u.Memberships.Select(m => m.Role.Name).Distinct().ToList(),
            u.IsEmailVerified,
            u.IsPhoneVerified,
            u.Memberships.Select(m => new UserAppMembershipDto(m.AppId, m.RoleId, m.Role.Name, (int)m.Status)).ToList()
        )).ToList();
    }
}
