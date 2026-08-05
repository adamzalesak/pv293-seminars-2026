namespace Kofi.BusinessLayer.Abstractions;

public record PagedResult<T>
{
    public required List<T> Items { get; init; }
    public required int Page { get; init; }
    public required int PageSize { get; init; }
    public required int TotalCount { get; init; }
}
