using Microsoft.AspNetCore.Http.HttpResults;
using Kofi.BusinessLayer.Dtos;
using Kofi.BusinessLayer.Services;

namespace Kofi.API.Endpoints;

public static class CustomerEndpoints
{
    public static void MapCustomerEndpoints(this WebApplication app)
    {
        app.MapGet("/customers", async Task<Ok<List<CustomerDto>>> (CustomerService service) =>
        {
            var customers = await service.GetCustomers();
            return TypedResults.Ok(customers);
        }).RequireAuthorization("Administrator");

        app.MapGet("/customers/{id:guid}", async Task<Results<Ok<CustomerDto>, NotFound>> (CustomerService service, Guid id) =>
        {
            var customer = await service.GetCustomer(id);
            if (customer == null)
            {
                return TypedResults.NotFound();
            }
            return TypedResults.Ok(customer);
        }).RequireAuthorization("Administrator");

        app.MapPost("/customers", async Task<Created<CustomerDto>> (CustomerService service, CustomerDto request) =>
        {
            var customer = await service.RegisterCustomer(request);
            return TypedResults.Created($"/customers/{customer.Id}", customer);
        }).RequireAuthorization("Administrator");

        app.MapPost("/customers/{id:guid}/vip", async Task<Results<Ok, BadRequest<ErrorResponse>>> (CustomerService service, Guid id) =>
        {
            try
            {
                await service.MakeVip(id);
                return TypedResults.Ok();
            }
            catch (Exception ex)
            {
                return TypedResults.BadRequest(new ErrorResponse(ex.Message));
            }
        }).RequireAuthorization("Administrator");
    }
}
