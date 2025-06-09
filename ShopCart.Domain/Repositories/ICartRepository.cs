using ShopCart.Domain.Entities;

namespace ShopCart.Domain.Repositories;

public interface ICartRepository
{
    Task<Cart?> GetByIdAsync(Guid id);
    Task CreateAsync(Cart? cart);
    Task UpdateAsync(Guid id, Cart cart);
    Task DeleteAsync(Guid id);
}