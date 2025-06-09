using System.Net;
using AutoFixture;
using Moq;
using ShopCart.Application.Interfaces;
using ShopCart.Application.UseCases;
using ShopCart.CrossCutting;
using ShopCart.Domain.Entities;
using ShopCart.Domain.Repositories;
using Xunit;

namespace ShopCart.Application.Tests.UseCases;

public class GetCartDetailsUseCaseTests : TestBase
{
    private readonly Mock<ICartRepository> _cartRepositoryMock;
    private readonly Mock<ICartCache> _cartCacheMock;
    private readonly GetCartDetailsUseCase _sut;

    public GetCartDetailsUseCaseTests()
    {
        _cartRepositoryMock = Fixture.Freeze<Mock<ICartRepository>>();
        _cartCacheMock = Fixture.Freeze<Mock<ICartCache>>();
        _sut = new GetCartDetailsUseCase(_cartRepositoryMock.Object, _cartCacheMock.Object);
    }
 
    [Fact]
    public async Task ExecuteAsync_WhenCartExistsInCache_ShouldReturnCachedCart()
    {
        // Arrange
        var cartId = Guid.NewGuid();
        var cachedCart = new Cart(Guid.NewGuid(), cartId);
        
        _cartCacheMock.Setup(x => x.GetAsync(cartId))
            .ReturnsAsync(cachedCart);
        
        // Act
        var result = await _sut.ExecuteAsync(cartId);
        
        // Assert
        Assert.Same(cachedCart, result);
        _cartCacheMock.Verify(x => x.GetAsync(cartId), Times.Once);
        _cartRepositoryMock.Verify(x => x.GetByIdAsync(It.IsAny<Guid>()), Times.Never);
    }
}