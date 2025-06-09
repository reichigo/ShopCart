using ShopCart.Application.Interfaces;
using StackExchange.Redis;

namespace ShopCart.Infrastructure.Cache.RedisDataCache;

public class CartRedisDataCache(IConnectionMultiplexer redis) 
    : RedisCacheProvider(redis), ICartRedisDataCache
{
    private static string GetKey(Guid cartId) => $"cart-total:{cartId}";

    public Task<decimal?> GetTotalAsync(Guid cartId)
    {
        return GetAsync<decimal?>(GetKey(cartId));
    }

    public Task SetTotalAsync(Guid cartId, decimal total, TimeSpan expiration)
    {
        return SetAsync(GetKey(cartId), total, expiration);
    }

    public Task InvalidateTotalAsync(Guid cartId)
    {
        return RemoveAsync(GetKey(cartId));
    }
}