using MediatR;
using Apps.Domain.Repositories;

namespace Apps.Application.Features.Apps.Queries.GetPackagesByApp;

public class GetPackagesByAppQueryHandler : IRequestHandler<GetPackagesByAppQuery, List<SubscriptionPackageDto>>
{
    private readonly ISubscriptionPackageRepository _repository;

    public GetPackagesByAppQueryHandler(ISubscriptionPackageRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<SubscriptionPackageDto>> Handle(GetPackagesByAppQuery request, CancellationToken cancellationToken)
    {
        var packages = await _repository.GetByAppIdAsync(request.AppId);
        
        return packages.Select(p => {
            decimal price = 0;
            string currency = "USD";
            
            if (!string.IsNullOrEmpty(p.LocalizedPricingJson))
            {
                try
                {
                    var pricingData = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, object>>>(p.LocalizedPricingJson) ?? new();
                    var countryKey = request.Country?.ToUpper() ?? "DEFAULT";
                    
                    if (!pricingData.ContainsKey(countryKey))
                        countryKey = "DEFAULT";
                        
                    if (pricingData.ContainsKey(countryKey))
                    {
                        var regionInfo = pricingData[countryKey];
                        if (regionInfo.ContainsKey("Price")) price = Convert.ToDecimal(regionInfo["Price"].ToString());
                        if (regionInfo.ContainsKey("Currency")) currency = regionInfo["Currency"].ToString()!;
                    }
                }
                catch { /* Ignore parsing errors, assume 0/USD defaults */ }
            }

            return new SubscriptionPackageDto
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                Price = price,
                Period = (int)p.Period,
                Currency = currency,
                PackageType = (int)p.Type,
                CoinsAmount = p.CoinsAmount
            };
        }).ToList();
    }
}
