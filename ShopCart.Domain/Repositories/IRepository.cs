namespace ShopCart.Domain.Repositories;

public interface IRepository<TEntity, TId> where TEntity : class
{
    Task<TEntity?> GetByIdAsync(TId id);
    Task CreateAsync(TEntity entity);
    Task UpdateAsync(TId id, TEntity entity);
    Task DeleteAsync(TId id);
}