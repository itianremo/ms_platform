using MediatR;
using Apps.Domain.Entities;

namespace Apps.Application.Features.Apps.Queries.GetPackagesByApp;

public record GetPackagesByAppQuery(Guid AppId) : IRequest<List<SubscriptionPackageDto>>;

public class SubscriptionPackageDto
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public decimal Price { get; set; }
    public int Period { get; set; }
    public string Currency { get; set; }
}
