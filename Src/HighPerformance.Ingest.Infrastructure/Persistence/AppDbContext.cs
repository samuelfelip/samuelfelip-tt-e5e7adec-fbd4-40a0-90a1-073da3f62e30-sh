using HighPerformance.Ingest.Application.Abstractions;
using HighPerformance.Ingest.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace HighPerformance.Ingest.Infrastructure.Persistence;

public sealed class AppDbContext : DbContext, IApplicationDbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<ScoreEntry> ScoreEntries => Set<ScoreEntry>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var isNpgsql = Database.ProviderName?.Contains("Npgsql", StringComparison.OrdinalIgnoreCase) == true;

        modelBuilder.Entity<ScoreEntry>(entity =>
        {
            entity.ToTable("ScoreEntries");
            entity.HasKey(s => s.Id);
            entity.Property(s => s.UserId).HasMaxLength(200).IsRequired();
            entity.Property(s => s.Score).IsRequired();
            entity.Property(s => s.Timestamp).IsRequired();
            entity.HasIndex(s => new { s.UserId, s.Timestamp }).HasDatabaseName("IX_ScoreEntries_UserId_Timestamp");

            if (isNpgsql)
                entity.Property(s => s.Version).IsRowVersion().HasColumnName("xmin");
            else
                entity.Ignore(s => s.Version);
        });
    }
}
