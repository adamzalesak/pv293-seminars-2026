using Kofi.API.Auth;
using System.Security.Claims;
using Microsoft.AspNetCore.Http.HttpResults;
using MediatR;
using Kofi.BusinessLayer.Abstractions;
using Kofi.BusinessLayer.Features.Products.ActivateProduct;
using Kofi.BusinessLayer.Features.Products.ChangeProductPrice;
using Kofi.BusinessLayer.Features.Products.CreateProduct;
using Kofi.BusinessLayer.Features.Products.DeactivateProduct;
using Kofi.BusinessLayer.Features.Products.GetProduct;
using Kofi.BusinessLayer.Features.Products.RestockProduct;
using Kofi.BusinessLayer.Features.Products.SearchProducts;
using Kofi.BusinessLayer.Features.Products.UpdateProduct;

namespace Kofi.API.Endpoints;

public static class ProductEndpoints
{
    public static void MapProductEndpoints(this WebApplication app)
    {
        var products = app.MapGroup("/products").WithTags("Products");

        // The shop lists what is on sale; the administrator can ask for the rest.
        products.MapGet("", async Task<Results<Ok<PagedResult<ProductSummary>>, ForbidHttpResult>> (
            IMediator mediator, ClaimsPrincipal user, string? search, string? sort, int? page, int? pageSize,
            bool? includeInactive) =>
        {
            if (includeInactive == true && !user.IsInRole(Roles.Administrator))
            {
                return TypedResults.Forbid();
            }

            return TypedResults.Ok(await mediator.Send(new SearchProductsQuery
            {
                Search = search,
                Sort = sort,
                Page = Math.Max(page ?? 1, 1),
                PageSize = Math.Clamp(pageSize ?? 20, 1, 100),
                IncludeInactive = includeInactive ?? false,
            }));
        });

        products.MapGet("/{id:guid}", async Task<Results<Ok<ProductDetail>, NotFound>> (IMediator mediator, Guid id) =>
        {
            var product = await mediator.Send(new GetProductQuery { Id = id });
            return product == null ? TypedResults.NotFound() : TypedResults.Ok(product);
        });

        products.MapPost("", async Task<Created<CreateProductResponse>> (IMediator mediator, CreateProductCommand command) =>
        {
            var product = await mediator.Send(command);
            return TypedResults.Created($"/products/{product.Id}", product);
        }).RequireAuthorization(Roles.Administrator);

        products.MapPut("/{id:guid}", async Task<NoContent> (
            IMediator mediator, Guid id, UpdateProductRequest request) =>
        {
            await mediator.Send(new UpdateProductCommand
            {
                Id = id,
                Name = request.Name,
                Description = request.Description,
                RoastedOn = request.RoastedOn,
                Origin = request.Origin,
                ImageUrl = request.ImageUrl,
            });
            return TypedResults.NoContent();
        }).RequireAuthorization(Roles.Administrator);

        products.MapPut("/{id:guid}/price", async Task<NoContent> (
            IMediator mediator, Guid id, ChangeProductPriceRequest request) =>
        {
            await mediator.Send(new ChangeProductPriceCommand { Id = id, Price = request.Price });
            return TypedResults.NoContent();
        }).RequireAuthorization(Roles.Administrator);

        products.MapPost("/{id:guid}/deactivate", async Task<NoContent> (IMediator mediator, Guid id) =>
        {
            await mediator.Send(new DeactivateProductCommand { Id = id });
            return TypedResults.NoContent();
        }).RequireAuthorization(Roles.Administrator);

        products.MapPost("/{id:guid}/activate", async Task<NoContent> (IMediator mediator, Guid id) =>
        {
            await mediator.Send(new ActivateProductCommand { Id = id });
            return TypedResults.NoContent();
        }).RequireAuthorization(Roles.Administrator);

        products.MapPost("/{id:guid}/restock", async Task<NoContent> (
            IMediator mediator, Guid id, RestockProductRequest request) =>
        {
            await mediator.Send(new RestockProductCommand { Id = id, Quantity = request.Quantity });
            return TypedResults.NoContent();
        }).RequireAuthorization(Roles.Administrator);
    }
}
