using Microsoft.EntityFrameworkCore;
using ShopCart.Api.Extensions;
using ShopCart.Api.Infrastructure;
using ShopCart.Application.Interfaces;
using ShopCart.Application.UseCases;
using ShopCart.Domain.Repositories;
using ShopCart.Infrastructure.Cache;
using ShopCart.Infrastructure.Cache.RedisDataCache;
using ShopCart.Infrastructure.Repositories;
using ShopCart.Infrastructure.Repositories.Datasource.SqlServerDataSource;
using StackExchange.Redis;
using DbContext = ShopCart.Infrastructure.DbContext;
using ShopCart.Api.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Register all services with appropriate lifetimes
RegisterServices(builder.Services, builder.Configuration);

if (builder.Environment.IsDevelopment())
{
    builder.Services.AddOpenApiUi();
}

var app = builder.Build();

// Adicionar o middleware de tratamento de exceções antes de outros middlewares
app.UseExceptionHandling();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    await DockerContainerManager.EnsureContainersRunningAsync(builder);
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.MapCart();

// Print available endpoints to console
PrintAvailableEndpoints(app);

using (var scope = app.Services.CreateScope())
{
    scope.ServiceProvider.GetRequiredService<DbContext>().Database.Migrate();
}

app.Run();


void RegisterServices(IServiceCollection services, IConfiguration configuration)
{
    // Database - Scoped with retry policy
    services.AddDbContext<DbContext>(options =>
        options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"),
            sqlServerOptionsAction: sqlOptions =>
            {
                sqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 5,
                    maxRetryDelay: TimeSpan.FromSeconds(30),
                    errorNumbersToAdd: null);
            }));
    
    // Redis connection - Singleton
    var redisConnection = configuration.GetConnectionString("RedisConnection") ?? "localhost:6379";
    services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect(redisConnection));
    
    // Cache providers - Scoped
    services.AddScoped<IRedisCacheProvider, RedisCacheProvider>();
    services.AddScoped<ICartRedisDataCache, CartRedisDataCache>();
    services.AddScoped<ICartCache, CartCache>();
    
    // Datasources - Scoped
    services.AddScoped<CartSqlDatasource>();
    services.AddScoped<ProductSqlDatasource>();
    services.AddScoped<DiscountSqlDatasource>();
    
    // Repositories - Scoped
    services.AddScoped<ICartRepository, CartRepository>();
    services.AddScoped<IProductRepository, ProductRepository>();
    services.AddScoped<IDiscountRepository, DiscountRepository>();
    
    // Use cases - Transient
    services.AddTransient<CreateCartUseCase>();
    services.AddTransient<GetCartDetailsUseCase>();
    services.AddTransient<AddItemToCartUseCase>();
    services.AddTransient<RemoveItemFromCartUseCase>();
    services.AddTransient<ApplyDiscountUseCase>();

    // services.AddOpenApi();
}

// Helper to show available endpoints
void PrintAvailableEndpoints(WebApplication webApp)
{
    var urls = webApp.Urls.ToList();
    var baseUrl = urls.FirstOrDefault() ?? "http://localhost:5240";
    
    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine("\n=================================================");
    Console.WriteLine("           SHOPCART API IS RUNNING               ");
    Console.WriteLine("=================================================");
    Console.WriteLine($"Server running at: {baseUrl}");
    Console.WriteLine("\nAvailable Endpoints:");
    
    try
    {
        // Get all endpoints from the endpoint data source
        var endpointDataSource = webApp.Services.GetRequiredService<Microsoft.AspNetCore.Routing.EndpointDataSource>();
        
        // Get all HTTP endpoints
        var endpoints = endpointDataSource.Endpoints
            .OfType<Microsoft.AspNetCore.Routing.RouteEndpoint>()
            .Where(e => e.Metadata.GetMetadata<Microsoft.AspNetCore.Routing.HttpMethodMetadata>() != null)
            .OrderBy(e => e.RoutePattern.RawText);
        
        // Display all endpoints
        foreach (var endpoint in endpoints)
        {
            var httpMethodMetadata = endpoint.Metadata.GetMetadata<Microsoft.AspNetCore.Routing.HttpMethodMetadata>();
            var httpMethod = httpMethodMetadata?.HttpMethods.FirstOrDefault() ?? "GET";
            
            var routePattern = endpoint.RoutePattern.RawText ?? "";
            var displayName = endpoint.DisplayName ?? "";
            
            if (displayName.StartsWith("HTTP: "))
                displayName = displayName.Substring(6);
                
            Console.WriteLine($"{httpMethod,-6} {baseUrl}/{routePattern,-40} - {displayName}");
        }
        
        // Add documentation endpoints
        Console.WriteLine($"GET    {baseUrl}/swagger                      - Swagger UI");
        Console.WriteLine($"GET    {baseUrl}/elements                     - Stoplight Elements");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error retrieving endpoints: {ex.Message}");
        Console.WriteLine("Please check your endpoint registration in CartEndpoint.cs");
    }
    
    Console.WriteLine("=================================================");
    Console.ResetColor();
}
