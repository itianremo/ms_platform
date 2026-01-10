using Media.Application.Common.Interfaces;
using Media.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Minio;

namespace Media.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<MinioSettings>(configuration.GetSection("MinioSettings"));

        services.AddMinio(configureClient => configureClient
            .WithEndpoint(configuration["MinioSettings:Endpoint"])
            .WithCredentials(configuration["MinioSettings:AccessKey"], configuration["MinioSettings:SecretKey"])
            .Build());

        services.AddScoped<IMediaService, MinioService>();
        services.AddScoped<IImageModerationService, ImageModerationService>();
        services.AddScoped<IImageProcessingService, ImageProcessingService>();

        return services;
    }
}
