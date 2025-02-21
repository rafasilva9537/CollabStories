using System.ComponentModel.DataAnnotations;

namespace api.Dtos.AppUser;

public class UpdateUserDto
{
    public required string UserName { get; set; }
    public required string Email { get; set; }
    public required string CurrentPassword { get; set; }
    public required string NewPassword { get; set; }
    public required string Description { get; set; }
}
