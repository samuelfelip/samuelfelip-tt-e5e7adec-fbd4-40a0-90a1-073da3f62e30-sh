using HighPerformance.Ingest.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace HighPerformance.Ingest.Application.Abstractions;

public interface IApplicationDbContext
{
    DbSet<ScoreEntry> ScoreEntries { get; }
    DatabaseFacade Database { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
