namespace ShopCart.Infrastructure.Cache.RedisDataCache;

public interface ICartRedisDataCache : IRedisCacheProvider<RedisDataModel.CartRedisDataModel>;