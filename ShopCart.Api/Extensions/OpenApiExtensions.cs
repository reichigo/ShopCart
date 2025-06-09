using Microsoft.OpenApi.Models;

namespace ShopCart.Api.Extensions;

public static class OpenApiExtensions
{
    public static IServiceCollection AddOpenApiUi(this IServiceCollection services)
    {
        // Adicione o serviço de explorador de API
        services.AddEndpointsApiExplorer();
        
        // Adicione o gerador de Swagger
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "ShopCart API",
                Version = "v1",
                Description = "API for managing shopping carts",
                Contact = new OpenApiContact
                {
                    Name = "ShopCart Team",
                    Email = "support@shopcart.com"
                }
            });
        });
        
        return services;
    }
    
    public static WebApplication MapOpenApi(this WebApplication app)
    {
        // Generate the OpenAPI document
        app.UseSwagger();
        
        // Serve static files for Stoplight Elements
        app.UseStaticFiles();
        
        // Serve Swagger UI
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "ShopCart API v1");
            c.RoutePrefix = "swagger";
        });
        
        // Map Stoplight Elements endpoint
        app.MapGet("/elements", (HttpContext context) =>
        {
            context.Response.Redirect("/stoplight.html");
            return Task.CompletedTask;
        });
        
        return app;
    }
}