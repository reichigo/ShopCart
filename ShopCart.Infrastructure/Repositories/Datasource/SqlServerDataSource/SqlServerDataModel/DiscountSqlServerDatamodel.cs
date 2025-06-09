namespace ShopCart.Infrastructure.Repositories.Datasource.SqlServerDataSource.SqlServerDataModel;

public class DiscountSqlServerDatamodel
{
    public string Code { get; init; } = string.Empty;
    public decimal Value { get; init; }
    public DiscountTypeSqlServerDatasource Type { get; init; }
}