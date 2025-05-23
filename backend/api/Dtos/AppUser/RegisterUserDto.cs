using System.ComponentModel.DataAnnotations;

namespace api.Dtos.AppUser;

public class RegisterUserDto
{
    [Required(ErrorMessage = "User name is required")]
    public required string UserName { get; set; }
    [EmailAddress]
    [Required(ErrorMessage = "Email is required")]
    public required string Email { get; set; }
    [Required(ErrorMessage = "Password is required")]
    public required string Password { get; set; }
}