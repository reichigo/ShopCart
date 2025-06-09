using ShopCart.Domain.Entities;

namespace ShopCart.Domain.Repositories;

public interface IDiscountRepository
{
    Task<Discount?> GetByCodeAsync(string code);
}