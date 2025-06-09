using ShopCart.Application.Interfaces;
using ShopCart.Domain.Entities;
using ShopCart.Infrastructure.Cache.RedisDataCache;
using ShopCart.Infrastructure.Mappers;

namespace ShopCart.Infrastructure.Cache;

public class CartCache(ICartRedisDataCache redisCache) : ICartCache
{
    public async Task<Cart?> GetAsync(Guid cartId)
    {
        var cart = await redisCache.GetAsync(cartId.ToString());
        
        return cart?.ToDomain();
    }

    public Task CreateAsync(Cart cart, TimeSpan expiration)
    {
        return redisCache.SetAsync(cart.Id.ToString(), cart.ToRedisDatamodel(), expiration);
    }

    public Task InvalidateCacheAsync(Guid cartId)
    {
        return redisCache.RemoveAsync(cartId.ToString());
    }
}