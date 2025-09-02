namespace api.Dtos.AppUser;

public record UserMainInfoDto
{
    public required string Nickname { get; init; }
    public required string UserName { get; init; }
    public required DateTimeOffset CreatedDate { get; init; }
}