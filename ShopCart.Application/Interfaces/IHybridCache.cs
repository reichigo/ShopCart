namespace ShopCart.Application.Interfaces;

public interface IHybridCache
{
    Task<T?> GetAsync<T>(string key);
    Task SetAsync<T>(string key, T value, TimeSpan expiration);
    Task RemoveAsync(string key);
    
    // Métodos específicos para o cache híbrido
    Task<T?> GetFromPrimaryAsync<T>(string key);
    Task<T?> GetFromSecondaryAsync<T>(string key);
    Task SetToPrimaryAsync<T>(string key, T value, TimeSpan expiration);
    Task SetToSecondaryAsync<T>(string key, T value, TimeSpan expiration);
}