namespace api.Dtos.AppUser;

public class UserMainInfoDto
{
    public required string Nickname { get; set; }
    public required string UserName { get; set; }
    public required DateTimeOffset CreatedDate { get; set; }
}