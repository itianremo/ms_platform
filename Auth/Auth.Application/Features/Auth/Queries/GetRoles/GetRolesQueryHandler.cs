using MediatR;
using Auth.Domain.Repositories;
using Auth.Application.Common.DTOs;

namespace Auth.Application.Features.Auth.Queries.GetRoles;

public class GetRolesQueryHandler : IRequestHandler<GetRolesQuery, List<RoleDto>>
{
    private readonly IUserRepository _repository;

    public GetRolesQueryHandler(IUserRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<RoleDto>> Handle(GetRolesQuery request, CancellationToken cancellationToken)
    {
        var roles = await _repository.GetRolesAsync(request.AppId);
        
        return roles.Select(r => new RoleDto(
            r.Id,
            r.Name,
            r.Permissions.Select(p => p.Name).ToList()
        )).ToList();
    }
}
