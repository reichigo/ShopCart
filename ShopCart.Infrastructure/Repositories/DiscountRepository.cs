using ShopCart.Domain.Entities;
using ShopCart.Domain.Repositories;
using ShopCart.Infrastructure.Repositories.Datasource.SqlServerDataSource;
using ShopCart.Infrastructure.Mappers;

namespace ShopCart.Infrastructure.Repositories;

public class DiscountRepository : IDiscountRepository
{
    private readonly DiscountSqlDatasource _discountSqlDatasource;

    public DiscountRepository(DiscountSqlDatasource discountSqlDatasource)
    {
        _discountSqlDatasource = discountSqlDatasource;
    }

    public async Task<Discount?> GetByCodeAsync(string code)
    {
        var discountSqlDataModel = await _discountSqlDatasource.GetByCodeAsync(code);
        return discountSqlDataModel?.ToDomain();
    }
}