namespace api.Dtos.StoryPart
{
    public class StoryPartInListDto
    {
        public int Id { get; set; }
        public required string Text { get; set; }
        public DateTimeOffset CreatedDate { get; set; }
        public string? UserName { get; set; }
    }
}