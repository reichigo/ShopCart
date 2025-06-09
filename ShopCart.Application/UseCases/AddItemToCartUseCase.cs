using System.Net;
using ShopCart.Application.Interfaces;
using ShopCart.CrossCutting;
using ShopCart.Domain.Repositories;

namespace ShopCart.Application.UseCases;

public class AddItemToCartUseCase(
    ICartRepository cartRepository,
    IProductRepository productRepository,
    ICartCache cartCache)
{
    public async Task ExecuteAsync(Guid cartId, Guid productId, int quantity)
    {
        var cart = await cartRepository.GetByIdAsync(cartId);

        if(quantity <= 0) throw new ArgumentException("Quantity must be greater than 0");
        
        if (cart == null)
        {
            await cartCache.InvalidateCacheAsync(cart.Id);
            throw new GlobalException("Cart not found.", HttpStatusCode.NotFound);
        }

        if (cart.Items.Exists(x => x.Product.Id == productId))
        {
            cart.UpdateQuantity(productId, quantity);
        }
        else
        {
            var product = await productRepository.GetByIdAsync(productId);

            if (product == null)
            {
                throw new GlobalException("Product not found.", HttpStatusCode.NotFound);
            }
            
            cart.AddCartItem(product, quantity);
        }
        
        await cartRepository.UpdateAsync(cartId, cart);

        await cartCache.InvalidateCacheAsync(cart.Id);

        await cartCache.CreateAsync(cart, TimeSpan.FromMinutes(10));
    }
}