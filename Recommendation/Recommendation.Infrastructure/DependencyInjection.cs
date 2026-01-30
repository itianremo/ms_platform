using Microsoft.Extensions.DependencyInjection;
using Recommendation.Domain.Entities;
using Recommendation.Infrastructure.Services;

namespace Recommendation.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddSingleton<IRecommendationEngine, RecommendationEngine>();
        services.AddSingleton<IRecommendationStore, InMemoryRecommendationStore>();
        return services;
    }
}
