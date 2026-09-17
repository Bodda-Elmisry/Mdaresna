using Microsoft.EntityFrameworkCore;
using Mdaresna.Schools.Domain.Identity;

namespace Mdaresna.Schools.Infrastructure.Persistence;

/// <summary>
/// Base context for one school's isolated operational database. It is intentionally not registered
/// against a single application-wide connection string; the authenticated school scope will select
/// the database when the persistence routing phase is implemented.
/// </summary>
public class SchoolsDbContext(DbContextOptions options) : DbContext(options)
{
    public DbSet<Person> Persons => Set<Person>();
    public DbSet<PersonContact> PersonContacts => Set<PersonContact>();
    public DbSet<LocalUserAccount> LocalUsers => Set<LocalUserAccount>();
    public DbSet<LocalUserCredential> LocalUserCredentials => Set<LocalUserCredential>();
    public DbSet<LocalUserSession> LocalUserSessions => Set<LocalUserSession>();
    public DbSet<LocalRole> LocalRoles => Set<LocalRole>();
    public DbSet<LocalPermission> LocalPermissions => Set<LocalPermission>();
    public DbSet<LocalRolePermission> LocalRolePermissions => Set<LocalRolePermission>();
    public DbSet<LocalUserRole> LocalUserRoles => Set<LocalUserRole>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasDefaultSchema("school");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(SchoolsDbContext).Assembly);
    }
}

public sealed class PostgreSqlSchoolsDbContext(
    DbContextOptions<PostgreSqlSchoolsDbContext> options) : SchoolsDbContext(options);

public sealed class SqlServerSchoolsDbContext(
    DbContextOptions<SqlServerSchoolsDbContext> options) : SchoolsDbContext(options);
