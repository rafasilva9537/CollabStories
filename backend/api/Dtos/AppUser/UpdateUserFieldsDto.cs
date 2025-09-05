using System.ComponentModel.DataAnnotations;

namespace api.Dtos.AppUser;

public record UpdateUserFieldsDto
{
    [MinLength(1, ErrorMessage = "UserName should be at least 1 character long")]
    public string? UserName { get; init; }
    
    [MinLength(1, ErrorMessage = "NickName should be at least 1 character long")]
    public string? NickName { get; init; }
    
    [EmailAddress]
    public string? Email { get; init; }
    
    public string? Description { get; init; }
}