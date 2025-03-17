using api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace api.Data;

public class StoryConfiguration : IEntityTypeConfiguration<Story>
{
    public void Configure(EntityTypeBuilder<Story> builder)
    {
        builder.ToTable("Story");
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Title).HasMaxLength(90).IsRequired();
        builder.Property(s => s.Description).HasMaxLength(200);
        builder.Property(s => s.CreatedDate).HasDefaultValueSql("GetUtcDate()");
        builder.Property(s => s.UpdatedDate).HasDefaultValueSql("GetUtcDate()");;
        builder.Property(s => s.MaximumAuthors).HasDefaultValue(6);
        builder.Property(s => s.TurnDurationSeconds).HasDefaultValue(300);

        builder.HasOne(s => s.User)
            .WithMany(au => au.Stories)
            .HasForeignKey(s => s.UserId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);
    }
}