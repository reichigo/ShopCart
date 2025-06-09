namespace ShopCart.Api.Requests;

public record AddItemRequest(Guid ProductId, int Quantity);