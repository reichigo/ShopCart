namespace ShopCart.Application.Interfaces;

public interface ICacheProvider<T>
{
    Task<T?> GetAsync(string key);
    Task SetAsync(string key, T value, TimeSpan expiration);
    Task RemoveAsync(string key);
}