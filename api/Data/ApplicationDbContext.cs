using Microsoft.EntityFrameworkCore;
using api.Models;
using System.Reflection;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace api.Data;
 
public class ApplicationDbContext : IdentityDbContext<AppUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> dbContextoptions) : base(dbContextoptions)
    {
    }

    public DbSet<Story> Story { get; set; }
    public DbSet<StoryPart> StoryPart { get; set; }
    public DbSet<AppUser> AppUser { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}