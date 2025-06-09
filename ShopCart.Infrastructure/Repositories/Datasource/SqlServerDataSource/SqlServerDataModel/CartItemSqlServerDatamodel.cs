namespace ShopCart.Infrastructure.Repositories.Datasource.SqlServerDataSource.SqlServerDataModel;

public class CartItemSqlServerDatamodel
{
    public Guid CartId { get; set; }
    public Guid ProductId { get; set; }
    public virtual ProductSqlServerDatamodel? Product { get; set; }
    public int Quantity { get; set; }
}