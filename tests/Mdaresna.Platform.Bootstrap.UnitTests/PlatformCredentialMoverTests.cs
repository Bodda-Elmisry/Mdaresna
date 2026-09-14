using Mdaresna.Platform.Bootstrap;
using Mdaresna.Platform.Domain.Access;
using Mdaresna.Platform.Infrastructure.IdentityAuth;
using Mdaresna.Platform.Infrastructure.Persistence;
using Mdaresna.Platform.Infrastructure.Persistence.Identity.Entities;
using Mdaresna.Platform.Infrastructure.Persistence.Platform.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Xunit;

namespace Mdaresna.Platform.Bootstrap.UnitTests;

public sealed class PlatformCredentialMoverTests
{
    [Fact]
    public async Task Move_preserves_login_then_removes_identity_credential()
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
                    pg.MigrationsHistoryTable("__EFMigrationsHistory", "identity")).Options);
        await using var platform = new PostgreSqlPlatformDbContext(
            new DbContextOptionsBuilder<PostgreSqlPlatformDbContext>()
                .UseNpgsql(platformConnection, pg =>
                    pg.MigrationsHistoryTable("__EFMigrationsHistory", "platform")).Options);
        await using var identityTransaction = await identity.Database.BeginTransactionAsync();
        await using var platformTransaction = await platform.Database.BeginTransactionAsync();
        var personId = Guid.NewGuid();
        var now = DateTimeOffset.UtcNow;
        var phone = "00967" + Random.Shared.NextInt64(100000000, 999999999).ToString();
        const string password = "mover-test-password-147852369";
        var account = new Account
        {
            Id = personId, Status = AccountStatus.Active,
            DisplayName = "Operator", CreatedAtUtc = now, UpdatedAtUtc = now
        };
        var hasher = new PasswordHasher<Account>();
        account.PasswordCredential = new PlatformPasswordCredentialFactory(hasher)
            .Create(account, password, now);
        account.LoginIdentifiers.Add(new LoginIdentifier
        {
            Id = Guid.NewGuid(), AccountId = personId,
            Type = LoginIdentifierType.Phone, SchoolId = null,
            NormalizedValue = phone, DisplayValue = phone,
            IsVerified = true, VerifiedAtUtc = now, CreatedAtUtc = now
        });
        identity.Accounts.Add(account);
        await identity.SaveChangesAsync();

        var accountId = IdentityAccountId.From(personId);
        var role = PlatformRole.Create(PlatformRoleId.New(),
            $"move-{personId:N}", "Move test", false,
            [PlatformPermissionCodes.SchoolsRead], accountId, now);
        platform.Roles.Add(role);
        platform.RoleAssignments.Add(PlatformRoleAssignment.Assign(
            PlatformRoleAssignmentId.New(), accountId, role.Id, accountId, now));
        platform.LocalUsers.Add(new PlatformLocalUser
        {
            Id = Guid.NewGuid(), PersonId = personId,
            UserName = $"platform-{personId:N}",
            NormalizedUserName = $"PLATFORM-{personId:N}",
            DisplayName = "Operator", Status = "PendingActivation",
            CreatedAtUtc = now, UpdatedAtUtc = now
        });
        await platform.SaveChangesAsync();

        var mover = new PlatformCredentialMover(identity, platform);
        Assert.False((await mover.RunAsync(personId, execute: false)).Executed);
        Assert.Null((await platform.LocalUsers.Include(x => x.Credential)
            .SingleAsync(x => x.PersonId == personId)).Credential);
        Assert.True((await mover.RunAsync(personId, execute: true)).Executed);
        Assert.Null(await identity.PasswordCredentials
            .SingleOrDefaultAsync(x => x.AccountId == personId));
        var local = await platform.LocalUsers.Include(x => x.Credential)
            .SingleAsync(x => x.PersonId == personId);
        Assert.Equal("Active", local.Status);
        Assert.Equal(PasswordVerificationResult.Success,
            hasher.VerifyHashedPassword(account, local.Credential!.PasswordHash, password));
        Assert.NotNull(await new PlatformLoginService(identity, platform, hasher)
            .LoginAsync(phone, password));
        Assert.True((await mover.RunAsync(personId, execute: true)).AlreadyMoved);

        await platformTransaction.RollbackAsync();
        await identityTransaction.RollbackAsync();
    }
}
