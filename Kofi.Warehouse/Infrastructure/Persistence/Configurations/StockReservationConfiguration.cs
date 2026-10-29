using Kofi.Warehouse.Domain.Reservations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kofi.Warehouse.Infrastructure.Persistence.Configurations;

public class StockReservationConfiguration : IEntityTypeConfiguration<StockReservation>
{
    public void Configure(EntityTypeBuilder<StockReservation> reservation)
    {
        reservation.ToTable("StockReservations", "warehouse");
        reservation.HasKey(r => r.Id);
        reservation.Property(r => r.Id).HasColumnName("DeliveryId").ValueGeneratedNever();
        reservation.Ignore(r => r.DeliveryId);
        reservation.Ignore(r => r.DomainEvents);
        reservation.OwnsMany(r => r.Lines, line =>
        {
            line.ToTable("StockReservationLines", "warehouse");
            line.WithOwner().HasForeignKey("DeliveryId");
            line.Property<int>("Id");
            line.HasKey("Id");
        });
        reservation.Navigation(r => r.Lines).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
