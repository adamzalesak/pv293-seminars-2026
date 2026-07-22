using Kofi.API.Auth;
using System.Security.Claims;
using Microsoft.AspNetCore.Http.HttpResults;
using Kofi.BusinessLayer.Dtos;
using Kofi.BusinessLayer.Services;

namespace Kofi.API.Endpoints;

public static class ProductEndpoints
{
    public static void MapProductEndpoints(this WebApplication app)
    {
        // The shop lists what is on sale; the administrator can ask for the rest.
        app.MapGet("/products", async Task<Results<Ok<PagedResult<ProductDto>>, ForbidHttpResult>> (
            IProductService service, ClaimsPrincipal user, string? search, string? sort, int? page, int? pageSize,
            bool? includeInactive) =>
        {
            if (includeInactive == true && !user.IsInRole(Roles.Administrator))
            {
                return TypedResults.Forbid();
            }

            var products = await service.GetProducts(
                search, sort, Math.Max(page ?? 1, 1), Math.Clamp(pageSize ?? 20, 1, 100), includeInactive ?? false);
            return TypedResults.Ok(products);
        });

        app.MapGet("/products/{id:guid}", async Task<Results<Ok<ProductDto>, NotFound>> (IProductService service, Guid id) =>
        {
            var product = await service.GetProduct(id);
            if (product == null)
            {
                return TypedResults.NotFound();
            }
            return TypedResults.Ok(product);
        });

        app.MapPost("/products", async Task<Results<Created<ProductDto>, BadRequest<ErrorResponse>>> (IProductService service, ProductDto request) =>
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
        }).RequireAuthorization(Roles.Administrator);

        app.MapPut("/products/{id:guid}", async Task<Results<NoContent, BadRequest<ErrorResponse>>> (
            IProductService service, Guid id, ProductDto request) =>
        {
            try
            {
                await service.UpdateProduct(id, request);
                return TypedResults.NoContent();
            }
            catch (Exception ex)
            {
                return TypedResults.BadRequest(new ErrorResponse(ex.Message));
            }
        }).RequireAuthorization(Roles.Administrator);

        app.MapPut("/products/{id:guid}/price", async Task<Results<NoContent, BadRequest<string>, ProblemHttpResult>> (
            IProductService service, Guid id, ChangePriceRequest request) =>
        {
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
        }).RequireAuthorization(Roles.Administrator);

        app.MapPost("/products/{id:guid}/deactivate", async Task<Results<Ok, ContentHttpResult>> (IProductService service, Guid id) =>
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
        }).RequireAuthorization(Roles.Administrator);

        app.MapPost("/products/{id:guid}/activate", async Task<Results<Ok, ContentHttpResult>> (IProductService service, Guid id) =>
        {
            try
            {
                await service.Activate(id);
                return TypedResults.Ok();
            }
            catch (Exception ex)
            {
                return TypedResults.Text("Error: " + ex.Message, "text/plain", null, 500);
            }
        }).RequireAuthorization(Roles.Administrator);

        app.MapPost("/products/{id:guid}/restock", async Task<Ok> (
            IProductService service, Guid id, RestockRequest request) =>
        {
            await service.Restock(id, request.Quantity);
            return TypedResults.Ok();
        }).RequireAuthorization(Roles.Administrator);
    }
}
