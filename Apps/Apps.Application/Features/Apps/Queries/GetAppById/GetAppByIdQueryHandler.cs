using MediatR;
using Apps.Domain.Entities;
using Apps.Application.Features.Apps.Queries.GetAllApps;
using Apps.Domain.Repositories;

namespace Apps.Application.Features.Apps.Queries.GetAppById;

public class GetAppByIdQueryHandler : IRequestHandler<GetAppByIdQuery, AppDto?>
{
    private readonly IAppRepository _repository;

    public GetAppByIdQueryHandler(IAppRepository repository)
    {
        _repository = repository;
    }

    public async Task<AppDto?> Handle(GetAppByIdQuery request, CancellationToken cancellationToken)
    {
        var app = await _repository.GetByIdAsync(request.Id);
        return app?.ToDto();
    }
}
