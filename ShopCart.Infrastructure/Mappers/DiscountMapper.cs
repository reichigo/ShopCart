using ShopCart.Domain.Entities;
using ShopCart.Infrastructure.Repositories.Datasource.SqlServerDataSource.SqlServerDataModel;

namespace ShopCart.Infrastructure.Mappers;

public static class DiscountMapper
{
    public static Discount? ToDomain(this DiscountSqlServerDatamodel? source)
    {
        if (source is null)
            throw new ArgumentNullException(nameof(source));

        var discount = new Discount(
            source.Code,
            source.Value,
            source.Type == DiscountTypeSqlServerDatasource.Percentage ? DiscountType.Percentage : DiscountType.FixedAmount);    

        return discount;
    }
}