namespace ShopCart.Infrastructure.Cache.RedisDataCache.RedisDataModel;

public record CartRedisDataModel(Guid Id, Guid UserId, IEnumerable<CartItemRedisDataModel> Items,DiscountRedisDataModel? Discount);