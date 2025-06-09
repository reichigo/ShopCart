using ShopCart.Domain.Entities;

namespace ShopCart.Application.Interfaces;

public interface ICartCache
{
    Task<Cart?> GetAsync(Guid cartId);
    Task CreateAsync(Cart cart, TimeSpan expiration);
    Task InvalidateCacheAsync(Guid cartId);
}