using Mdaresna.Doamin.Models.ReportingManagement;
using Mdaresna.Doamin.ModelsConfigrations.ReportingManagement;
using Microsoft.EntityFrameworkCore;

namespace Mdaresna.Infrastructure.Data;

public class SchoolReportDBContext : DbContext
{
    public SchoolReportDBContext(DbContextOptions<SchoolReportDBContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new StudentReportConfig());
        modelBuilder.RegisterUtcTimeZoneConverters();
    }

    public DbSet<StudentReport> StudentReports { get; set; }

    /// <summary>
    /// Creates a report database context from a connection string supplied by the caller.
    /// </summary>
    public static SchoolReportDBContext Create(string connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new ArgumentException("Connection string is required.", nameof(connectionString));
        }

        var options = new DbContextOptionsBuilder<SchoolReportDBContext>()
            .UseSqlServer(
                connectionString,
                sqlOptions => sqlOptions.MigrationsAssembly(typeof(SchoolReportDBContext).Assembly.FullName))
            .Options;

        return new SchoolReportDBContext(options);
    }

    /// <summary>
    /// Creates a report database context and ensures the target database is created and migrated.
    /// </summary>
    /// <remarks>
    /// Business: report databases are created per external connection string. Calling this
    /// method before writing reports guarantees that a missing database is created and all
    /// pending report database migrations are applied.
    /// </remarks>
    public static async Task<SchoolReportDBContext> CreateAndMigrateAsync(
        string connectionString,
        CancellationToken cancellationToken = default)
    {
        var context = Create(connectionString);

        try
        {
            await context.Database.MigrateAsync(cancellationToken);
            return context;
        }
        catch
        {
            await context.DisposeAsync();
            throw;
        }
    }

    /// <summary>
    /// Ensures the target report database exists and has all pending migrations applied.
    /// </summary>
    /// <remarks>
    /// Business: use this method when the caller only needs to prepare the report database and
    /// does not need to keep a context instance open afterwards.
    /// </remarks>
    public static async Task EnsureDatabaseMigratedAsync(
        string connectionString,
        CancellationToken cancellationToken = default)
    {
        await using var context = Create(connectionString);
        await context.Database.MigrateAsync(cancellationToken);
    }

    public override int SaveChanges()
    {
        this.ConvertAllDatesToUtc();
        return base.SaveChanges();
    }

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        this.ConvertAllDatesToUtc();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        this.ConvertAllDatesToUtc();
        return base.SaveChangesAsync(cancellationToken);
    }

    public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        this.ConvertAllDatesToUtc();
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }
}

