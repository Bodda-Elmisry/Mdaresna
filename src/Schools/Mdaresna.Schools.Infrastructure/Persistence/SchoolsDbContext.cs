using Microsoft.EntityFrameworkCore;
using Mdaresna.Schools.Domain.Identity;
using Mdaresna.Schools.Domain.School;

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
    public DbSet<LocalUserActivationChallenge> LocalUserActivationChallenges => Set<LocalUserActivationChallenge>();
    public DbSet<LocalUserSession> LocalUserSessions => Set<LocalUserSession>();
    public DbSet<LocalRole> LocalRoles => Set<LocalRole>();
    public DbSet<LocalPermission> LocalPermissions => Set<LocalPermission>();
    public DbSet<LocalRolePermission> LocalRolePermissions => Set<LocalRolePermission>();
    public DbSet<LocalUserRole> LocalUserRoles => Set<LocalUserRole>();
    public DbSet<SchoolInformation> SchoolInformation => Set<SchoolInformation>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasDefaultSchema("school");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(SchoolsDbContext).Assembly);
    }
}

public sealed class PostgreSqlSchoolsDbContext(
    DbContextOptions<PostgreSqlSchoolsDbContext> options) : SchoolsDbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        ConfigurePostgreSqlConcurrency<Person>(modelBuilder, nameof(Person.RowVersion));
        ConfigurePostgreSqlConcurrency<LocalUserAccount>(modelBuilder, nameof(LocalUserAccount.RowVersion));
        ConfigurePostgreSqlConcurrency<LocalUserCredential>(modelBuilder, nameof(LocalUserCredential.RowVersion));
        ConfigurePostgreSqlConcurrency<LocalRole>(modelBuilder, nameof(LocalRole.RowVersion));
        ConfigurePostgreSqlConcurrency<Mdaresna.Schools.Domain.School.SchoolInformation>(
            modelBuilder, nameof(Mdaresna.Schools.Domain.School.SchoolInformation.RowVersion));
    }

    private static void ConfigurePostgreSqlConcurrency<TEntity>(ModelBuilder modelBuilder, string rowVersionProperty)
        where TEntity : class
    {
        var entity = modelBuilder.Entity<TEntity>();
        entity.Ignore(rowVersionProperty);
        entity.Property<uint>("xmin").HasColumnName("xmin").IsRowVersion();
    }
}

public sealed class SqlServerSchoolsDbContext(
    DbContextOptions<SqlServerSchoolsDbContext> options) : SchoolsDbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Person>().Property(x => x.RowVersion).IsRowVersion();
        modelBuilder.Entity<LocalUserAccount>().Property(x => x.RowVersion).IsRowVersion();
        modelBuilder.Entity<LocalUserCredential>().Property(x => x.RowVersion).IsRowVersion();
        modelBuilder.Entity<LocalRole>().Property(x => x.RowVersion).IsRowVersion();
        modelBuilder.Entity<Mdaresna.Schools.Domain.School.SchoolInformation>()
            .Property(x => x.RowVersion).IsRowVersion();
    }
}
