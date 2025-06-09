namespace ShopCart.Infrastructure.Repositories.Datasource.SqlServerDataSource.SqlServerDataModel;

public class CartSqlServerDatamodel
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public virtual List<CartItemSqlServerDatamodel> Items { get; set; } = [];
    public virtual DiscountSqlServerDatamodel? AppliedDiscount { get; set; }
    public string? DiscountCode { get; set; }
}