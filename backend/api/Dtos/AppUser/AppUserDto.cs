using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace api.Dtos.AppUser;

public record AppUserDto
{
    public required string Nickname { get; init; }
    public required string UserName { get; init; }
    public required string Email { get; init; }
    public required DateTimeOffset CreatedDate { get; init; }
    public required string Description { get; init; }
}
