using ShopCart.Api.Responses;
using ShopCart.Domain.Entities;

namespace ShopCart.Api.Mappers;

public static class CartResponseMapper
{
    public static CartResponse ToResponse(this Cart cart)
    {
        var response = new CartResponse
        {
            Id = cart.Id,
            UserId = cart.UserId,
            Items = cart.Items.Select(item => new CartItemResponse
            {
                Product = new ProductResponse
                {
                    Id = item.Product.Id,
                    Name = item.Product.Name,
                    Price = item.Product.Price.Amount
                },
                Quantity = item.Quantity,
                Subtotal = item.Product.Price.Amount * item.Quantity
            }).ToList(),
            Total = cart.CalculateTotal().Amount
        };

        if (cart.AppliedDiscount != null)
        {
            response.AppliedDiscount = new DiscountResponse
            {
                Code = cart.AppliedDiscount.Code,
                Value = cart.AppliedDiscount.Value,
                Type = cart.AppliedDiscount.Type.ToString()
            };
        }

        return response;
    }
}
