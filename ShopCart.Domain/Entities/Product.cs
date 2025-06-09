using System.Text.Json.Serialization;

namespace ShopCart.Domain.Entities;

public class Product
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public Money Price { get; private set; }

    // Construtor sem parâmetros para desserialização
    public Product()
    {
        Id = Guid.Empty;
        Name = string.Empty;
        Price = new Money(0);
    }

    [JsonConstructor]
    public Product(Guid id, string name, Money price)
    {
        Id = id;
        Name = name;
        Price = price;
    }
}