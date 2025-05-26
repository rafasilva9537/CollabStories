using api.Constants;
using Microsoft.AspNetCore.Identity;

namespace api.Data.Seed;

public class SeedRoles
{
    public static async Task InitializeAsync(RoleManager<IdentityRole<int>> roleManager)
    {
        IdentityRole<int>? adminRole = await roleManager.FindByNameAsync(RoleConstants.Admin);
        if (adminRole is null)
        {
            await roleManager.CreateAsync(new IdentityRole<int>(RoleConstants.Admin));
        }

        IdentityRole<int>? userRole = await roleManager.FindByNameAsync(RoleConstants.User);
        if (userRole is null)
        {
            await roleManager.CreateAsync(new IdentityRole<int>(RoleConstants.User));
        }
    }
}