using MediatR;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Payments.Domain.Repositories;

namespace Payments.Application.Features.Payments.Queries.GetPlans;

public record PlanDto(System.Guid Id, string Name, decimal Amount, string Currency, string Interval, string ProviderPlanId);

public record GetPlansQuery() : IRequest<List<PlanDto>>;

public class GetPlansQueryHandler : IRequestHandler<GetPlansQuery, List<PlanDto>>
{
    private readonly IPlanRepository _planRepository;

    public GetPlansQueryHandler(IPlanRepository planRepository)
    {
        _planRepository = planRepository;
    }

    public async Task<List<PlanDto>> Handle(GetPlansQuery request, CancellationToken cancellationToken)
    {
        var plans = await _planRepository.ListAsync(p => p.IsActive);

        return plans.Select(p => new PlanDto(p.Id, p.Name, p.Amount, p.Currency, p.Interval, p.ProviderPlanId)).ToList();
    }
}
