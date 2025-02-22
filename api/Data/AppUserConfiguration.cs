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
            .WithOne(s => s.OwnerUser)
            .HasForeignKey(s => s.OwnerUserId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);;

        builder.HasMany(au => au.StoryParts)
            .WithOne(sp => sp.OwnerUser)
            .HasForeignKey(sp => sp.OwnerUserId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);
    }
}