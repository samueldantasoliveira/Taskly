public class PagedResult<T>
{
    public IReadOnlyCollection<T> Items { get; init; }= [];
    public long TotalCount { get; init; }
}