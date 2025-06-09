namespace ShopCart.Domain.Entities;

public class Discount(string code, decimal value, DiscountType type)
{
    public string Code { get; private set; } = code;
    public decimal Value { get; private set; } = value;
    public DiscountType Type { get; private set; } = type;

    public decimal CalculateDiscount(decimal totalAmount)
    {
        return Type switch
        {
            DiscountType.Percentage => totalAmount * (Value / 100),
            DiscountType.FixedAmount => Value,
            _ => 0
        };
    }
}