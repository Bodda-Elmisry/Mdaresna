using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Mdaresna.Schools.Infrastructure.Persistence;

public sealed class PostgreSqlSchoolsDbContextFactory : IDesignTimeDbContextFactory<PostgreSqlSchoolsDbContext>
{
    public PostgreSqlSchoolsDbContext CreateDbContext(string[] args)
    {
        var connection = Environment.GetEnvironmentVariable("MDARESNA_SCHOOLS_POSTGRES_DESIGNTIME_CONNECTION");
        if (string.IsNullOrWhiteSpace(connection))
            throw new InvalidOperationException("MDARESNA_SCHOOLS_POSTGRES_DESIGNTIME_CONNECTION is required for PostgreSQL design-time operations.");
        return new PostgreSqlSchoolsDbContext(new DbContextOptionsBuilder<PostgreSqlSchoolsDbContext>().UseNpgsql(connection).Options);
    }
}

public sealed class SqlServerSchoolsDbContextFactory : IDesignTimeDbContextFactory<SqlServerSchoolsDbContext>
{
    public SqlServerSchoolsDbContext CreateDbContext(string[] args)
    {
        var connection = Environment.GetEnvironmentVariable("MDARESNA_SCHOOLS_SQLSERVER_DESIGNTIME_CONNECTION");
        if (string.IsNullOrWhiteSpace(connection))
            throw new InvalidOperationException("MDARESNA_SCHOOLS_SQLSERVER_DESIGNTIME_CONNECTION is required for SQL Server design-time operations.");
        return new SqlServerSchoolsDbContext(new DbContextOptionsBuilder<SqlServerSchoolsDbContext>().UseSqlServer(connection).Options);
    }
}
