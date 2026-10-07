using Kofi.Marketing.Domain.Coupons;
using Microsoft.EntityFrameworkCore;

namespace Kofi.Marketing.Application.Persistence;

public interface IMarketingDbContext
{
    DbSet<Coupon> Coupons { get; }
}
