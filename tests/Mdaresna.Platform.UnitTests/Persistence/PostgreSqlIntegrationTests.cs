using Mdaresna.Platform.Application.Billing.Units;
using Mdaresna.Platform.Application.Registry.Read;
using Mdaresna.Platform.Domain.Billing.Units;
using Mdaresna.Platform.Domain.Registry;
using Mdaresna.Platform.Infrastructure.Persistence;
using Mdaresna.Platform.Infrastructure.Persistence.Platform.Billing;
using Mdaresna.Platform.Infrastructure.Persistence.Platform.Registry;
using Mdaresna.Platform.Infrastructure.IdentityAuth;
using Mdaresna.Platform.Infrastructure.Persistence.Identity.Entities;
using Mdaresna.Tenancy.Abstractions.Identifiers;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Mdaresna.Platform.UnitTests.Persistence;

public sealed class PostgreSqlIntegrationTests
{
    [Fact]
    public async Task Account_language_is_independent_for_each_application()
    {
        var connection = Environment.GetEnvironmentVariable("MDARESNA_POSTGRES_TEST_IDENTITY");
        if (string.IsNullOrWhiteSpace(connection)) return;
        var target = new NpgsqlConnectionStringBuilder(connection);
        Assert.Equal("localhost", target.Host, ignoreCase: true);
        Assert.StartsWith("mdaresna_pg_verify_", target.Database, StringComparison.OrdinalIgnoreCase);

        var options = new DbContextOptionsBuilder<PostgreSqlIdentityDbContext>()
            .UseNpgsql(connection).Options;
        await using var db = new PostgreSqlIdentityDbContext(options);
        await using var transaction = await db.Database.BeginTransactionAsync();
        var accountId = Guid.NewGuid();
        var now = DateTimeOffset.UtcNow;
        db.Accounts.Add(new Account
        {
            Id = accountId, Status = AccountStatus.Active,
            CreatedAtUtc = now, UpdatedAtUtc = now
        });
        await db.SaveChangesAsync();

        var languages = new AccountAppLanguageService(db);
        Assert.Null(await languages.GetStoredAsync(accountId, AccountAppLanguageService.PlatformApp));
        await languages.SetAsync(accountId, AccountAppLanguageService.PlatformApp, "ar");
        await languages.SetAsync(accountId, AccountAppLanguageService.SchoolsApp, "en");
        Assert.Equal("ar", await languages.GetStoredAsync(accountId, AccountAppLanguageService.PlatformApp));
        Assert.Equal("en", await languages.GetStoredAsync(accountId, AccountAppLanguageService.SchoolsApp));
        Assert.Null(await languages.GetStoredAsync(accountId, AccountAppLanguageService.FamilyApp));
        await transaction.RollbackAsync();
    }

    [Fact]
    public async Task Save_and_search_use_postgresql_semantics()
    {
        var connection = Environment.GetEnvironmentVariable("MDARESNA_POSTGRES_TEST_PLATFORM");
        if (string.IsNullOrWhiteSpace(connection)) return;

        // Opt-in test may write only to a dedicated local verification database.
        var target = new NpgsqlConnectionStringBuilder(connection);
        Assert.Equal("localhost", target.Host, ignoreCase: true);
        Assert.StartsWith("mdaresna_pg_verify_", target.Database, StringComparison.OrdinalIgnoreCase);

        var options = new DbContextOptionsBuilder<PostgreSqlPlatformDbContext>()
            .UseNpgsql(connection).Options;
        await using var db = new PostgreSqlPlatformDbContext(options);
        await using var transaction = await db.Database.BeginTransactionAsync();

        var now = DateTimeOffset.UtcNow;
        var tenant = Tenant.Create(TenantId.New(), "MiXeD % Name", null, now);
        var unitType = UnitType.Create(Guid.NewGuid(), "PG-CHECK", "MiXeD_Unit", 1, "EGP", now);
        db.Tenants.Add(tenant);
        db.UnitTypes.Add(unitType);
        await db.SaveChangesAsync();

        Assert.Equal(16, db.Entry(tenant).Property<byte[]>("RowVersion").CurrentValue?.Length);
        Assert.Equal(16, db.Entry(unitType).Property<byte[]>("RowVersion").CurrentValue?.Length);

        var registry = new RegistryReadStore(db);
        var tenantMatch = await registry.ListTenantsAsync(new ListTenantsQuery(Search: "mixed %"));
        var tenantNoMatch = await registry.ListTenantsAsync(new ListTenantsQuery(Search: "mixed _"));
        Assert.Contains(tenantMatch.Items, item => item.Id == tenant.Id);
        Assert.DoesNotContain(tenantNoMatch.Items, item => item.Id == tenant.Id);

        var units = new UnitTypeRepository(db);
        var unitMatch = await units.ListAsync(new UnitTypeListQuery(Search: "mixed_"));
        var unitNoMatch = await units.ListAsync(new UnitTypeListQuery(Search: "mixed%"));
        Assert.Contains(unitMatch.Items, item => item.Id == unitType.Id);
        Assert.DoesNotContain(unitNoMatch.Items, item => item.Id == unitType.Id);

        await transaction.RollbackAsync();
    }
}
