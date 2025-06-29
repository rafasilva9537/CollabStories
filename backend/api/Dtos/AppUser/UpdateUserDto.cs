using System.ComponentModel.DataAnnotations;

namespace api.Dtos.AppUser;

public record UpdateUserDto
{
    public required string UserName { get; init; }
    public required string Email { get; init; }
    public required string CurrentPassword { get; init; }
    public required string NewPassword { get; init; }
    public required string Description { get; init; }
}
