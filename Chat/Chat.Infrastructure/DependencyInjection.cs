using Chat.Domain.Repositories;
using Chat.Infrastructure.Persistence;
using Chat.Infrastructure.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace Chat.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddSingleton<ChatDbContext>();
        services.AddScoped<IChatRepository, ChatRepository>();
        services.AddScoped<Chat.Application.Common.Interfaces.ITextModerationService, Chat.Infrastructure.Services.TextModerationService>();
        return services;
    }
}
