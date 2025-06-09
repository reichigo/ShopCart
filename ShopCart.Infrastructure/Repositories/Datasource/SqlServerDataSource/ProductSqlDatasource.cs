using Microsoft.EntityFrameworkCore;
using ShopCart.Infrastructure.Repositories.Datasource.SqlServerDataSource.SqlServerDataModel;

namespace ShopCart.Infrastructure.Repositories.Datasource.SqlServerDataSource;

public sealed class ProductSqlDatasource(DbContext context)
{
    private readonly DbContext _context = context;
    private readonly DbSet<ProductSqlServerDatamodel> _dbSet = context.Products;

    public Task<ProductSqlServerDatamodel?> GetByIdAsync(Guid id)
    {
        return _dbSet.FindAsync(id).AsTask();
    }
}
