using AutoFixture;
using Moq;
using ShopCart.Application.UseCases;
using ShopCart.Domain.Entities;
using ShopCart.Domain.Repositories;
using Xunit;

namespace ShopCart.Application.Tests.UseCases;

public class CreateCartUseCaseTests : TestBase
{
    private readonly Mock<ICartRepository> _cartRepositoryMock;
    private readonly CreateCartUseCase _sut;

    public CreateCartUseCaseTests()
    {
        _cartRepositoryMock = Fixture.Freeze<Mock<ICartRepository>>();
        _sut = new CreateCartUseCase(_cartRepositoryMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_WithValidUserId_ShouldCreateCartAndReturnId()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        _cartRepositoryMock.Setup(x => x.CreateAsync(It.IsAny<Cart>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _sut.ExecuteAsync(userId);

        // Assert
        Assert.NotEqual(Guid.Empty, result);
        _cartRepositoryMock.Verify(x => x.CreateAsync(It.Is<Cart>(c => 
            c.Id == result && 
            c.UserId == Guid.Parse(userId))), Times.Once);
    }
}