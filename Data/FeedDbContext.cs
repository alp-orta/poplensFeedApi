using Microsoft.EntityFrameworkCore;
using poplensFeedApi.Models;

namespace poplensFeedApi.Data {
    public class FeedDbContext : DbContext {
        public FeedDbContext(DbContextOptions<FeedDbContext> options) : base(options) { }

        public DbSet<DisplayedReview> DisplayedReviews { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder) {
            modelBuilder.HasDefaultSchema("public");
            base.OnModelCreating(modelBuilder);

            // Configure the DisplayedReview entity
            modelBuilder.Entity<DisplayedReview>(entity => {

                entity.ToTable("DisplayedReviews");
                entity.HasKey(e => e.Id);
                
                entity.Property(e => e.Id)
                    .HasDefaultValueSql("gen_random_uuid()"); // PostgreSQL UUID generation

                entity.Property(e => e.ProfileId)
                    .IsRequired();
                
                entity.Property(e => e.ReviewId)
                    .IsRequired();
                
                entity.Property(e => e.DisplayedAt)
                    .IsRequired()
                    .HasDefaultValueSql("NOW()"); // Default value in PostgreSQL

                // Create a unique index on ProfileId and ReviewId to prevent duplicates
                entity.HasIndex(e => new { e.ProfileId, e.ReviewId })
                    .IsUnique();
            });
        }
    }
}
