namespace Kofi.Common.Demo;

// Switches for the classroom demo, section "Demo" in appsettings. Both are
// off unless configured.
public class DemoOptions
{
    // How long the referral coupon handler waits before it issues the coupon.
    public int MarketingDelayMs { get; init; }

    // Makes the popularity handler in Catalog fail.
    public bool PopularityFails { get; init; }
}
