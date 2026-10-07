using Kofi.Warehouse.Application.Features.Products.RestockProduct;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Kofi.Warehouse.API.Endpoints;

public static class StockEndpoints
{
    public static void MapStockEndpoints(this WebApplication app)
    {
        var products = app.MapGroup("/products").WithTags("Products");
        products.MapPost("/{id:guid}/restock", async Task<NoContent> (
                IMediator mediator, Guid id, RestockProductRequest request) =>
            {
                await mediator.Send(new RestockProductCommand { Id = id, Quantity = request.Quantity });
                return TypedResults.NoContent();
            })
            .RequireAuthorization(Roles.Administrator);
    }
}
