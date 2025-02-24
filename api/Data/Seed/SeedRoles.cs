using api.Constants;
using Microsoft.AspNetCore.Identity;

namespace api.Data.Seed;

public class SeedRoles {
    public static async Task InitializeAsync(IServiceProvider serviceProvider)
    {
        RoleManager<IdentityRole> _roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        IdentityRole? adminRole = await _roleManager.FindByNameAsync(RoleConstants.Admin);
        if(adminRole is null)
        {
            await _roleManager.CreateAsync(new IdentityRole(RoleConstants.Admin));
        }

        IdentityRole? userRole = await _roleManager.FindByNameAsync(RoleConstants.User);
        if(userRole is null)
        {
            await _roleManager.CreateAsync(new IdentityRole(RoleConstants.User));
        }
    }
}