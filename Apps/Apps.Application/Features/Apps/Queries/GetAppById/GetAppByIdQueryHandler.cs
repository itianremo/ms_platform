using MediatR;
using Apps.Domain.Entities;
using Apps.Domain.Repositories;

namespace Apps.Application.Features.Apps.Queries.GetAppById;

public class GetAppByIdQueryHandler : IRequestHandler<GetAppByIdQuery, AppConfig?>
{
    private readonly IAppRepository _repository;

    public GetAppByIdQueryHandler(IAppRepository repository)
    {
        _repository = repository;
    }

    public async Task<AppConfig?> Handle(GetAppByIdQuery request, CancellationToken cancellationToken)
    {
        return await _repository.GetByIdAsync(request.Id);
    }
}
