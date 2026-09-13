using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Mdaresna.Platform.Infrastructure.Persistence.DesignTime;

public sealed class PostgreSqlPlatformDbContextFactory :
    IDesignTimeDbContextFactory<PostgreSqlPlatformDbContext>
{
    public PostgreSqlPlatformDbContext CreateDbContext(string[] args)
    {
        var connection = Require("MDARESNA_PLATFORM_POSTGRES_DESIGNTIME_CONNECTION");
        var options = new DbContextOptionsBuilder<PostgreSqlPlatformDbContext>()
            .UseNpgsql(connection, pg =>
                pg.MigrationsHistoryTable("__EFMigrationsHistory", "platform"))
            .Options;
        return new PostgreSqlPlatformDbContext(options);
    }

    private static string Require(string name) =>
        Environment.GetEnvironmentVariable(name) is { Length: > 0 } value
            ? value
            : throw new InvalidOperationException($"{name} is required for PostgreSQL design-time operations.");
}

public sealed class PostgreSqlIdentityDbContextFactory :
    IDesignTimeDbContextFactory<PostgreSqlIdentityDbContext>
{
    public PostgreSqlIdentityDbContext CreateDbContext(string[] args)
    {
        var connection = Environment.GetEnvironmentVariable(
            "MDARESNA_IDENTITY_POSTGRES_DESIGNTIME_CONNECTION");
        if (string.IsNullOrWhiteSpace(connection))
            throw new InvalidOperationException(
                "MDARESNA_IDENTITY_POSTGRES_DESIGNTIME_CONNECTION is required for PostgreSQL design-time operations.");
        var options = new DbContextOptionsBuilder<PostgreSqlIdentityDbContext>()
            .UseNpgsql(connection, pg =>
                pg.MigrationsHistoryTable("__EFMigrationsHistory", "identity"))
            .Options;
        return new PostgreSqlIdentityDbContext(options);
    }
}
