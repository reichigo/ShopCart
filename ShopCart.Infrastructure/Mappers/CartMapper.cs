using Riok.Mapperly.Abstractions;
using ShopCart.Domain.Entities;
using ShopCart.Infrastructure.Repositories.Datasource.SqlServerDataSource.SqlServerDataModel;

namespace ShopCart.Infrastructure.Mappers;

public static class CartMapper
{
    public static Cart ToDomain(this CartSqlServerDatamodel? source)
    {
        if (source is null)
            throw new ArgumentNullException(nameof(source));

        var cart = new Cart(source.Id);

        foreach (var item in source.Items)
        {
            var product = new Product(
                item.Product.Id,
                item.Product.Name,
                new Money(item.Product.Price)
            );

            var cartItem = new CartItem(product, item.Quantity);

            cart.AddCartItem(product, item.Quantity); 
        }

        return cart;
    }
    
    public static CartSqlServerDatamodel ToSqlDatamodel(this Cart cart)
    {
        if (cart == null)
            throw new ArgumentNullException(nameof(cart));

        var cartDatamodel = new CartSqlServerDatamodel
        {
            Id = cart.Id,
            UserId = cart.UserId,
            Items = new List<CartItemSqlServerDatamodel>()
        };

        foreach (var item in cart.Items)
        {
            cartDatamodel.Items.Add(new CartItemSqlServerDatamodel
            {
                CartId = cart.Id,
                ProductId = item.Product.Id,
                Quantity = item.Quantity
            });
        }

        if (cart.AppliedDiscount != null)
        {
            cartDatamodel.DiscountCode = cart.AppliedDiscount.Code;
        }

        return cartDatamodel;
    }
}