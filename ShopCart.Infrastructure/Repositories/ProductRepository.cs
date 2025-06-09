using ShopCart.Domain.Entities;
using ShopCart.Domain.Repositories;
using ShopCart.Infrastructure.Repositories.Datasource.SqlServerDataSource;
using ShopCart.Infrastructure.Mappers;

namespace ShopCart.Infrastructure.Repositories;

public class ProductRepository(ProductSqlDatasource productSqlDatasource) : IProductRepository
{
    public async Task<Product?> GetByIdAsync(Guid id)
    {
        var productSqlDataModel = await productSqlDatasource.GetByIdAsync(id);
       return productSqlDataModel?.ToDomain();
    }
}