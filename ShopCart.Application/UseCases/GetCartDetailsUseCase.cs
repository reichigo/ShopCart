using System.Net;
using ShopCart.Application.Interfaces;
using ShopCart.CrossCutting;
using ShopCart.Domain.Entities;
using ShopCart.Domain.Repositories;

namespace ShopCart.Application.UseCases;

public class GetCartDetailsUseCase(ICartRepository cartRepository, ICartCache cartCache)
{
    public async Task<Cart> ExecuteAsync(Guid cartId)
    {
        var cachedCart = await cartCache.GetAsync(cartId);
        
        if (cachedCart is not null) return cachedCart;
        
        var cart = await cartRepository.GetByIdAsync(cartId) ?? throw new GlobalException("Product not found.", HttpStatusCode.NotFound);
        
        return cart;
    }
}