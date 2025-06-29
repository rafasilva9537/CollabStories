namespace api.Dtos.StoryPart
{
    public record StoryPartDto
    {
        public int Id { get; init; }
        public required string Text { get; init; }
        public DateTimeOffset CreatedDate { get; init; }
        public int StoryId { get; init; }
        public string? UserName { get; init; }
    }
}