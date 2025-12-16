using Convoy.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Convoy.Data.Context;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Location> Locations { get; set; }
    public DbSet<DailySummary> DailySummaries { get; set; }
    public DbSet<HourlySummary> HourlySummaries { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // User configuration
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Phone).IsRequired().HasMaxLength(20);
            entity.HasIndex(e => e.Phone).IsUnique();
        });

        // Location configuration
        modelBuilder.Entity<Location>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Latitude).IsRequired();
            entity.Property(e => e.Longitude).IsRequired();
            entity.Property(e => e.Timestamp).IsRequired();

            // User bilan relationship
            entity.HasOne(e => e.User)
                  .WithMany(u => u.Locations)
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Cascade);

            // Index tezlik uchun
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.Timestamp);
        });

        // DailySummary configuration
        modelBuilder.Entity<DailySummary>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Date).IsRequired();
            entity.Property(e => e.TotalLocations).HasDefaultValue(0);
            entity.Property(e => e.TotalDistanceKm).HasDefaultValue(0);

            // User bilan relationship
            entity.HasOne(e => e.User)
                  .WithMany(u => u.DailySummaries)
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Cascade);

            // Index tezlik uchun
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.Date);
            entity.HasIndex(e => new { e.UserId, e.Date }).IsUnique(); // Bir user uchun bir kun bitta summary
        });

        // HourlySummary configuration
        modelBuilder.Entity<HourlySummary>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Hour).IsRequired();
            entity.Property(e => e.LocationCount).HasDefaultValue(0);
            entity.Property(e => e.DistanceKm).HasDefaultValue(0);

            // DailySummary bilan relationship
            entity.HasOne(e => e.DailySummary)
                  .WithMany(d => d.HourlySummaries)
                  .HasForeignKey(e => e.DailySummaryId)
                  .OnDelete(DeleteBehavior.Cascade);

            // User bilan relationship
            entity.HasOne(e => e.User)
                  .WithMany(u => u.HourlySummaries)
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Cascade);

            // Index tezlik uchun
            entity.HasIndex(e => e.DailySummaryId);
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => new { e.DailySummaryId, e.Hour }).IsUnique(); // Bir summary uchun bir soat bitta entry
        });
    }
}
