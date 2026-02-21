using MediatR;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Users.Domain.Repositories;

namespace Users.Application.Queries.GetReportReasons;

public class GetReportReasonsQueryHandler : IRequestHandler<GetReportReasonsQuery, List<string>>
{
    private readonly IReportReasonRepository _repository;

    public GetReportReasonsQueryHandler(IReportReasonRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<string>> Handle(GetReportReasonsQuery request, CancellationToken cancellationToken)
    {
        var reasons = await _repository.GetActiveReasonsAsync(request.AppId);
        return reasons.Select(r => r.ReasonText).ToList();
    }
}
