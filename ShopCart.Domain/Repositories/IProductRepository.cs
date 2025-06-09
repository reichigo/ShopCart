using ShopCart.Domain.Entities;

namespace ShopCart.Domain.Repositories;

public interface IProductRepository
{
    Task<Product?> GetByIdAsync(Guid id);
}