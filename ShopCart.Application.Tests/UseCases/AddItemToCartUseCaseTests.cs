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

public class AddItemToCartUseCaseTests : TestBase
{
    private readonly Mock<ICartRepository> _cartRepositoryMock;
    private readonly Mock<IProductRepository> _productRepositoryMock;
    private readonly Mock<ICartCache> _cartCacheMock;
    private readonly AddItemToCartUseCase _sut;

    public AddItemToCartUseCaseTests()
    {
        _cartRepositoryMock = Fixture.Freeze<Mock<ICartRepository>>();
        _productRepositoryMock = Fixture.Freeze<Mock<IProductRepository>>();
        _cartCacheMock = Fixture.Freeze<Mock<ICartCache>>();
        _sut = new AddItemToCartUseCase(_cartRepositoryMock.Object, _productRepositoryMock.Object, _cartCacheMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_WhenCartAndProductExist_ShouldAddItemToCart()
    {
        // Arrange
        var cartId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var quantity = 2;
        
        var cart = new Cart(Guid.NewGuid(), cartId);
        var product = new Product(productId, "Test Product", new Money(10.0m));
        
        _cartRepositoryMock.Setup(x => x.GetByIdAsync(cartId))
            .ReturnsAsync(cart);
        _productRepositoryMock.Setup(x => x.GetByIdAsync(productId))
            .ReturnsAsync(product);
        
        // Configurar o mock para aceitar qualquer chamada de InvalidateCacheAsync
        _cartCacheMock.Setup(x => x.CreateAsync(It.IsAny<Cart>(), It.IsAny<TimeSpan>()))
            .Returns(Task.CompletedTask);
        
        // Act
        await _sut.ExecuteAsync(cartId, productId, quantity);
        
        // Assert
        _cartRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Guid>(), It.IsAny<Cart>()), Times.Once);
        _cartCacheMock.Verify(x => x.InvalidateCacheAsync(It.IsAny<Guid>()), Times.Once);
        _cartCacheMock.Verify(x => x.CreateAsync(cart, It.IsAny<TimeSpan>()), Times.Once);
        
        // Verificar se o item foi adicionado ao carrinho
        Assert.Contains(cart.Items, i => i.Product.Id == productId && i.Quantity == quantity);
    }

    [Fact]
    public async Task ExecuteAsync_WhenProductDoesNotExist_ShouldThrowException()
    {
        // Arrange
        var cartId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var quantity = 2;
        
        var cart = new Cart(Guid.NewGuid(), cartId);
        
        _cartRepositoryMock.Setup(x => x.GetByIdAsync(cartId))
            .ReturnsAsync(cart);
        _productRepositoryMock.Setup(x => x.GetByIdAsync(productId))
            .ReturnsAsync((Product)null);
        
        // Act & Assert
        var exception = await Assert.ThrowsAsync<GlobalException>(() => 
            _sut.ExecuteAsync(cartId, productId, quantity));
        
        Assert.Equal(HttpStatusCode.NotFound, exception.StatusCode);
        Assert.Contains("Product not found", exception.Message);
    }

    [Fact]
    public async Task ExecuteAsync_WhenProductAlreadyInCart_ShouldUpdateQuantity()
    {
        // Arrange
        var cartId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var initialQuantity = 1;
        var additionalQuantity = 2;
        
        var product = new Product(productId, "Test Product", new Money(10.0m));
        var cart = new Cart(Guid.NewGuid(), cartId);
        cart.AddCartItem(product, initialQuantity);
        
        _cartRepositoryMock.Setup(x => x.GetByIdAsync(cartId))
            .ReturnsAsync(cart);
        
        // Act
        await _sut.ExecuteAsync(cartId, productId, additionalQuantity);
        
        // Assert
        _cartRepositoryMock.Verify(x => x.UpdateAsync(cartId, cart), Times.Once);
        
        // Verificar se a quantidade foi atualizada
        var cartItem = cart.Items.First(i => i.Product.Id == productId);
        Assert.Equal(initialQuantity + additionalQuantity, cartItem.Quantity);
    }
}