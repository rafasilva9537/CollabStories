namespace api.Dtos.Pagination;

public sealed record PagedKeysetUserList<T> where T : class
{
    public required List<T> Items { get; init; }
    public required DateTimeOffset? NextDate { get; init; }
    public required string? NextUserName { get; init; }
    public required bool HasMore { get; init; }
}