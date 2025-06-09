using Microsoft.EntityFrameworkCore;
using ShopCart.Infrastructure.Repositories.Datasource.SqlServerDataSource.SqlServerDataModel;

namespace ShopCart.Infrastructure;

public class DbContext : Microsoft.EntityFrameworkCore.DbContext
{
    public DbSet<CartSqlServerDatamodel> Carts { get; set; } = default!;
    public DbSet<CartItemSqlServerDatamodel> CartItems { get; set; } = default!;
    public DbSet<ProductSqlServerDatamodel> Products { get; set; } = default!;
    public DbSet<DiscountSqlServerDatamodel> Discounts { get; set; } = default!;

    public DbContext(DbContextOptions<DbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CartSqlServerDatamodel>(entity =>
        {
            entity.ToTable("Carts");
            entity.HasKey(c => c.Id);
            entity.HasMany(c => c.Items)
                .WithOne()
                .HasForeignKey("CartId");

            entity.HasOne(c => c.AppliedDiscount)
                .WithMany()
                .HasForeignKey("DiscountCode")
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<CartItemSqlServerDatamodel>(entity =>
        {
            entity.ToTable("CartItems");
            entity.HasKey(ci => new { ci.CartId, ci.ProductId });
            entity.HasOne(ci => ci.Product)
                .WithMany()
                .HasForeignKey(ci => ci.ProductId);
        });

        modelBuilder.Entity<ProductSqlServerDatamodel>(entity =>
        {
            entity.ToTable("Products");
            entity.HasKey(p => p.Id);
            entity.Property(p => p.Name).IsRequired().HasMaxLength(200);
            entity.Property(p => p.Price).HasColumnType("decimal(18,2)");
        });

        modelBuilder.Entity<DiscountSqlServerDatamodel>(entity =>
        {
            entity.ToTable("Discounts");
            entity.HasKey(d => d.Code);
            entity.Property(d => d.Value).HasColumnType("decimal(18,2)");
        });

        // Seed data
        SeedData(modelBuilder);
    }

    private void SeedData(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ProductSqlServerDatamodel>().HasData(
            new ProductSqlServerDatamodel
            {
                Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                Name = "Wireless Mouse",
                Price = 99.90m
            },
            new ProductSqlServerDatamodel
            {
                Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                Name = "Mechanical Keyboard",
                Price = 250.00m
            },
            new ProductSqlServerDatamodel
            {
                Id = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                Name = "USB-C Hub",
                Price = 89.00m
            },
            new ProductSqlServerDatamodel
            {
                Id = Guid.Parse("44444444-4444-4444-4444-444444444444"),
                Name = "HD Webcam",
                Price = 180.00m
            },
            new ProductSqlServerDatamodel
            {
                Id = Guid.Parse("55555555-5555-5555-5555-555555555555"),
                Name = "Noise Cancelling Headphones",
                Price = 520.50m
            }
        );

        modelBuilder.Entity<DiscountSqlServerDatamodel>().HasData(
            new DiscountSqlServerDatamodel
            {
                Code = "WELCOME10",
                Value = 10.00m,
                Type =  DiscountTypeSqlServerDatasource.FixedAmount
            },
            new DiscountSqlServerDatamodel
            {
                Code = "SUMMER20",
                Value = 20.00m,
                Type = DiscountTypeSqlServerDatasource.Percentage
            },
            new DiscountSqlServerDatamodel
            {
                Code = "FIXED50",
                Value = 50.00m,
                Type = DiscountTypeSqlServerDatasource.FixedAmount
            }
        );
    }
}