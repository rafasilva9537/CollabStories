using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using api.Models;

namespace api.Data
{
    public class ApplicationDBContext : DbContext
    {
        public ApplicationDBContext(DbContextOptions<ApplicationDBContext> dbContextoptions) : base(dbContextoptions)
        {
        }

        public DbSet<Story> Story { get; set; }
        public DbSet<StoryPart> StoryPart { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Story Model Builders
            modelBuilder.Entity<Story>()
                .Property(s => s.MaximumAuthors)
                .HasDefaultValue(6);
            modelBuilder.Entity<Story>()
                .Property(s => s.TurnDurationSeconds)
                .HasDefaultValue(300);

            // StoryPart Model Builders   
        }
    }
}