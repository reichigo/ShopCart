using System.Net;
using ShopCart.Application.Interfaces;
using ShopCart.CrossCutting;
using ShopCart.Domain.Repositories;

namespace ShopCart.Application.UseCases;

public class ApplyDiscountUseCase
{
    private readonly ICartRepository _cartRepository;
    private readonly IDiscountRepository _discountRepository;
    private readonly ICartCache _cartCache;

    public ApplyDiscountUseCase(ICartRepository cartRepository, IDiscountRepository discountRepository, ICartCache cartCache)
    {
        _cartRepository = cartRepository;
        _discountRepository = discountRepository;
        _cartCache = cartCache;
    }

    public async Task ExecuteAsync(Guid cartId, string discountCode)
    {
        await _cartCache.InvalidateCacheAsync(cartId);
        
        var cart = await _cartRepository.GetByIdAsync(cartId) ?? throw new GlobalException("Cart not found.", HttpStatusCode.NotFound);
        var discount = await _discountRepository.GetByCodeAsync(discountCode) ?? throw new GlobalException("Discount not found.", HttpStatusCode.NotFound);

        cart.ApplyDiscount(discount);

        await _cartRepository.UpdateAsync(cartId, cart);
        await _cartCache.CreateAsync(cart, TimeSpan.FromMinutes(10));
    }
}