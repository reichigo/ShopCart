using System.Net;

namespace ShopCart.CrossCutting;

public class GlobalException : Exception
{
    public HttpStatusCode StatusCode { get; }

    public GlobalException(string message, HttpStatusCode statusCode = HttpStatusCode.InternalServerError) 
        : base(message)
    {
        StatusCode = statusCode;
    }
}
