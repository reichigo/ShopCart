using ShopCart.Application.Interfaces;
using ShopCart.Domain.Entities;
using ShopCart.Infrastructure.Cache.RedisDataCache;

namespace ShopCart.Infrastructure.Cache;

public class CartCache(IRedisCacheProvider redisCache) : ICartCache
{
    private static string GetKey(Guid cartId) => $"cart-total:{cartId}";

    public Task<Cart?> GetAsync(Guid cartId)
    {
        return redisCache.GetAsync<Cart?>(GetKey(cartId));
    }

    public Task CreateAsync(Cart cart, TimeSpan expiration)
    {
        return redisCache.SetAsync(GetKey(cart.Id), cart, expiration);
    }

    public Task InvalidateCacheAsync(Guid cartId)
    {
        return redisCache.RemoveAsync(GetKey(cartId));
    }
}