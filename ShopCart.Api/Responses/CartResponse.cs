using ShopCart.Domain.Entities;

namespace ShopCart.Api.Responses;

public class CartResponse
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public List<CartItemResponse> Items { get; set; } = [];
    public DiscountResponse? AppliedDiscount { get; set; }
    public decimal Total { get; set; }
}

public class CartItemResponse
{
    public ProductResponse Product { get; set; } = null!;
    public int Quantity { get; set; }
    public decimal Subtotal { get; set; }
}

public class ProductResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
}

public class DiscountResponse
{
    public string Code { get; set; } = string.Empty;
    public decimal Value { get; set; }
    public string Type { get; set; } = string.Empty;
}