using ShopCart.Domain.Entities;

namespace ShopCart.Infrastructure.Cache.RedisDataCache.RedisDataModel;

public record ProductRedisDataModel(Guid Id, string Name, decimal Amount);