using Kofi.Application.Features.Customers.GetCustomer;
using Kofi.Application.Features.Customers.GetCustomers;
using Kofi.Application.Features.Customers.MakeCustomerVip;
using Kofi.Application.Features.Customers.RegisterCustomer;
using Kofi.Auth;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Kofi.Endpoints;

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
