using ShopCart.Domain.Entities;

namespace ShopCart.Infrastructure.Cache.RedisDataCache.RedisDataModel;

public record DiscountRedisDataModel(string Code, decimal Value , DiscountType Type);