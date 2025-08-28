namespace api.Dtos.Pagination;

public sealed record PagedKeysetList<T>
{
    public required List<T> Items { get; init; }
    public required DateTimeOffset? NextDate { get; init; }
    public required string? NextUserName { get; init; }
    public required bool HasMore { get; init; }
}