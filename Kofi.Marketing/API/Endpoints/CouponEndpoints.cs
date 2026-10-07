using Kofi.Marketing.Application.Features.Coupons.CheckCoupon;
using Kofi.Marketing.Application.Features.Coupons.GetCoupons;
using Kofi.Marketing.Application.Features.Coupons.GetMyCoupons;
using Kofi.Marketing.Application.Features.Coupons.InvalidateCoupon;
using Kofi.Marketing.Application.Features.Coupons.IssueCoupon;
using Kofi.Marketing.Application.Features.Coupons.UpdateCoupon;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Kofi.Marketing.API.Endpoints;

public static class CouponEndpoints
{
    public static void MapCouponEndpoints(this WebApplication app)
    {
        var coupons = app.MapGroup("/coupons").WithTags("Coupons").RequireAuthorization();

        coupons.MapGet("", async Task<Ok<List<CouponSummary>>> (IMediator mediator) =>
                TypedResults.Ok(await mediator.Send(new GetCouponsQuery())))
            .RequireAuthorization(Roles.Administrator);

        coupons.MapGet("/mine", async Task<Ok<List<MyCoupon>>> (IMediator mediator) =>
                TypedResults.Ok(await mediator.Send(new GetMyCouponsQuery())))
            .RequireAuthorization(Roles.Customer);

        coupons.MapGet("/{code}/check", async Task<Ok<CouponCheck>> (IMediator mediator, string code) =>
                TypedResults.Ok(await mediator.Send(new CheckCouponQuery { Code = code })))
            .RequireAuthorization(Roles.Customer);

        coupons.MapPost("", async Task<Created<IssueCouponResponse>> (IMediator mediator, IssueCouponCommand command) =>
            {
                var coupon = await mediator.Send(command);
                return TypedResults.Created($"/coupons/{coupon.Id}", coupon);
            })
            .RequireAuthorization(Roles.Administrator);

        coupons.MapPut("/{id:guid}", async Task<NoContent> (IMediator mediator, Guid id, UpdateCouponRequest request) =>
            {
                await mediator.Send(new UpdateCouponCommand
                    { Id = id, DiscountPercent = request.DiscountPercent, ExpiresAt = request.ExpiresAt });
                return TypedResults.NoContent();
            })
            .RequireAuthorization(Roles.Administrator);

        coupons.MapPost("/{id:guid}/invalidate", async Task<NoContent> (IMediator mediator, Guid id) =>
            {
                await mediator.Send(new InvalidateCouponCommand { Id = id });
                return TypedResults.NoContent();
            })
            .RequireAuthorization(Roles.Administrator);
    }
}
