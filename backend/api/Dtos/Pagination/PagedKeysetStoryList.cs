namespace api.Dtos.Pagination;

public class PagedKeysetStoryList<T> where T : class
{
    public required List<T> Items { get; init; }
    public required int? NextId { get; init; }
    public required bool HasMore { get; init; }
}