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

public class ApplyDiscountUseCaseTests : TestBase
{
    private readonly Mock<ICartRepository> _cartRepositoryMock;
    private readonly Mock<IDiscountRepository> _discountRepositoryMock;
    private readonly Mock<ICartCache> _cartCacheMock;
    private readonly ApplyDiscountUseCase _sut;

    public ApplyDiscountUseCaseTests()
    {
        _cartRepositoryMock = Fixture.Freeze<Mock<ICartRepository>>();
        _discountRepositoryMock = Fixture.Freeze<Mock<IDiscountRepository>>();
        _cartCacheMock = Fixture.Freeze<Mock<ICartCache>>();
        _sut = new ApplyDiscountUseCase(_cartRepositoryMock.Object, _discountRepositoryMock.Object, _cartCacheMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_WhenCartAndDiscountExist_ShouldApplyDiscountToCart()
    {
        // Arrange
        var cartId = Guid.NewGuid();
        var discountCode = "DISCOUNT10";
        
        var cart = new Cart(Guid.NewGuid(), Guid.NewGuid());
        var discount = new Discount(discountCode, 10.0m, DiscountType.Percentage);
        
        _cartRepositoryMock.Setup(x => x.GetByIdAsync(cartId))
            .ReturnsAsync(cart);
        _discountRepositoryMock.Setup(x => x.GetByCodeAsync(discountCode))
            .ReturnsAsync(discount);
        
        // Act
        await _sut.ExecuteAsync(cartId, discountCode);
        
        // Assert
        _cartRepositoryMock.Verify(x => x.UpdateAsync(cartId, cart), Times.Once);
        _cartCacheMock.Verify(x => x.InvalidateCacheAsync(cartId), Times.Once);
        _cartCacheMock.Verify(x => x.CreateAsync(cart, It.IsAny<TimeSpan>()), Times.Once);
        
        // Verificar se o desconto foi aplicado ao carrinho
        Assert.Equal(discount, cart.AppliedDiscount);
    }

    [Fact]
    public async Task ExecuteAsync_WhenCartDoesNotExist_ShouldThrowException()
    {
        // Arrange
        var cartId = Guid.NewGuid();
        var discountCode = "DISCOUNT10";
        
        _cartRepositoryMock.Setup(x => x.GetByIdAsync(cartId))
            .ReturnsAsync((Cart)null);
        
        // Act & Assert
        var exception = await Assert.ThrowsAsync<GlobalException>(() => 
            _sut.ExecuteAsync(cartId, discountCode));
        
        Assert.Equal(HttpStatusCode.NotFound, exception.StatusCode);
        Assert.Contains("Cart not found", exception.Message);
    }

    [Fact]
    public async Task ExecuteAsync_WhenDiscountDoesNotExist_ShouldThrowException()
    {
        // Arrange
        var cartId = Guid.NewGuid();
        var discountCode = "INVALID";
        
        var cart = new Cart(Guid.NewGuid(), cartId);
        
        _cartRepositoryMock.Setup(x => x.GetByIdAsync(cartId))
            .ReturnsAsync(cart);
        _discountRepositoryMock.Setup(x => x.GetByCodeAsync(discountCode))
            .ReturnsAsync((Discount)null);
        
        // Act & Assert
        var exception = await Assert.ThrowsAsync<GlobalException>(() => 
            _sut.ExecuteAsync(cartId, discountCode));
        
        Assert.Equal(HttpStatusCode.NotFound, exception.StatusCode);
        Assert.Contains("Discount not found", exception.Message);
    }

    [Fact]
    public async Task ExecuteAsync_WhenDiscountAlreadyApplied_ShouldReplaceDiscount()
    {
        // Arrange
        var cartId = Guid.NewGuid();
        var oldDiscountCode = "OLD10";
        var newDiscountCode = "NEW20";
        
        var cart = new Cart(Guid.NewGuid(), cartId);
        var oldDiscount = new Discount(oldDiscountCode, 10.0m, DiscountType.Percentage);
        var newDiscount = new Discount(newDiscountCode, 20.0m, DiscountType.Percentage);
        
        cart.ApplyDiscount(oldDiscount);
        
        _cartRepositoryMock.Setup(x => x.GetByIdAsync(cartId))
            .ReturnsAsync(cart);
        _discountRepositoryMock.Setup(x => x.GetByCodeAsync(newDiscountCode))
            .ReturnsAsync(newDiscount);
        
        // Act
        await _sut.ExecuteAsync(cartId, newDiscountCode);
        
        // Assert
        _cartRepositoryMock.Verify(x => x.UpdateAsync(cartId, cart), Times.Once);
        
        // Verificar se o novo desconto foi aplicado ao carrinho
        Assert.Equal(newDiscount, cart.AppliedDiscount);
        Assert.NotEqual(oldDiscount, cart.AppliedDiscount);
    }
    
    [Fact]
    public async Task ExecuteAsync_WithFixedAmountDiscount_ShouldApplyCorrectly()
    {
        // Arrange
        var cartId = Guid.NewGuid();
        var discountCode = "FIXED50";
        var discountValue = 50.0m;
        
        var cart = new Cart(Guid.NewGuid(), cartId);
        var product = new Product(Guid.NewGuid(), "Test Product", new Money(100.0m));
        cart.AddCartItem(product, 1);
        
        var discount = new Discount(discountCode, discountValue, DiscountType.FixedAmount);
        
        _cartRepositoryMock.Setup(x => x.GetByIdAsync(cartId))
            .ReturnsAsync(cart);
        _discountRepositoryMock.Setup(x => x.GetByCodeAsync(discountCode))
            .ReturnsAsync(discount);
        
        // Act
        await _sut.ExecuteAsync(cartId, discountCode);
        
        // Assert
        _cartRepositoryMock.Verify(x => x.UpdateAsync(cartId, cart), Times.Once);
        
        // Verificar se o desconto foi aplicado corretamente
        Assert.Equal(discount, cart.AppliedDiscount);
        Assert.Equal(50.0m, cart.CalculateTotal().Amount);
    }
    
    [Fact]
    public async Task ExecuteAsync_WithEmptyCart_ShouldStillApplyDiscount()
    {
        // Arrange
        var cartId = Guid.NewGuid();
        var discountCode = "DISCOUNT10";
        
        var cart = new Cart(Guid.NewGuid(), cartId);
        var discount = new Discount(discountCode, 10.0m, DiscountType.Percentage);
        
        _cartRepositoryMock.Setup(x => x.GetByIdAsync(cartId))
            .ReturnsAsync(cart);
        _discountRepositoryMock.Setup(x => x.GetByCodeAsync(discountCode))
            .ReturnsAsync(discount);
        
        // Act
        await _sut.ExecuteAsync(cartId, discountCode);
        
        // Assert
        _cartRepositoryMock.Verify(x => x.UpdateAsync(cartId, cart), Times.Once);
        
        // Verificar se o desconto foi aplicado ao carrinho vazio
        Assert.Equal(discount, cart.AppliedDiscount);
        Assert.Equal(0.0m, cart.CalculateTotal().Amount); // Desconto em carrinho vazio deve ser zero
    }
    
    [Fact]
    public async Task ExecuteAsync_ShouldInvalidateCacheBeforeGettingCart()
    {
        // Arrange
        var cartId = Guid.NewGuid();
        var discountCode = "DISCOUNT10";
        
        var cart = new Cart(Guid.NewGuid(), cartId);
        var discount = new Discount(discountCode, 10.0m, DiscountType.Percentage);
        
        _cartRepositoryMock.Setup(x => x.GetByIdAsync(cartId))
            .ReturnsAsync(cart);
        _discountRepositoryMock.Setup(x => x.GetByCodeAsync(discountCode))
            .ReturnsAsync(discount);
        
        // Configurar a sequência de chamadas para verificar a ordem
        var sequence = new MockSequence();
        _cartCacheMock.InSequence(sequence).Setup(x => x.InvalidateCacheAsync(cartId))
            .Returns(Task.CompletedTask);
        _cartRepositoryMock.InSequence(sequence).Setup(x => x.GetByIdAsync(cartId))
            .ReturnsAsync(cart);
        
        // Act
        await _sut.ExecuteAsync(cartId, discountCode);
        
        // Assert
        _cartCacheMock.Verify(x => x.InvalidateCacheAsync(cartId), Times.Once);
        _cartRepositoryMock.Verify(x => x.GetByIdAsync(cartId), Times.Once);
    }
}