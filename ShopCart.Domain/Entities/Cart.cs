using System.Text.Json.Serialization;

namespace ShopCart.Domain.Entities;

public class Cart(Guid userId, Guid id)
{
    public Guid Id { get; private set; } = id;
    public Guid UserId { get; private set; } = userId;
    public List<CartItem> Items { get; private set; } = [];
    public Discount? AppliedDiscount { get; private set; }

  
    public void AddCartItem(Product product, int quantity)
    {
        var existingItem = Items.FirstOrDefault(i => i.Product.Id == product.Id);
        if (existingItem is not null)
            existingItem.IncreaseQuantity(quantity);
        else
            Items.Add(new CartItem(product, quantity));
    }
    
    public void UpdateQuantity(Guid productId, int quantity)
    {
        var existingItem = Items.FirstOrDefault(x => x.Product.Id == productId);
        existingItem?.IncreaseQuantity(quantity);
    }

    public void RemoveItem(Guid productId)
    {
        Items.RemoveAll(i => i.Product.Id == productId);
    }

    public void ApplyDiscount(Discount discount)
    {
        AppliedDiscount = discount;
    }

    public Money CalculateTotal()
    {
        var total = Items.Sum(x => x.Product.Price.Amount * x.Quantity);

        if (AppliedDiscount != null)
            total -= AppliedDiscount.CalculateDiscount(total);

        return new Money(total);
    }
}
