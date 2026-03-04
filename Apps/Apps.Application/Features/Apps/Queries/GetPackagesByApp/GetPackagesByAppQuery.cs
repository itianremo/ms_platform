using MediatR;
using Apps.Domain.Entities;

namespace Apps.Application.Features.Apps.Queries.GetPackagesByApp;

public record GetPackagesByAppQuery(Guid AppId, string? Country, string DefaultCountry = "US") : IRequest<GetPackagesResponseDto>;

public class GetPackagesResponseDto
{
    public List<SubscriptionPackageDto> Subscriptions { get; set; } = new();
    public List<SubscriptionPackageDto> Coins { get; set; } = new();
}

public class SubscriptionPackageDto
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public decimal Price { get; set; }
    public int Period { get; set; }
    public string Currency { get; set; }
    public int PackageType { get; set; }
    public int CoinsAmount { get; set; }
}
