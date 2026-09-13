using Mdaresna.Platform.Infrastructure.Persistence.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Mdaresna.Platform.Infrastructure.Persistence.DesignTime;

public sealed class IdentityDbContextFactory : IDesignTimeDbContextFactory<IdentityDbContext>
{
    public IdentityDbContext CreateDbContext(string[] args)
    {
        var identityConnection = DesignTimeConnectionStrings.Identity;
        SqlDatabaseTargetValidator.EnsureDifferent(
            DesignTimeConnectionStrings.Platform,
            identityConnection);

        var options = new DbContextOptionsBuilder<IdentityDbContext>()
            .UseSqlServer(
                identityConnection,
                sql =>
                {
                    sql.MigrationsAssembly(typeof(IdentityDbContextFactory).Assembly.FullName);
                    sql.MigrationsHistoryTable("__EFMigrationsHistory", "identity");
                })
            .Options;

        return new IdentityDbContext(options);
    }
}
