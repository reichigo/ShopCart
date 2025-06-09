using ShopCart.Domain.Entities;
using ShopCart.Infrastructure.Repositories.Datasource.SqlServerDataSource.SqlServerDataModel;

namespace ShopCart.Infrastructure.Mappers;

public static class ProductMapper
{
    public static Product ToDomain(this ProductSqlServerDatamodel productSqlServerDatamodel)
    {
        return new Product(
            productSqlServerDatamodel.Id,
            productSqlServerDatamodel.Name,
            new Money(productSqlServerDatamodel.Price));
    }
}