using Mdaresna.Schools.Application.Identity;
using Mdaresna.Schools.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Mdaresna.Schools.Infrastructure.Identity;

public interface ISchoolDbContextFactory
{
    Task<SchoolLoginTarget?> ResolveAsync(string schoolCode, CancellationToken cancellationToken = default);
    Task<SchoolsDbContext?> CreateAsync(string schoolCode, CancellationToken cancellationToken = default);
}

internal sealed class SchoolDbContextFactory(ISchoolLoginTenantResolver resolver) : ISchoolDbContextFactory
{
    public Task<SchoolLoginTarget?> ResolveAsync(string schoolCode, CancellationToken cancellationToken = default) =>
        resolver.ResolveAsync(schoolCode, cancellationToken);

    public async Task<SchoolsDbContext?> CreateAsync(string schoolCode, CancellationToken cancellationToken = default)
    {
        var target = await resolver.ResolveAsync(schoolCode, cancellationToken);
        if (target is null) return null;
        if (target.Provider.Equals("PostgreSql", StringComparison.OrdinalIgnoreCase))
            return new PostgreSqlSchoolsDbContext(new DbContextOptionsBuilder<PostgreSqlSchoolsDbContext>()
                .UseNpgsql(target.ConnectionString).Options);
        if (target.Provider.Equals("SqlServer", StringComparison.OrdinalIgnoreCase))
            return new SqlServerSchoolsDbContext(new DbContextOptionsBuilder<SqlServerSchoolsDbContext>()
                .UseSqlServer(target.ConnectionString).Options);
        throw new InvalidOperationException($"Unsupported school database provider '{target.Provider}'.");
    }
}
