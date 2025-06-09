using ShopCart.Domain.Entities;
using ShopCart.Domain.Repositories;
using ShopCart.Infrastructure.Repositories.Datasource.SqlServerDataSource;
using ShopCart.Infrastructure.Mappers;
using ShopCart.Infrastructure.Repositories.Datasource.SqlServerDataSource.SqlServerDataModel;

namespace ShopCart.Infrastructure.Repositories;

public class CartRepository(CartSqlDatasource cartSqlDatasource) : ICartRepository
{
    public async Task<Cart?> GetByIdAsync(Guid id)
    {
        var cartSqlDataModel = await cartSqlDatasource.GetByIdAsync(id);

        return cartSqlDataModel.ToDomain();
    }

    public Task CreateAsync(Cart cart)
    {
        return cartSqlDatasource.CreateAsync(cart.ToSqlDatamodel());
    }

    public Task UpdateAsync(Guid id, Cart cart)
    {
        return cartSqlDatasource.UpdateAsync(id, cart.ToSqlDatamodel());
    }

    public Task DeleteAsync(Guid id)
    {
        return cartSqlDatasource.DeleteAsync(id);
    }
}