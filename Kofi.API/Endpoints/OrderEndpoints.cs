using System.Security.Claims;
using Kofi.API.Auth;
using Microsoft.AspNetCore.Http.HttpResults;
using Kofi.BusinessLayer.Dtos;
using Kofi.BusinessLayer.Services;

namespace Kofi.API.Endpoints;

public static class OrderEndpoints
{
    public static void MapOrderEndpoints(this WebApplication app)
    {
        // A customer sees their own orders, the administrator everyone's.
        app.MapGet("/orders", async Task<Results<Ok<List<OrderDto>>, ProblemHttpResult>> (
            IOrderService service,
            ClaimsPrincipal user,
            bool? includeItems,
            bool? onlyPaid,
            DateTime? from,
            string? sortBy) =>
        {
            try
            {
                var orders = await service.GetOrders(
                    user.CustomerId(), includeItems ?? false, onlyPaid ?? false, from, sortBy);
                return TypedResults.Ok(orders);
            }
            catch (Exception ex)
            {
                return TypedResults.Problem(ex.Message);
            }
        }).RequireAuthorization();

        app.MapGet("/orders/{id:guid}", async Task<Results<Ok<OrderDto>, NotFound>> (IOrderService service, Guid id, ClaimsPrincipal user) =>
        {
            var detail = await service.GetOrderDetail(id, user.CustomerId());
            if (detail == null)
            {
                return TypedResults.NotFound();
            }
            return TypedResults.Ok(detail);
        }).RequireAuthorization();

        app.MapPost("/orders", async Task<Results<Created<OrderDto>, BadRequest<ErrorResponse>>> (IOrderService service, OrderDto request, ClaimsPrincipal user) =>
        {
            if (string.IsNullOrWhiteSpace(request.CustomerName))
            {
                return TypedResults.BadRequest(new ErrorResponse("Customer name is required"));
            }

            try
            {
                var order = await service.CreateOrder(user.CustomerId()!.Value, request);
                return TypedResults.Created($"/orders/{order.Id}", order);
            }
            catch (Exception ex)
            {
                return TypedResults.BadRequest(new ErrorResponse(ex.Message));
            }
        }).RequireAuthorization(Roles.Customer);

        app.MapPost("/orders/{id:guid}/cancel", async Task<Results<Ok, ContentHttpResult>> (IOrderService service, Guid id) =>
        {
            try
            {
                await service.CancelOrder(id);
                return TypedResults.Ok();
            }
            catch (Exception ex)
            {
                return TypedResults.Text("Error: " + ex.Message, "text/plain", null, 500);
            }
        }).RequireAuthorization(Roles.Administrator);

        app.MapPost("/orders/{id:guid}/pay", async Task<Results<Ok<PaymentStartedResponse>, BadRequest<ErrorResponse>>> (
            IOrderService service, Guid id, ClaimsPrincipal user, HttpRequest http) =>
        {
            // The provider sends the customer back here when they are done.
            var returnUrl = $"{http.Scheme}://{http.Host}/orders/{id}/payment-return";
            try
            {
                var payment = await service.StartPayment(id, user.CustomerId()!.Value, returnUrl);
                return TypedResults.Ok(new PaymentStartedResponse(payment.PaymentId, payment.RedirectUrl));
            }
            catch (Exception ex)
            {
                return TypedResults.BadRequest(new ErrorResponse(ex.Message));
            }
        }).RequireAuthorization(Roles.Customer);

        // The provider's page redirects the browser here without our token,
        // so the endpoint is open and trusts nothing but the provider itself.
        app.MapGet("/orders/{id:guid}/payment-return", async Task<Results<Ok<PaymentReturnResponse>, BadRequest<ErrorResponse>>> (IOrderService service, Guid id, Guid paymentId) =>
        {
            try
            {
                var status = await service.ConfirmPayment(id, paymentId);
                return TypedResults.Ok(new PaymentReturnResponse(id, status.ToString()));
            }
            catch (Exception ex)
            {
                return TypedResults.BadRequest(new ErrorResponse(ex.Message));
            }
        });

        app.MapPost("/orders/{id:guid}/ship", async Task<Ok> (IOrderService service, Guid id) =>
        {
            await service.MarkAsShipped(id);
            return TypedResults.Ok();
        }).RequireAuthorization(Roles.Administrator);

        app.MapPut("/orders/{id:guid}/address", async Task<Results<NoContent, ProblemHttpResult>> (
            IOrderService service, Guid id, UpdateAddressRequest request) =>
        {
            try
            {
                await service.UpdateShippingAddress(id, request.Street, request.City, request.Zip);
                return TypedResults.NoContent();
            }
            catch (Exception ex)
            {
                return TypedResults.Problem(ex.Message);
            }
        }).RequireAuthorization(Roles.Administrator);

        app.MapPost("/orders/{id:guid}/items", async Task<Results<Ok, BadRequest<string>>> (
            IOrderService service, Guid id, AddItemRequest request) =>
        {
            try
            {
                await service.AddItemToOrder(id, request.ProductId, request.Quantity);
                return TypedResults.Ok();
            }
            catch (Exception ex)
            {
                return TypedResults.BadRequest(ex.Message);
            }
        }).RequireAuthorization(Roles.Administrator);

        app.MapDelete("/orders/{id:guid}/items/{itemId:guid}", async Task<Results<NoContent, BadRequest<string>>> (
            IOrderService service, Guid id, Guid itemId) =>
        {
            try
            {
                await service.RemoveItemFromOrder(id, itemId);
                return TypedResults.NoContent();
            }
            catch (Exception ex)
            {
                return TypedResults.BadRequest(ex.Message);
            }
        }).RequireAuthorization(Roles.Administrator);

        app.MapPost("/orders/{id:guid}/resend-confirmation", async Task<Results<Ok, ProblemHttpResult>> (IOrderService service, Guid id) =>
        {
            try
            {
                await service.ResendConfirmationEmail(id);
                return TypedResults.Ok();
            }
            catch (Exception ex)
            {
                return TypedResults.Problem(ex.Message);
            }
        }).RequireAuthorization(Roles.Administrator);

        app.MapGet("/orders/export", async Task<ContentHttpResult> (IOrderService service, DateTime? from) =>
        {
            var csv = await service.ExportOrdersToCsv(from);
            return TypedResults.Text(csv, "text/csv");
        }).RequireAuthorization(Roles.Administrator);

        app.MapGet("/orders/stats/{status}", async Task<Ok<OrderCountResponse>> (IOrderService service, string status) =>
        {
            var count = await service.GetOrderCountByStatus(status);
            return TypedResults.Ok(new OrderCountResponse(status, count));
        }).RequireAuthorization(Roles.Administrator);
    }
}
