using Microsoft.EntityFrameworkCore;
using api.Models;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace api.Data;

public class AppUserConfiguration : IEntityTypeConfiguration<AppUser>
{
    public void Configure(EntityTypeBuilder<AppUser> builder)
    {
        builder.ToTable("AppUser");
        builder.Property(au => au.Nickname).HasMaxLength(70);
        builder.Property(au => au.Description).HasMaxLength(200);
    }
}