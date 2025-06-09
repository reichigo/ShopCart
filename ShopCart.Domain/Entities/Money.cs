using System.Text.Json.Serialization;

namespace ShopCart.Domain.Entities;

public class Money
{
    public decimal Amount { get; private set; }

    // Construtor sem parâmetros para desserialização
    public Money()
    {
        Amount = 0;
    }
    public Money(decimal amount)
    {
        Amount = amount;
    }
}
