using ShopCart.Application.Interfaces;
using ShopCart.Domain.Entities;
using ShopCart.Domain.Repositories;

namespace ShopCart.Application.UseCases;

public class CreateCartUseCase(ICartRepository cartRepository)
{
    public async Task<Guid> ExecuteAsync(string userId)
    {
        ArgumentException.ThrowIfNullOrEmpty(userId);
        
        var cart = new Cart(Guid.Parse(userId), Guid.NewGuid());
        await cartRepository.CreateAsync(cart);
        return cart.Id;
    }
}