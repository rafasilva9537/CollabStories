using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace api.Dtos.AppUser;

public class AppUserDto
{
    public required string Nickname { get; set; }
    public required string UserName { get; set; }
    public required string Email { get; set; }
    public required DateTimeOffset CreatedDate { get; set; }
    public required string Description { get; set; }
}
