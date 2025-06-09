using Microsoft.AspNetCore.Mvc;
using ShopCart.Api.Mappers;
using ShopCart.Api.Requests;
using ShopCart.Application.Interfaces;
using ShopCart.Application.UseCases;

public static class CartEndpoint
{
    public static RouteGroupBuilder MapCart(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/cart").WithTags("Cart");

        group.MapPost("/", CreateCartAsync)
            .WithName("CreateCart")
            .WithDescription("Creates a new shopping cart")
            .WithOpenApi();

        group.MapGet("/{cartId:guid}", GetCartByIdAsync)
            .WithName("GetCart")
            .WithDescription("Gets a shopping cart by ID")
            .WithOpenApi();

        group.MapPost("/{cartId:guid}/items", AddItemToCartAsync)
            .WithName("AddItemToCart")
            .WithDescription("Adds an item to the shopping cart")
            .WithOpenApi();

        group.MapDelete("/{cartId:guid}/items/{productId:guid}", RemoveItemFromCartAsync)
            .WithName("RemoveItemFromCart")
            .WithDescription("Removes an item from the shopping cart")
            .WithOpenApi();

        group.MapPost("/{cartId:guid}/discount", ApplyDiscountAsync)
            .WithName("ApplyDiscount")
            .WithDescription("Applies a discount to the shopping cart")
            .WithOpenApi();

        return group;
    }

    private static async Task<IResult> CreateCartAsync(
        [FromBody] CreateCartRequest request,
        [FromServices] CreateCartUseCase useCase)
    {
        var cartId = await useCase.ExecuteAsync(request.UserId);
        return Results.Created($"/cart/{cartId}", new { cartId });
    }

    private static async Task<IResult> GetCartByIdAsync(
        Guid cartId,
        [FromServices] GetCartDetailsUseCase useCase)
    {
        var cart = await useCase.ExecuteAsync(cartId);
        return Results.Ok(cart.ToResponse());
    }

    private static async Task<IResult> AddItemToCartAsync(
        Guid cartId,
        [FromBody] AddItemRequest request,
        [FromServices] AddItemToCartUseCase useCase)
    {
        await useCase.ExecuteAsync(cartId, request.ProductId, request.Quantity);
        return Results.Ok();
    }

    private static async Task<IResult> RemoveItemFromCartAsync(
        Guid cartId,
        Guid productId,
        [FromServices] RemoveItemFromCartUseCase useCase)
    {
        await useCase.ExecuteAsync(cartId, productId);
        return Results.Ok();
    }

    private static async Task<IResult> ApplyDiscountAsync(
        Guid cartId,
        [FromBody] ApplyDiscountRequest request,
        [FromServices] ApplyDiscountUseCase useCase)
    {
        await useCase.ExecuteAsync(cartId, request.DiscountCode);
        return Results.Ok();
    }
}