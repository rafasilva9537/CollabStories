using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace api.Models;
public class AppUser : IdentityUser
{
    public string Nickname { get; set; } = String.Empty;
    public DateTimeOffset CreatedDate { get; set; } = DateTimeOffset.UtcNow;
    public string Description { get; set; } = String.Empty;
}