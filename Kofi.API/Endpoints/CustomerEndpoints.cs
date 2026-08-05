using Kofi.API.Auth;
using Microsoft.AspNetCore.Http.HttpResults;
using MediatR;
using Kofi.BusinessLayer.Features.Customers.GetCustomer;
using Kofi.BusinessLayer.Features.Customers.GetCustomers;
using Kofi.BusinessLayer.Features.Customers.MakeCustomerVip;
using Kofi.BusinessLayer.Features.Customers.RegisterCustomer;

namespace Kofi.API.Endpoints;

public static class CustomerEndpoints
{
    public static void MapCustomerEndpoints(this WebApplication app)
    {
        var customers = app.MapGroup("/customers").WithTags("Customers").RequireAuthorization(Roles.Administrator);

        customers.MapGet("", async Task<Ok<List<CustomerSummary>>> (IMediator mediator) =>
            TypedResults.Ok(await mediator.Send(new GetCustomersQuery())));

        customers.MapGet("/{id:guid}", async Task<Results<Ok<CustomerDetail>, NotFound>> (IMediator mediator, Guid id) =>
        {
            var customer = await mediator.Send(new GetCustomerQuery { Id = id });
            return customer == null ? TypedResults.NotFound() : TypedResults.Ok(customer);
        });

        customers.MapPost("", async Task<Created<RegisterCustomerResponse>> (IMediator mediator, RegisterCustomerCommand command) =>
        {
            var customer = await mediator.Send(command);
            return TypedResults.Created($"/customers/{customer.Id}", customer);
        });

        customers.MapPost("/{id:guid}/vip", async Task<NoContent> (IMediator mediator, Guid id) =>
        {
            await mediator.Send(new MakeCustomerVipCommand { Id = id });
            return TypedResults.NoContent();
        });
    }
}
