using System.ComponentModel.DataAnnotations;

namespace api.Dtos.AppUser;

public record RegisterUserDto
{
    [Required(ErrorMessage = "User name is required")]
    public required string UserName { get; init; }
    [EmailAddress]
    [Required(ErrorMessage = "Email is required")]
    public required string Email { get; init; }
    [Required(ErrorMessage = "Password is required")]
    public required string Password { get; init; }
}