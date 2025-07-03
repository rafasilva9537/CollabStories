namespace api.Dtos.AppUser;

public record UserMainInfoDto
{
    // TODO: remove exposed id
    public int Id { get; init; }
    public required string Nickname { get; init; }
    public required string UserName { get; init; }
    public required DateTimeOffset CreatedDate { get; init; }
}