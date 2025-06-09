using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using ShopCart.Api.Middleware;
using ShopCart.CrossCutting;
using Xunit;

namespace ShopCart.Api.Tests.Middleware;

public class ExceptionHandlingMiddlewareTests
{
    private readonly Mock<ILogger<ExceptionHandlingMiddleware>> _loggerMock;
    private readonly Mock<IWebHostEnvironment> _envMock;
    private readonly ExceptionHandlingMiddleware _middleware;
    private readonly HttpContext _httpContext;
    private readonly Mock<IServiceProvider> _serviceProviderMock;

    public ExceptionHandlingMiddlewareTests()
    {
        _loggerMock = new Mock<ILogger<ExceptionHandlingMiddleware>>();
        _envMock = new Mock<IWebHostEnvironment>();
        _serviceProviderMock = new Mock<IServiceProvider>();
        
        // Configurar o ambiente de desenvolvimento
        _envMock.Setup(e => e.EnvironmentName).Returns("Development");
        _serviceProviderMock.Setup(sp => sp.GetService(typeof(IWebHostEnvironment)))
            .Returns(_envMock.Object);
        
        // Configurar o contexto HTTP
        _httpContext = new DefaultHttpContext
        {
            RequestServices = _serviceProviderMock.Object,
            Response =
            {
                Body = new MemoryStream()
            }
        };
        
        // Criar o middleware
        _middleware = new ExceptionHandlingMiddleware(
            next: _ => throw new Exception("Test exception"),
            logger: _loggerMock.Object);
    }

    [Fact]
    public async Task InvokeAsync_WithGlobalException_ShouldReturnCorrectStatusAndMessage()
    {
        // Arrange
        var expectedStatusCode = HttpStatusCode.NotFound;
        var expectedMessage = "Resource not found";
        var nextDelegate = new RequestDelegate(_ => 
            throw new GlobalException(expectedMessage, expectedStatusCode));
        
        var middleware = new ExceptionHandlingMiddleware(nextDelegate, _loggerMock.Object);
        
        // Act
        await middleware.InvokeAsync(_httpContext);
        
        // Assert
        _httpContext.Response.Body.Seek(0, SeekOrigin.Begin);
        var responseBody = await new StreamReader(_httpContext.Response.Body).ReadToEndAsync();
        var response = JsonSerializer.Deserialize<Dictionary<string, object>>(responseBody, 
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        
        Assert.Equal((int)expectedStatusCode, _httpContext.Response.StatusCode);
        Assert.Equal("application/json", _httpContext.Response.ContentType);
        Assert.Equal(expectedMessage, response["message"].ToString());
    }

    [Fact]
    public async Task InvokeAsync_WithArgumentException_ShouldReturnBadRequest()
    {
        // Arrange
        var expectedMessage = "Invalid argument";
        var nextDelegate = new RequestDelegate(_ => 
            throw new ArgumentException(expectedMessage));
        
        var middleware = new ExceptionHandlingMiddleware(nextDelegate, _loggerMock.Object);
        
        // Act
        await middleware.InvokeAsync(_httpContext);
        
        // Assert
        _httpContext.Response.Body.Seek(0, SeekOrigin.Begin);
        var responseBody = await new StreamReader(_httpContext.Response.Body).ReadToEndAsync();
        var response = JsonSerializer.Deserialize<Dictionary<string, object>>(responseBody, 
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        
        Assert.Equal((int)HttpStatusCode.BadRequest, _httpContext.Response.StatusCode);
        Assert.Equal(expectedMessage, response["message"].ToString());
    }

    [Fact]
    public async Task InvokeAsync_WithUnauthorizedAccessException_ShouldReturnUnauthorized()
    {
        // Arrange
        var nextDelegate = new RequestDelegate(_ => 
            throw new UnauthorizedAccessException());
        
        var middleware = new ExceptionHandlingMiddleware(nextDelegate, _loggerMock.Object);
        
        // Act
        await middleware.InvokeAsync(_httpContext);
        
        // Assert
        Assert.Equal((int)HttpStatusCode.Unauthorized, _httpContext.Response.StatusCode);
    }

    [Fact]
    public async Task InvokeAsync_WithKeyNotFoundException_ShouldReturnNotFound()
    {
        // Arrange
        var expectedMessage = "Key not found";
        var nextDelegate = new RequestDelegate(_ => 
            throw new KeyNotFoundException(expectedMessage));
        
        var middleware = new ExceptionHandlingMiddleware(nextDelegate, _loggerMock.Object);
        
        // Act
        await middleware.InvokeAsync(_httpContext);
        
        // Assert
        _httpContext.Response.Body.Seek(0, SeekOrigin.Begin);
        var responseBody = await new StreamReader(_httpContext.Response.Body).ReadToEndAsync();
        var response = JsonSerializer.Deserialize<Dictionary<string, object>>(responseBody, 
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        
        Assert.Equal((int)HttpStatusCode.NotFound, _httpContext.Response.StatusCode);
        Assert.Equal(expectedMessage, response["message"].ToString());
    }

    [Fact]
    public async Task InvokeAsync_WithGenericException_ShouldReturnInternalServerError()
    {
        // Arrange
        var expectedMessage = "Test exception";
        var nextDelegate = new RequestDelegate(_ => 
            throw new Exception(expectedMessage));
        
        var middleware = new ExceptionHandlingMiddleware(nextDelegate, _loggerMock.Object);
        
        // Act
        await middleware.InvokeAsync(_httpContext);
        
        // Assert
        _httpContext.Response.Body.Seek(0, SeekOrigin.Begin);
        var responseBody = await new StreamReader(_httpContext.Response.Body).ReadToEndAsync();
        var response = JsonSerializer.Deserialize<Dictionary<string, object>>(responseBody, 
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        
        Assert.Equal((int)HttpStatusCode.InternalServerError, _httpContext.Response.StatusCode);
        
        // Em ambiente de desenvolvimento, a mensagem original deve ser retornada
        Assert.Equal(expectedMessage, response["message"].ToString());
    }

    [Fact]
    public async Task InvokeAsync_InProductionEnvironment_ShouldHideExceptionDetails()
    {
        // Arrange
        _envMock.Setup(e => e.EnvironmentName).Returns("Production");
        
        var nextDelegate = new RequestDelegate(_ => 
            throw new Exception("Sensitive error details"));
        
        var middleware = new ExceptionHandlingMiddleware(nextDelegate, _loggerMock.Object);
        
        // Act
        await middleware.InvokeAsync(_httpContext);
        
        // Assert
        _httpContext.Response.Body.Seek(0, SeekOrigin.Begin);
        var responseBody = await new StreamReader(_httpContext.Response.Body).ReadToEndAsync();
        var response = JsonSerializer.Deserialize<Dictionary<string, object>>(responseBody, 
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        
        Assert.Equal((int)HttpStatusCode.InternalServerError, _httpContext.Response.StatusCode);
        
        // Em ambiente de produção, a mensagem genérica deve ser retornada
        Assert.Equal("An unexpected error occurred.", response["message"].ToString());
        
        // Verificar se os detalhes do erro não estão presentes
        Assert.Null(response.GetValueOrDefault("details"));
    }
}