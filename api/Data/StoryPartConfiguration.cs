using Microsoft.EntityFrameworkCore;
using api.Models;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace api.Data;

public class StoryPartConfiguration : IEntityTypeConfiguration<StoryPart>
{
    public void Configure(EntityTypeBuilder<StoryPart> builder)
    {
        builder.ToTable("StoryPart");
        builder.HasKey(sp => sp.Id);
        builder.Property(sp => sp.Text).HasMaxLength(4000);
        builder.Property(sp => sp.CreatedDate).HasDefaultValueSql("GetUtcDate()");

        builder.HasOne(sp => sp.User)
            .WithMany(au => au.StoryParts)
            .HasForeignKey(sp => sp.UserId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);
    }
}