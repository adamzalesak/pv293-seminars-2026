using Kofi.Common.Abstractions;
using Kofi.Marketing.Application.Persistence;
using Kofi.Marketing.Domain.Coupons;
using Kofi.Subscriptions.Contracts;

namespace Kofi.Marketing.Application.EventHandlers;

public class IssueReferralCouponOnSubscriptionActivated(IMarketingDbContext db)
    : IIntegrationEventHandler<SubscriptionActivatedIntegrationEvent>
{
    public Task Handle(SubscriptionActivatedIntegrationEvent activated, CancellationToken ct)
    {
        db.Coupons.Add(Coupon.IssueReferral(ReferralCode.Generate(), activated.CustomerId, activated.ActivatedAt));
        return Task.CompletedTask;
    }
}
