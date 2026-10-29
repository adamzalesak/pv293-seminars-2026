using Kofi.Common.Demo;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Kofi.Common.Abstractions;
using Kofi.Marketing.Application.Persistence;
using Kofi.Marketing.Domain.Coupons;
using Kofi.Subscriptions.Contracts;

namespace Kofi.Marketing.Application.EventHandlers;

public class IssueReferralCouponOnSubscriptionActivated(IMarketingDbContext db, IOptions<DemoOptions> demo)
    : IIntegrationEventHandler<SubscriptionActivatedIntegrationEvent>
{
    public async Task Handle(SubscriptionActivatedIntegrationEvent activated, CancellationToken ct)
    {
        await Task.Delay(demo.Value.MarketingDelayMs, ct);

        // The same event may arrive again: one referral code per subscription.
        if (await db.Coupons.AnyAsync(c => c.SubscriptionId == activated.SubscriptionId, ct))
        {
            return;
        }

        db.Coupons.Add(Coupon.IssueReferral(ReferralCode.Generate(), activated.CustomerId, activated.SubscriptionId,
            activated.ActivatedAt));
    }
}
