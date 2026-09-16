using Microsoft.EntityFrameworkCore;

namespace Mdaresna.Schools.Infrastructure.Persistence;

/// <summary>
/// Base context for one school's isolated operational database. It is intentionally not registered
/// against a single application-wide connection string; the authenticated school scope will select
/// the database when the persistence routing phase is implemented.
/// </summary>
public class SchoolsDbContext(DbContextOptions options) : DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasDefaultSchema("school");
    }
}

public sealed class PostgreSqlSchoolsDbContext(
    DbContextOptions<PostgreSqlSchoolsDbContext> options) : SchoolsDbContext(options);
