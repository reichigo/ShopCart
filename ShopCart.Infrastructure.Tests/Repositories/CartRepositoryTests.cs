using Microsoft.EntityFrameworkCore;
using Moq;
using ShopCart.Domain.Entities;
using ShopCart.Infrastructure.Mappers;
using ShopCart.Infrastructure.Repositories;
using ShopCart.Infrastructure.Repositories.Datasource.SqlServerDataSource;
using ShopCart.Infrastructure.Repositories.Datasource.SqlServerDataSource.SqlServerDataModel;
using Xunit;

namespace ShopCart.Infrastructure.Tests.Repositories;

public class CartRepositoryTests : TestBase
{
    private readonly DbContextOptions<DbContext> _dbContextOptions;
    private readonly DbContext _dbContext;
    private readonly CartRepository _sut;

    public CartRepositoryTests()
    {
        // Configurar o banco de dados em memória para testes
        _dbContextOptions = new DbContextOptionsBuilder<DbContext>()
            .UseInMemoryDatabase(databaseName: $"ShopCartTestDb_{Guid.NewGuid()}")
            .Options;
        
        _dbContext = new DbContext(_dbContextOptions);
        
        // Criar o repositório com o datasource real
        var cartDatasource = new CartSqlDatasource(_dbContext);
        _sut = new CartRepository(cartDatasource);
    }

    [Fact]
    public async Task GetByIdAsync_WhenCartExists_ShouldReturnCart()
    {
        // Arrange
        var cartId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        
        var cartDatamodel = new CartSqlServerDatamodel
        {
            Id = cartId,
            UserId = userId,
            Items = new List<CartItemSqlServerDatamodel>()
        };
        
        await _dbContext.Carts.AddAsync(cartDatamodel);
        await _dbContext.SaveChangesAsync();
        
        // Act
        var result = await _sut.GetByIdAsync(cartId);
        
        // Assert
        Assert.NotNull(result);
        Assert.Equal(cartId, result.Id);
        Assert.Equal(userId, result.UserId);
    }

    [Fact]
    public async Task CreateAsync_ShouldAddCartToDatabase()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var cart = new Cart(userId, Guid.NewGuid());
        
        // Act
        await _sut.CreateAsync(cart);
        
        // Assert
        var savedCart = await _dbContext.Carts.FindAsync(cart.Id);
        Assert.NotNull(savedCart);
        Assert.Equal(cart.Id, savedCart.Id);
        Assert.Equal(cart.UserId, savedCart.UserId);
    }

    [Fact]
    public async Task DeleteAsync_WhenCartExists_ShouldRemoveCartFromDatabase()
    {
        // Arrange
        var cartId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        
        var cartDatamodel = new CartSqlServerDatamodel
        {
            Id = cartId,
            UserId = userId,
            Items = new List<CartItemSqlServerDatamodel>()
        };
        
        await _dbContext.Carts.AddAsync(cartDatamodel);
        await _dbContext.SaveChangesAsync();
        
        // Act
        await _sut.DeleteAsync(cartId);
        
        // Assert
        var deletedCart = await _dbContext.Carts.FindAsync(cartId);
        Assert.Null(deletedCart);
    }
}