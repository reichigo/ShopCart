using Microsoft.EntityFrameworkCore;
using ShopCart.Infrastructure.Repositories.Datasource.SqlServerDataSource.SqlServerDataModel;

namespace ShopCart.Infrastructure.Repositories.Datasource.SqlServerDataSource;

public class DiscountSqlDatasource(DbContext context)
{
    private readonly DbContext _context = context;
    private readonly DbSet<DiscountSqlServerDatamodel> _dbSet = context.Discounts;

    public virtual Task<DiscountSqlServerDatamodel?> GetByCodeAsync(string code)
    {
        return _dbSet.FindAsync(code).AsTask();
    }
}