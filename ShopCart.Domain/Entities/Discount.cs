using System.Text.Json.Serialization;

namespace ShopCart.Domain.Entities;

public class Discount
{
    public string Code { get; private set; }
    public decimal Value { get; private set; }
    public DiscountType Type { get; private set; }

    // Construtor sem parâmetros para desserialização
    public Discount()
    {
        Code = string.Empty;
        Value = 0;
        Type = DiscountType.Percentage;
    }

    [JsonConstructor]
    public Discount(string code, decimal value, DiscountType type)
    {
        Code = code;
        Value = value;
        Type = type;
    }

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