namespace Kofi.Warehouse.Domain.Reservations;

// The goods put aside for one subscription delivery, kept so that giving
// them back happens exactly once.
public class StockReservation : AggregateRoot
{
    private readonly List<ReservedLine> _lines = new();

    private StockReservation()
    {
    }

    public Guid DeliveryId => Id;
    public bool IsReleased { get; private set; }
    public IReadOnlyList<ReservedLine> Lines => _lines.AsReadOnly();

    public static StockReservation Create(Guid deliveryId, IEnumerable<ReservedLine> lines)
    {
        var reservation = new StockReservation { Id = deliveryId };
        reservation._lines.AddRange(lines);
        return reservation;
    }

    // Returns the lines to put back, or none when they were already put back.
    public IReadOnlyList<ReservedLine> Release()
    {
        if (IsReleased)
        {
            return [];
        }

        IsReleased = true;
        return Lines;
    }
}
