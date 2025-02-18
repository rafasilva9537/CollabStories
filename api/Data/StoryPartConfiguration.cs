using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using api.Models;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace api.Data
{
    public class StoryPartConfiguration : IEntityTypeConfiguration<StoryPart>
    {
        public void Configure(EntityTypeBuilder<StoryPart> builder)
        {
            builder.ToTable("StoryPart");
            builder.HasKey(sp => sp.Id);
            builder.Property(sp => sp.Text).HasMaxLength(4000);
            builder.Property(sp => sp.CreatedDate).HasDefaultValueSql("GetUtcDate()");
        }
    }
}