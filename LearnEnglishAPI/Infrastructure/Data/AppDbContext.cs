// IngilizceProje/Infrastructure/Data/AppDbContext.cs
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using IngilizceProje.Domain.Entities;

namespace IngilizceProje.Infrastructure.Data
{
    public class AppDbContext : IdentityDbContext<AppUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Word> Words { get; set; } = null!;
        public DbSet<QuizResult> QuizResults { get; set; } = null!;
        public DbSet<ReviewHistory> ReviewHistories { get; set; } = null!;
        public DbSet<DailyGoal> DailyGoals { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // AppUser configurations
            modelBuilder.Entity<AppUser>(entity =>
            {
                entity.HasMany(u => u.Words)
                      .WithOne(w => w.User)
                      .HasForeignKey(w => w.UserId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(u => u.QuizResults)
                      .WithOne(qr => qr.User)
                      .HasForeignKey(qr => qr.UserId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(u => u.ReviewHistories)
                      .WithOne(rh => rh.User)
                      .HasForeignKey(rh => rh.UserId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(u => u.DailyGoals)
                      .WithOne(dg => dg.User)
                      .HasForeignKey(dg => dg.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Word configurations
            modelBuilder.Entity<Word>(entity =>
            {
                entity.HasKey(w => w.Id);
                entity.Property(w => w.EnglishWord).IsRequired().HasMaxLength(150);
                entity.Property(w => w.TurkishMeaning).IsRequired().HasMaxLength(250);
                entity.Property(w => w.ExampleSentence).HasMaxLength(1000);
                entity.Property(w => w.ExampleSentenceTranslation).HasMaxLength(1000);
                entity.Property(w => w.CreatedBy).IsRequired().HasMaxLength(256);
                entity.Property(w => w.UpdatedBy).HasMaxLength(256);

                entity.HasMany(w => w.QuizResults)
                      .WithOne(qr => qr.Word)
                      .HasForeignKey(qr => qr.WordId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasMany(w => w.ReviewHistories)
                      .WithOne(rh => rh.Word)
                      .HasForeignKey(rh => rh.WordId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // QuizResult configurations
            modelBuilder.Entity<QuizResult>(entity =>
            {
                entity.HasKey(qr => qr.Id);
                entity.Property(qr => qr.UserId).IsRequired();
            });

            // ReviewHistory configurations
            modelBuilder.Entity<ReviewHistory>(entity =>
            {
                entity.HasKey(rh => rh.Id);
                entity.Property(rh => rh.UserId).IsRequired();
            });

            // DailyGoal configurations
            modelBuilder.Entity<DailyGoal>(entity =>
            {
                entity.HasKey(dg => dg.Id);
                entity.Property(dg => dg.UserId).IsRequired();
            });
        }
    }
}
