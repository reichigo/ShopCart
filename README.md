# ShopCart API

## Overview

ShopCart API is a modern, microservice-based shopping cart system built with .NET 9. It provides a robust and scalable solution for e-commerce applications, featuring cart management, product catalog, and discount handling capabilities.

## Features

- **Cart Management**: Create carts, add/remove items, and retrieve cart details
- **Discount System**: Apply discount codes to carts (percentage or fixed amount)
- **Caching**: Redis-based caching for improved performance
- **Persistence**: SQL Server database for reliable data storage
- **API Documentation**: Swagger UI and Stoplight Elements for API exploration
- **Docker Support**: Containerized deployment with Docker and Docker Compose

## Architecture

The project follows Clean Architecture principles with a clear separation of concerns:

- **ShopCart.Api**: API endpoints, middleware, and configuration
- **ShopCart.Application**: Use cases and business logic
- **ShopCart.Domain**: Core entities and repository interfaces
- **ShopCart.Infrastructure**: Data access, caching, and external services
- **ShopCart.CrossCutting**: Shared utilities and cross-cutting concerns
- **Test Projects**: Unit and integration tests for each layer

## Getting Started

### Prerequisites

- .NET 9 SDK
- Docker and Docker Compose (for containerized deployment)
- SQL Server (automatically provisioned via Docker)
- Redis (automatically provisioned via Docker)

### Running the Application

#### Using Docker Compose

The easiest way to run the application is using Docker Compose:

```bash
# Clone the repository
git clone https://github.com/yourusername/shopcart.git
cd shopcart

# Start the application and dependencies
docker compose up
```

This will start:
- ShopCart API on ports 8080/8081
- SQL Server on port 1433
- Redis on port 6379

#### Using .NET CLI

```bash
# Clone the repository
git clone https://github.com/yourusername/shopcart.git
cd shopcart

# Start dependencies (SQL Server and Redis)
docker compose up sqlserver redis -d

# Run the API
cd ShopCart.Api
dotnet run
```

### API Documentation

Once the application is running, you can explore the API using:

- **Swagger UI**: http://localhost:8080/swagger
- **Stoplight Elements**: http://localhost:8080/elements

## API Endpoints

| Method | Endpoint                      | Description                       |
|--------|-------------------------------|-----------------------------------|
| POST   | /cart                         | Create a new cart                 |
| GET    | /cart/{cartId}                | Get cart details                  |
| POST   | /cart/{cartId}/items          | Add item to cart                  |
| DELETE | /cart/{cartId}/items/{itemId} | Remove item from cart             |
| POST   | /cart/{cartId}/discount       | Apply discount code to cart       |

## Development

### Project Structure

```
ShopCart/
├── ShopCart.Api/                # API endpoints and configuration
├── ShopCart.Application/        # Business logic and use cases
├── ShopCart.Domain/             # Core entities and interfaces
├── ShopCart.Infrastructure/     # Data access and external services
├── ShopCart.CrossCutting/       # Shared utilities
├── ShopCart.Api.Tests/          # API layer tests
├── ShopCart.Application.Tests/  # Application layer tests
├── ShopCart.Infrastructure.Tests/ # Infrastructure layer tests
├── compose.yaml                 # Docker Compose configuration
└── README.md                    # This file
```

### Running Tests

```bash
# Run all tests
dotnet test

# Run specific test project
dotnet test ShopCart.Api.Tests
```

## Deployment

The application is containerized and can be deployed to any environment that supports Docker containers.

### Environment Variables

The following environment variables can be configured:

- `ASPNETCORE_ENVIRONMENT`: Development, Staging, Production
- `ConnectionStrings__DefaultConnection`: SQL Server connection string
- `ConnectionStrings__RedisConnection`: Redis connection string

## License

This project is licensed under the MIT License - see the LICENSE file for details.
