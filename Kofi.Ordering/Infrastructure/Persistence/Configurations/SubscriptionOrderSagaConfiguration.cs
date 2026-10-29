using Kofi.Ordering.Application.Sagas;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kofi.Ordering.Infrastructure.Persistence.Configurations;

// The saga's state lives in Ordering's schema, next to the orders it places.
public class SubscriptionOrderSagaConfiguration : IEntityTypeConfiguration<SubscriptionOrderSaga>
{
    public void Configure(EntityTypeBuilder<SubscriptionOrderSaga> saga)
    {
        saga.ToTable("SubscriptionOrderSagas", "ordering");
        saga.HasKey(s => s.Id);
        saga.Property(s => s.Id).HasColumnName("DeliveryId").ValueGeneratedNever();
        saga.Property(s => s.Step).HasConversion<string>();
        saga.Property(s => s.Version).IsConcurrencyToken();
        saga.OwnsMany(s => s.Lines, line => line.ToJson());
    }
}
