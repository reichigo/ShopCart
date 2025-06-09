
using ShopCart.Infrastructure.Cache.RedisDataCache.RedisDataModel;
using StackExchange.Redis;

namespace ShopCart.Infrastructure.Cache.RedisDataCache;

public class CartRedisDataCache(IConnectionMultiplexer redis)
    : RedisCacheProvider<CartRedisDataModel>(redis), ICartRedisDataCache
{
    private static string GetKey(Guid cartId) => $"cart:{cartId}";

    public async Task<CartRedisDataModel?> GetAsync(Guid cartId)
    {
        return await GetAsync(GetKey(cartId));
    }

    public Task SetAsync(CartRedisDataModel cartRedisDataModel, TimeSpan expiration)
    {
        return SetAsync(GetKey(cartRedisDataModel.Id), cartRedisDataModel, expiration);
    }

    public Task InvalidateAsync(Guid cartId)
    {
        return RemoveAsync(GetKey(cartId));
    }
}