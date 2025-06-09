namespace ShopCart.Domain.Entities;

public class Product(Guid id, string name, Money price)
{
    public Guid Id { get; private set; } = id;
    public string Name { get; private set; } = name;
    public Money Price { get; private set; } = price;
}