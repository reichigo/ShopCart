using Riok.Mapperly.Abstractions;
using ShopCart.Domain.Entities;
using ShopCart.Infrastructure.Cache.RedisDataCache;
using ShopCart.Infrastructure.Cache.RedisDataCache.RedisDataModel;
using ShopCart.Infrastructure.Repositories.Datasource.SqlServerDataSource.SqlServerDataModel;

namespace ShopCart.Infrastructure.Mappers;

public static class CartMapper
{
    public static Cart ToDomain(this CartSqlServerDatamodel? source)
    {
        if (source is null)
            throw new ArgumentNullException(nameof(source));

        var cart = new Cart(source.UserId, source.Id);

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

    public static CartRedisDataModel ToRedisDatamodel(this Cart cart)
    {
        ArgumentNullException.ThrowIfNull(cart);

        return new CartRedisDataModel(cart.Id, cart.UserId, cart.Items.Select(x => x.ToRedisDatamodel()).ToList(), cart.AppliedDiscount?.ToRedisDatamodel());
    }

    public static CartItemRedisDataModel ToRedisDatamodel(this CartItem item)
    {
        return new CartItemRedisDataModel(item.Product.ToRedisDatamodel(), item.Quantity);
    }

    public static ProductRedisDataModel ToRedisDatamodel(this Product product)
    {
        return new ProductRedisDataModel(product.Id, product.Name, product.Price.Amount);
    }
    
    public static DiscountRedisDataModel ToRedisDatamodel(this Discount discount)
    {
        return new DiscountRedisDataModel(discount.Code, discount.Value, discount.Type);
    }
    
     public static Cart ToDomain(this CartRedisDataModel? source)
    {
        if (source is null)
            throw new ArgumentNullException(nameof(source));

        var cart = new Cart(source.UserId, source.Id);

        foreach (var item in source.Items)
        {
            var product = item.Product.ToDomain();
            cart.AddCartItem(product, item.Quantity);
        }

        if (source.Discount != null)
        {
            var discount = source.Discount.ToDomain();
            cart.ApplyDiscount(discount);
        }

        return cart;
    }

    public static Product ToDomain(this ProductRedisDataModel source)
    {
        return new Product(
            source.Id,
            source.Name,
            new Money(source.Amount)
        );
    }

    public static Discount ToDomain(this DiscountRedisDataModel source)
    {
        return new Discount(
            source.Code,
            source.Value,
            source.Type
        );
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