using System.Text.Json;
using AutoFixture;
using Moq;
using StackExchange.Redis;
using ShopCart.Infrastructure.Cache.RedisDataCache;
using ShopCart.Infrastructure.Cache.RedisDataCache.RedisDataModel;
using Xunit;

namespace ShopCart.Infrastructure.Tests.Cache;

public class RedisCacheProviderTests : TestBase
{
    private readonly Mock<IConnectionMultiplexer> _redisMock;
    private readonly Mock<IDatabase> _databaseMock;
    private readonly RedisCacheProvider<CartRedisDataModel> _sut;

    public RedisCacheProviderTests()
    {
        _databaseMock = new Mock<IDatabase>();
        _redisMock = new Mock<IConnectionMultiplexer>();
        
        _redisMock.Setup(x => x.GetDatabase(It.IsAny<int>(), It.IsAny<object>()))
            .Returns(_databaseMock.Object);
        
        _sut = new RedisCacheProvider<CartRedisDataModel>(_redisMock.Object);
    }

    [Fact]
    public async Task GetAsync_WhenKeyExists_ShouldReturnDeserializedValue()
    {
        // Arrange
        var key = "test-key";
        var value = new CartRedisDataModel(Guid.NewGuid(), Guid.NewGuid(), [], null);

        var serializedValue = JsonSerializer.Serialize(value);
        
        _databaseMock.Setup(x => x.StringGetAsync(key, It.IsAny<CommandFlags>()))
            .ReturnsAsync(new RedisValue(serializedValue));
        
        // Act
        var result = await _sut.GetAsync(key);
        
        // Assert
        _databaseMock.Verify(x => x.StringGetAsync(key, It.IsAny<CommandFlags>()), Times.Once);
    }

    [Fact]
    public async Task GetAsync_WhenKeyDoesNotExist_ShouldReturnDefault()
    {
        // Arrange
        var key = "non-existent-key";
        
        _databaseMock.Setup(x => x.StringGetAsync(key, It.IsAny<CommandFlags>()))
            .ReturnsAsync(RedisValue.Null);
        
        // Act
        var result = await _sut.GetAsync(key);
        
        // Assert
        _databaseMock.Verify(x => x.StringGetAsync(key, It.IsAny<CommandFlags>()), Times.Once);
    }
}
