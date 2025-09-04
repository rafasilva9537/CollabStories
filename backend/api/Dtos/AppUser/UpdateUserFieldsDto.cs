namespace api.Dtos.AppUser;

public record UpdateUserFieldsDto
{
    public string? UserName { get; init; }
    
    public string? NickName { get; init; }
    public string? Email { get; init; }
    public string? Description { get; init; }
}