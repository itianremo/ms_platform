using Microsoft.Extensions.Caching.Distributed;
using Shared.Kernel.Interfaces;
using System.Text.Json;

namespace Shared.Infrastructure.Caching;

public class RedisCacheService : ICacheService
{
    private readonly IDistributedCache _cache;

    public RedisCacheService(IDistributedCache cache)
    {
        _cache = cache;
    }

    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            var cachedResponse = await _cache.GetStringAsync(key, cancellationToken);
            return cachedResponse == null ? default : JsonSerializer.Deserialize<T>(cachedResponse);
        }
        catch
        {
            return default;
        }
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = expiration ?? TimeSpan.FromMinutes(60)
            };
            var response = JsonSerializer.Serialize(value);
            await _cache.SetStringAsync(key, response, options, cancellationToken);
        }
        catch { }
    }

    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        await _cache.RemoveAsync(key, cancellationToken);
    }
}
