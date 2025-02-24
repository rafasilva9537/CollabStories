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

        builder.HasMany(au => au.Stories)
            .WithOne(s => s.User)
            .HasForeignKey(s => s.UserId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);;

        builder.HasMany(au => au.StoryParts)
            .WithOne(sp => sp.User)
            .HasForeignKey(sp => sp.UserId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);
    }
}