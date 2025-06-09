using System.Net;
using Microsoft.EntityFrameworkCore;
using ShopCart.CrossCutting;
using ShopCart.Domain.Entities;
using ShopCart.Infrastructure.Repositories.Datasource.SqlServerDataSource.SqlServerDataModel;

namespace ShopCart.Infrastructure.Repositories.Datasource.SqlServerDataSource;

public class CartSqlDatasource(DbContext context)
{
    private readonly DbSet<CartSqlServerDatamodel> _dbSet = context.Carts;

    public virtual async Task<CartSqlServerDatamodel?> GetByIdAsync(Guid id)
    {
        return await _dbSet
            .Include(c => c.Items)
                .ThenInclude(i => i.Product)
            .Include(c => c.AppliedDiscount)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public virtual Task CreateAsync(CartSqlServerDatamodel entity)
    {
        _dbSet.Add(entity);
        return context.SaveChangesAsync();
    }

    public virtual async Task UpdateAsync(Guid id, CartSqlServerDatamodel entity)
    {
        var existingCart = await _dbSet
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (existingCart == null)
        {
            throw new GlobalException($"Cart with ID {id} not found.", HttpStatusCode.NotFound);
        }

        existingCart.UserId = entity.UserId;
        existingCart.DiscountCode = entity.DiscountCode;

        foreach (var existingItem in existingCart.Items.ToList().Where(existingItem => !entity.Items.Any(i => i.ProductId == existingItem.ProductId)))
        {
            context.CartItems.Remove(existingItem);
        }

        foreach (var item in entity.Items)
        {
            var existingItem = existingCart.Items
                .FirstOrDefault(i => i.ProductId == item.ProductId);

            if (existingItem == null)
            {
                var newItem = new CartItemSqlServerDatamodel
                {
                    CartId = id,
                    ProductId = item.ProductId,
                    Quantity = item.Quantity
                };
                existingCart.Items.Add(newItem);
            }
            else
            {
                existingItem.Quantity = item.Quantity;
            }
        }

         await context.SaveChangesAsync();
    }

    public virtual async Task DeleteAsync(Guid id)
    {
        var entity = await _dbSet.FindAsync(id);
        if (entity != null)
        {
            _dbSet.Remove(entity);
            await context.SaveChangesAsync();
        }
    }
}