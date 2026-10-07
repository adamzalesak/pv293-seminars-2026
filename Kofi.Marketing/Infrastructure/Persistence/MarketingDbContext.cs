using Kofi.Common.Persistence;
using Kofi.Marketing.Application.Persistence;
using Kofi.Marketing.Domain.Coupons;
using Microsoft.EntityFrameworkCore;

namespace Kofi.Marketing.Infrastructure.Persistence;

public class MarketingDbContext(DbContextOptions<MarketingDbContext> options) : AggregateDbContext(options), IMarketingDbContext
{
    public DbSet<Coupon> Coupons => Set<Coupon>();

    protected override void ConfigureModel(ModelBuilder modelBuilder) =>
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(MarketingDbContext).Assembly);
}
