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

public class RemoveItemFromCartUseCaseTests : TestBase
{
    private readonly Mock<ICartRepository> _cartRepositoryMock;
    private readonly Mock<ICartCache> _cartCacheMock;
    private readonly RemoveItemFromCartUseCase _sut;

    public RemoveItemFromCartUseCaseTests()
    {
        _cartRepositoryMock = Fixture.Freeze<Mock<ICartRepository>>();
        _cartCacheMock = Fixture.Freeze<Mock<ICartCache>>();
        _sut = new RemoveItemFromCartUseCase(_cartRepositoryMock.Object, _cartCacheMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_WhenCartExists_ShouldRemoveItemFromCart()
    {
        // Arrange
        var cartId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        
        var product = new Product(productId, "Test Product", new Money(10.0m));
        var cart = new Cart(Guid.NewGuid(), cartId);
        cart.AddCartItem(product, 1);
        
        _cartRepositoryMock.Setup(x => x.GetByIdAsync(cartId))
            .ReturnsAsync(cart);
        
        // Act
        await _sut.ExecuteAsync(cartId, productId);
        
        // Assert
        _cartRepositoryMock.Verify(x => x.UpdateAsync(cartId, cart), Times.Once);
        _cartCacheMock.Verify(x => x.InvalidateCacheAsync(cartId), Times.Once);
        _cartCacheMock.Verify(x => x.CreateAsync(cart, It.IsAny<TimeSpan>()), Times.Once);
        
        // Verificar se o item foi removido do carrinho
        Assert.DoesNotContain(cart.Items, i => i.Product.Id == productId);
    }

    [Fact]
    public async Task ExecuteAsync_WhenCartDoesNotExist_ShouldThrowException()
    {
        // Arrange
        var cartId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        
        _cartRepositoryMock.Setup(x => x.GetByIdAsync(cartId))
            .ReturnsAsync((Cart)null);
        
        // Act & Assert
        var exception = await Assert.ThrowsAsync<GlobalException>(() => 
            _sut.ExecuteAsync(cartId, productId));
        
        Assert.Equal(HttpStatusCode.NotFound, exception.StatusCode);
        Assert.Contains("Cart not found", exception.Message);
    }

    [Fact]
    public async Task ExecuteAsync_WhenItemNotInCart_ShouldNotThrowException()
    {
        // Arrange
        var cartId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var differentProductId = Guid.NewGuid();
        
        var product = new Product(differentProductId, "Different Product", new Money(10.0m));
        var cart = new Cart(Guid.NewGuid(), cartId);
        cart.AddCartItem(product, 1);
        
        _cartRepositoryMock.Setup(x => x.GetByIdAsync(cartId))
            .ReturnsAsync(cart);
        
        // Act - Não deve lançar exceção
        await _sut.ExecuteAsync(cartId, productId);
        
        // Assert
        _cartRepositoryMock.Verify(x => x.UpdateAsync(cartId, cart), Times.Once);
        
        // Verificar se o carrinho ainda contém o item original
        Assert.Contains(cart.Items, i => i.Product.Id == differentProductId);
    }
}