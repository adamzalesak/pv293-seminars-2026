using Kofi.API.Auth;
using Microsoft.AspNetCore.Http.HttpResults;
using MediatR;
using Kofi.BusinessLayer.Features.Orders.AddItemToOrder;
using Kofi.BusinessLayer.Features.Orders.CancelOrder;
using Kofi.BusinessLayer.Features.Orders.ConfirmOrderPayment;
using Kofi.BusinessLayer.Features.Orders.CreateOrder;
using Kofi.BusinessLayer.Features.Orders.ExportOrders;
using Kofi.BusinessLayer.Features.Orders.GetOrderCountByStatus;
using Kofi.BusinessLayer.Features.Orders.GetOrderDetail;
using Kofi.BusinessLayer.Features.Orders.GetOrders;
using Kofi.BusinessLayer.Features.Orders.MarkOrderShipped;
using Kofi.BusinessLayer.Features.Orders.RemoveItemFromOrder;
using Kofi.BusinessLayer.Features.Orders.ResendOrderConfirmation;
using Kofi.BusinessLayer.Features.Orders.StartOrderPayment;
using Kofi.BusinessLayer.Features.Orders.UpdateShippingAddress;

namespace Kofi.API.Endpoints;

public static class OrderEndpoints
{
    public static void MapOrderEndpoints(this WebApplication app)
    {
        var orders = app.MapGroup("/orders").WithTags("Orders").RequireAuthorization();

        orders.MapGet("", async Task<Ok<List<OrderSummary>>> (
            IMediator mediator, bool? onlyPaid, DateTime? from, string? sortBy) =>
            TypedResults.Ok(await mediator.Send(new GetOrdersQuery
            {
                OnlyPaid = onlyPaid,
                From = from,
                SortBy = sortBy,
            })));

        orders.MapGet("/export", async Task<ContentHttpResult> (IMediator mediator, DateTime? from) =>
            TypedResults.Text(await mediator.Send(new ExportOrdersQuery { From = from }), "text/csv"))
            .RequireAuthorization(Roles.Administrator);

        orders.MapGet("/stats/{status}", async Task<Ok<OrderCountResponse>> (IMediator mediator, string status) =>
            TypedResults.Ok(new OrderCountResponse(
                status, await mediator.Send(new GetOrderCountByStatusQuery { Status = status }))))
            .RequireAuthorization(Roles.Administrator);

        orders.MapGet("/{id:guid}", async Task<Results<Ok<OrderDetail>, NotFound>> (IMediator mediator, Guid id) =>
        {
            var detail = await mediator.Send(new GetOrderDetailQuery { Id = id });
            return detail == null ? TypedResults.NotFound() : TypedResults.Ok(detail);
        });

        orders.MapPost("", async Task<Created<CreateOrderResponse>> (IMediator mediator, CreateOrderCommand command) =>
        {
            var order = await mediator.Send(command);
            return TypedResults.Created($"/orders/{order.Id}", order);
        }).RequireAuthorization(Roles.Customer);

        orders.MapPost("/{id:guid}/cancel", async Task<Ok> (IMediator mediator, Guid id) =>
        {
            await mediator.Send(new CancelOrderCommand { Id = id });
            return TypedResults.Ok();
        }).RequireAuthorization(Roles.Administrator);

        orders.MapPost("/{id:guid}/pay", async Task<Ok<StartOrderPaymentResponse>> (
            IMediator mediator, Guid id, HttpRequest http) =>
        {
            // The provider sends the customer back here when they are done.
            var payment = await mediator.Send(new StartOrderPaymentCommand
            {
                Id = id,
                ReturnUrl = $"{http.Scheme}://{http.Host}/orders/{id}/payment-return",
            });
            return TypedResults.Ok(payment);
        }).RequireAuthorization(Roles.Customer);

        // The provider's page redirects the browser here without our token,
        // so the endpoint is open and trusts nothing but the provider itself.
        orders.MapGet("/{id:guid}/payment-return", async Task<Ok<ConfirmOrderPaymentResponse>> (IMediator mediator, Guid id, Guid paymentId) =>
            TypedResults.Ok(await mediator.Send(new ConfirmOrderPaymentCommand { Id = id, PaymentId = paymentId })))
            .AllowAnonymous();

        orders.MapPost("/{id:guid}/ship", async Task<Ok> (IMediator mediator, Guid id) =>
        {
            await mediator.Send(new MarkOrderShippedCommand { Id = id });
            return TypedResults.Ok();
        }).RequireAuthorization(Roles.Administrator);

        orders.MapPut("/{id:guid}/address", async Task<NoContent> (
            IMediator mediator, Guid id, UpdateShippingAddressRequest request) =>
        {
            await mediator.Send(new UpdateShippingAddressCommand
            {
                Id = id, Street = request.Street, City = request.City, Zip = request.Zip,
            });
            return TypedResults.NoContent();
        }).RequireAuthorization(Roles.Administrator);

        orders.MapPost("/{id:guid}/items", async Task<Ok> (
            IMediator mediator, Guid id, AddItemToOrderRequest request) =>
        {
            await mediator.Send(new AddItemToOrderCommand
            {
                OrderId = id, ProductId = request.ProductId, Quantity = request.Quantity,
            });
            return TypedResults.Ok();
        }).RequireAuthorization(Roles.Administrator);

        orders.MapDelete("/{id:guid}/items/{itemId:guid}", async Task<NoContent> (
            IMediator mediator, Guid id, Guid itemId) =>
        {
            await mediator.Send(new RemoveItemFromOrderCommand { OrderId = id, OrderItemId = itemId });
            return TypedResults.NoContent();
        }).RequireAuthorization(Roles.Administrator);

        orders.MapPost("/{id:guid}/resend-confirmation", async Task<Ok> (IMediator mediator, Guid id) =>
        {
            await mediator.Send(new ResendOrderConfirmationCommand { Id = id });
            return TypedResults.Ok();
        }).RequireAuthorization(Roles.Administrator);
    }
}
