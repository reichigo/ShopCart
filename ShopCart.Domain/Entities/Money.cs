namespace ShopCart.Domain.Entities;

public record Money(decimal Amount)
{
    public static Money operator +(Money a, Money b) => new Money(a.Amount + b.Amount);
    public static Money operator -(Money a, Money b) => new Money(a.Amount - b.Amount);

    public override string ToString() => Amount.ToString("C");
}