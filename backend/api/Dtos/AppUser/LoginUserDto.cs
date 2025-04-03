using System.ComponentModel.DataAnnotations;

namespace api.Dtos.AppUser;

public class LoginUserDto
{
    [Required(ErrorMessage = "User name is required")]
    public required string UserName { get; set; }
    [Required(ErrorMessage = "Password is required")]
    public required string Password { get; set; }
}