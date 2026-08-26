namespace Kofi.Domain.Shared;

// How many units of something. There is no such thing as zero or minus two
// packs, so the constructor refuses them and nothing that receives a
// Quantity has to check it again. Upper limits are not the quantity's own
// rule: they belong to whoever accepts it.
public record Quantity
{
    public Quantity(int value)
    {
        if (value <= 0)
        {
            throw new DomainRuleException("A quantity is at least one.");
        }

        Value = value;
    }

    public int Value { get; }

    public static Quantity operator +(Quantity left, Quantity right) => new(left.Value + right.Value);

    public override string ToString() => Value.ToString();
}
