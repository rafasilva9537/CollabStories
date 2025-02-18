using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace api.Data
{
    public class StoryConfiguration : IEntityTypeConfiguration<Story>
    {
        public void Configure(EntityTypeBuilder<Story> builder)
        {
            builder.ToTable("Story");
            builder.HasKey(s => s.Id);
            builder.Property(s => s.Title).HasMaxLength(90);
            builder.Property(s => s.Description).HasMaxLength(200);
            builder.Property(s => s.CreatedDate).HasDefaultValueSql("GetUtcDate()");
            builder.Property(s => s.UpdatedDate).HasDefaultValueSql("GetUtcDate()");;
            builder.Property(s => s.MaximumAuthors).HasDefaultValue(6);
            builder.Property(s => s.TurnDurationSeconds).HasDefaultValue(300);
        }
    }
}