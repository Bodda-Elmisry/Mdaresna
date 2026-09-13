using Mdaresna.Platform.Domain.Billing.Units;
using Mdaresna.Platform.Infrastructure.Persistence.Platform;
using Microsoft.EntityFrameworkCore;

namespace Mdaresna.Platform.UnitTests.Billing;

public sealed class UnitCommercePersistenceTests
{
    [Fact]
    public void UnitTypeCatalogQueryTranslatesForSqlServer()
    {
        using var db = CreateContext();
        var sql = db.UnitTypes.AsNoTracking()
            .Where(x => x.IsActive &&
                (x.Code.Contains("ACTIVATION") || x.DisplayName.Contains("ACTIVATION")))
            .OrderBy(x => x.Code)
            .Skip(20)
            .Take(20)
            .Select(x => new { x.Id, x.Code, x.UnitPrice, x.Currency })
            .ToQueryString();

        Assert.Contains("[billing].[unit_types]", sql);
        Assert.Contains("ORDER BY", sql);
        Assert.Contains("OFFSET", sql);
    }

    [Fact]
    public async Task PurchaseIntentCannotBeUpdatedThroughEf()
    {
        await using var db = CreateContext();
        var schoolId = Guid.NewGuid();
        var timestamp = new DateTimeOffset(2026, 9, 13, 10, 0, 0, TimeSpan.Zero);
        var unitType = UnitType.Create(
            Guid.NewGuid(), "STUDENT-ACTIVATION", "Student activation", 10m,
            "EGP", timestamp);
        var intent = UnitPurchaseIntent.Create(
            Guid.NewGuid(),
            Mdaresna.Tenancy.Abstractions.Identifiers.TenantId.From(schoolId),
            Mdaresna.Tenancy.Abstractions.Identifiers.SchoolId.From(schoolId),
            unitType, 2, "bank-transfer", timestamp, timestamp);

        db.Entry(intent).State = EntityState.Modified;

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => db.SaveChangesAsync());
        Assert.Contains("append-only", exception.Message);
    }

    private static PlatformDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<PlatformDbContext>()
            .UseSqlServer("Server=(localdb)\\MSSQLLocalDB;Database=UnitCommerceModelOnly;Trusted_Connection=True;")
            .Options;
        return new PlatformDbContext(options);
    }
}
