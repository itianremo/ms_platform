using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Recommendation.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // No MediatR purely needed if just a simple service, but good for consistency
        // services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));
        return services;
    }
}
