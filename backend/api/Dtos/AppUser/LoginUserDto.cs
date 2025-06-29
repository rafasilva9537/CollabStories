using System.ComponentModel.DataAnnotations;

namespace api.Dtos.AppUser;

public record LoginUserDto
{
    [Required(ErrorMessage = "User name is required")]
    public required string UserName { get; init; }
    [Required(ErrorMessage = "Password is required")]
    public required string Password { get; init; }
}