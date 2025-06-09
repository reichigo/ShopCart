using ShopCart.Application.Interfaces;

namespace ShopCart.Infrastructure.Cache.RedisDataCache;

public interface IRedisCacheProvider<T> : ICacheProvider<T>;