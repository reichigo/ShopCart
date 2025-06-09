namespace ShopCart.Infrastructure.Repositories.Datasource.SqlServerDataSource.SqlServerDataModel;

public class ProductSqlServerDatamodel
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
}