using System.Text.Json;
using ShopCart.Application.Interfaces;
using StackExchange.Redis;


namespace ShopCart.Infrastructure.Cache.RedisDataCache;

public class RedisCacheProvider<T>(IConnectionMultiplexer redis) : IRedisCacheProvider<T>
{
    private readonly IDatabase _redis = redis.GetDatabase();

    public async Task<T?> GetAsync(string key)
    {
        var data = await _redis.StringGetAsync(key);
        return data.HasValue ? JsonSerializer.Deserialize<T>(data!) : default;
    }

    public Task SetAsync(string key, T value, TimeSpan expiration)
    {
        var data = JsonSerializer.Serialize(value);
        return _redis.StringSetAsync(key, data, expiration);
    }

    public Task RemoveAsync(string key)
    {
        return _redis.KeyDeleteAsync(key);
    }
}