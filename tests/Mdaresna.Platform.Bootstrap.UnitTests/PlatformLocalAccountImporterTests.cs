using Mdaresna.Platform.Bootstrap;
using Mdaresna.Platform.Domain.Access;
using Mdaresna.Platform.Infrastructure.Persistence;
using Mdaresna.Platform.Infrastructure.Persistence.Identity.Entities;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Xunit;

namespace Mdaresna.Platform.Bootstrap.UnitTests;

public sealed class PlatformLocalAccountImporterTests
{
    [Fact]
    public async Task Import_is_dry_run_by_default_and_never_copies_password()
    {
        var identityConnection = Environment.GetEnvironmentVariable("MDARESNA_POSTGRES_TEST_IDENTITY");
        var platformConnection = Environment.GetEnvironmentVariable("MDARESNA_POSTGRES_TEST_PLATFORM");
        if (string.IsNullOrWhiteSpace(identityConnection) ||
            string.IsNullOrWhiteSpace(platformConnection)) return;

        foreach (var connection in new[] { identityConnection, platformConnection })
        {
            var target = new NpgsqlConnectionStringBuilder(connection);
            Assert.Equal("localhost", target.Host, ignoreCase: true);
            Assert.StartsWith("mdaresna_pg_verify_", target.Database,
                StringComparison.OrdinalIgnoreCase);
        }

        await using var identity = new PostgreSqlIdentityDbContext(
            new DbContextOptionsBuilder<PostgreSqlIdentityDbContext>()
                .UseNpgsql(identityConnection, pg =>
                    pg.MigrationsHistoryTable("__EFMigrationsHistory", "identity"))
                .Options);
        await using var platform = new PostgreSqlPlatformDbContext(
            new DbContextOptionsBuilder<PostgreSqlPlatformDbContext>()
                .UseNpgsql(platformConnection, pg =>
                    pg.MigrationsHistoryTable("__EFMigrationsHistory", "platform"))
                .Options);
        await using var identityTransaction = await identity.Database.BeginTransactionAsync();
        await using var platformTransaction = await platform.Database.BeginTransactionAsync();
        var personId = Guid.NewGuid();
        var now = DateTimeOffset.UtcNow;
        var account = new Account
        {
            Id = personId,
            Status = AccountStatus.Active,
            DisplayName = "Shared Name",
            CreatedAtUtc = now,
            UpdatedAtUtc = now,
            PasswordCredential = new PasswordCredential
            {
                AccountId = personId,
                PasswordHash = "legacy-hash",
                HashingAlgorithm = "AspNetIdentityV3",
                HashingVersion = 3,
                SecurityStamp = Guid.NewGuid().ToString("N"),
                ChangedAtUtc = now
            }
        };
        identity.Accounts.Add(account);
        await identity.SaveChangesAsync();

        var accountId = IdentityAccountId.From(personId);
        var role = PlatformRole.Create(PlatformRoleId.New(),
            $"import-{personId:N}", "Import test", false,
            [PlatformPermissionCodes.SchoolsRead], accountId, now);
        platform.Roles.Add(role);
        platform.RoleAssignments.Add(PlatformRoleAssignment.Assign(
            PlatformRoleAssignmentId.New(), accountId, role.Id, accountId, now));
        await platform.SaveChangesAsync();

        var importer = new PlatformLocalAccountImporter(identity, platform);
        var preview = await importer.RunAsync(dryRun: true);
        Assert.Equal(1, preview.Imported);
        Assert.Empty(platform.LocalUsers.Local);

        var imported = await importer.RunAsync(dryRun: false);
        Assert.Equal(1, imported.Imported);
        var local = await platform.LocalUsers.Include(x => x.Credential)
            .SingleAsync(x => x.PersonId == personId);
        Assert.Equal("Shared Name", local.DisplayName);
        Assert.Equal("PendingActivation", local.Status);
        Assert.Null(local.Credential);
        var repeat = await importer.RunAsync(dryRun: false);
        Assert.Equal(1, repeat.Skipped);
        Assert.Null(local.Credential);

        await platformTransaction.RollbackAsync();
        await identityTransaction.RollbackAsync();
    }
}
