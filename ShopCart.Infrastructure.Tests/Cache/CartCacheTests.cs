using Moq;
using ShopCart.Infrastructure.Cache;
using ShopCart.Infrastructure.Cache.RedisDataCache;
using ShopCart.Infrastructure.Cache.RedisDataCache.RedisDataModel;
using Xunit;

namespace ShopCart.Infrastructure.Tests.Cache;

public class CartCacheTests
{
    private readonly Mock<ICartRedisDataCache> _redisCacheMock;
    private readonly CartCache _sut;

    public CartCacheTests()
    {
        _redisCacheMock = new Mock<ICartRedisDataCache>();
        _sut = new CartCache(_redisCacheMock.Object);
    }

    [Fact]
    public async Task GetAsync_ShouldCallRedisCache()
    {
        // Arrange
        var expectedCart = new CartRedisDataModel(Guid.NewGuid(), Guid.NewGuid(), [], null);
        var key = $"cart-total:{expectedCart.Id}"; // Use o mesmo padrão de chave do seu método real

        _redisCacheMock.Setup(x => x.GetAsync(key))
            .ReturnsAsync(expectedCart);

        // Act
        var result = await _sut.GetAsync(expectedCart.Id);

        // Assert
        _redisCacheMock.Verify(x => x.GetAsync(expectedCart.Id.ToString()), Times.Once);
    }
}