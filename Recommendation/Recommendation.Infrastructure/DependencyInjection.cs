using Microsoft.Extensions.DependencyInjection;
using Recommendation.Domain.Entities;
using Recommendation.Domain.Entities;
using Recommendation.Infrastructure.Services;
using Recommendation.Application.Common.Interfaces;

namespace Recommendation.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddSingleton<IRecommendationEngine, RecommendationEngine>();
        services.AddSingleton<IRecommendationStore, InMemoryRecommendationStore>();
        services.AddHttpClient<IUsersService, UsersService>();
        return services;
    }
}
