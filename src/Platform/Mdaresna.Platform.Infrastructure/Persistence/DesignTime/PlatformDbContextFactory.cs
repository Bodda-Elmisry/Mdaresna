using Mdaresna.Platform.Infrastructure.Persistence.Platform;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Mdaresna.Platform.Infrastructure.Persistence.DesignTime;

public sealed class PlatformDbContextFactory : IDesignTimeDbContextFactory<PlatformDbContext>
{
    public PlatformDbContext CreateDbContext(string[] args)
    {
        var platformConnection = DesignTimeConnectionStrings.Platform;
        SqlDatabaseTargetValidator.EnsureDifferent(
            platformConnection,
            DesignTimeConnectionStrings.Identity);

        var options = new DbContextOptionsBuilder<PlatformDbContext>()
            .UseSqlServer(
                platformConnection,
                sql =>
                {
                    sql.MigrationsAssembly(typeof(PlatformDbContextFactory).Assembly.FullName);
                    sql.MigrationsHistoryTable("__EFMigrationsHistory", "platform");
                })
            .Options;

        return new PlatformDbContext(options);
    }
}
