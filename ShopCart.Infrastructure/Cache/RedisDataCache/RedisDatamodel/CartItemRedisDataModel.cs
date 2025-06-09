namespace ShopCart.Infrastructure.Cache.RedisDataCache.RedisDataModel;

public record CartItemRedisDataModel(ProductRedisDataModel Product, int Quantity);