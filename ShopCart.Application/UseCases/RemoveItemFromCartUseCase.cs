using System.Net;
using ShopCart.Application.Interfaces;
using ShopCart.CrossCutting;
using ShopCart.Domain.Repositories;

namespace ShopCart.Application.UseCases;

public class RemoveItemFromCartUseCase(ICartRepository cartRepository, ICartCache cartCache)
{
    public async Task ExecuteAsync(Guid cartId, Guid productId)
    {
        var cart = await cartRepository.GetByIdAsync(cartId) ?? throw new GlobalException("Cart not found.", HttpStatusCode.NotFound);
        
        cart.RemoveItem(productId);

        await cartRepository.UpdateAsync(cartId, cart);
        await cartCache.InvalidateCacheAsync(cartId);
        await cartCache.CreateAsync(cart, TimeSpan.FromMinutes(10));
    }
}