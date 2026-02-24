using MediatR;
using Users.Domain.Repositories;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Users.Application.Features.Users.Queries.GetProfileIds;

public class GetProfileIdsQueryHandler : IRequestHandler<GetProfileIdsQuery, List<ProfileIdDto>>
{
    private readonly IUserProfileRepository _repository;

    public GetProfileIdsQueryHandler(IUserProfileRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<ProfileIdDto>> Handle(GetProfileIdsQuery request, CancellationToken cancellationToken)
    {
        var profiles = await _repository.ListAsync();
        return profiles.Select(p => new ProfileIdDto(p.UserId, p.AppId, p.RoleId)).ToList();
    }
}
