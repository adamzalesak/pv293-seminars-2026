using Microsoft.AspNetCore.Http.HttpResults;
using Kofi.BusinessLayer.Dtos;
using Kofi.BusinessLayer.Services;

namespace Kofi.API.Endpoints;

public static class ProductEndpoints
{
    public static void MapProductEndpoints(this WebApplication app)
    {
        app.MapGet("/products", async Task<Ok<PagedResult<ProductDto>>> (
            ProductService service, string? search, string? sort, int? page, int? pageSize) =>
        {
            var products = await service.GetProducts(
                search, sort, Math.Max(page ?? 1, 1), Math.Clamp(pageSize ?? 20, 1, 100));
            return TypedResults.Ok(products);
        });

        app.MapGet("/products/{id:guid}", async Task<Results<Ok<ProductDto>, NotFound>> (ProductService service, Guid id) =>
        {
            var product = await service.GetProduct(id);
            if (product == null)
            {
                return TypedResults.NotFound();
            }
            return TypedResults.Ok(product);
        });

        app.MapPost("/products", async Task<Results<Created<ProductDto>, BadRequest<ErrorResponse>>> (ProductService service, ProductDto request) =>
        {
            try
            {
                var product = await service.CreateProduct(request);
                return TypedResults.Created($"/products/{product.Id}", product);
            }
            catch (Exception ex)
            {
                return TypedResults.BadRequest(new ErrorResponse(ex.Message));
            }
        }).RequireAuthorization("Administrator");

        app.MapPut("/products/{id:guid}/price", async Task<Results<NoContent, BadRequest<string>, ProblemHttpResult>> (
            ProductService service, Guid id, ChangePriceRequest request) =>
        {
            // Validation lives here for this one, in the service for creation.
            if (request.Price <= 0)
            {
                return TypedResults.BadRequest("Price must be positive");
            }

            try
            {
                await service.ChangePrice(id, request.Price);
                return TypedResults.NoContent();
            }
            catch (Exception ex)
            {
                return TypedResults.Problem(ex.Message);
            }
        }).RequireAuthorization("Administrator");

        app.MapPost("/products/{id:guid}/deactivate", async Task<Results<Ok, ContentHttpResult>> (ProductService service, Guid id) =>
        {
            try
            {
                await service.Deactivate(id);
                return TypedResults.Ok();
            }
            catch (Exception ex)
            {
                return TypedResults.Text("Error: " + ex.Message, "text/plain", null, 500);
            }
        }).RequireAuthorization("Administrator");

        app.MapPost("/products/{id:guid}/restock", async Task<Ok> (
            ProductService service, Guid id, RestockRequest request) =>
        {
            await service.Restock(id, request.Quantity);
            return TypedResults.Ok();
        }).RequireAuthorization("Administrator");
    }
}
