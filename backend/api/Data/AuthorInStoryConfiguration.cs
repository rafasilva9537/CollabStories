using Microsoft.EntityFrameworkCore;
using api.Models;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace api.Data;

public class AuthorInStoryConfiguration : IEntityTypeConfiguration<AuthorInStory>
{
    public void Configure(EntityTypeBuilder<AuthorInStory> builder)
    {
        builder.ToTable("AuthorInStory");

        builder.HasKey(ais => new { ais.AuthorId, ais.StoryId });
        builder.HasOne(ais => ais.Author)
            .WithMany(a => a.AuthorInStory)
            .HasForeignKey(ais => ais.AuthorId);
        builder.HasOne(ais => ais.Story)
            .WithMany(s => s.AuthorsInStory)
            .HasForeignKey(ais => ais.StoryId);
        
        builder.Property(ais => ais.EntryDate).IsRequired();
    }
}