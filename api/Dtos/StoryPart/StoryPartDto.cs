namespace api.Dtos.StoryPart
{
    public class StoryPartDto
    {
        public int Id { get; set; }
        public required string Text { get; set; }
        public DateTimeOffset CreatedDate { get; set; }
        public int StoryId { get; set; }
    }
}