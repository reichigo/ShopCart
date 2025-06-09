using System.Text.Json.Serialization;

namespace ShopCart.Domain.Entities;

public class CartItem(Product product, int quantity)
{
    public Product Product { get; private set; } = product;
    public int Quantity { get; private set; } = quantity;
    public Money TotalPrice => new Money(Product.Price.Amount * Quantity);

    public void IncreaseQuantity(int quantity)
    {
        Quantity += quantity;
    }
}