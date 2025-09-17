using System.ComponentModel.DataAnnotations;

namespace api.Dtos.AppUser;

public record ChangePasswordDto
{
    [Required]
    public required string CurrentPassword { get; init; }

    [Required]
    public required string NewPassword { get; init; }
}