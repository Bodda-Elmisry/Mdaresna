using Mdaresna.Platform.Infrastructure.Persistence.Platform;
using Mdaresna.Platform.Infrastructure.Persistence.Platform.Billing;
using Microsoft.EntityFrameworkCore;

namespace Mdaresna.Platform.UnitTests.Billing;

public sealed class PlatformPaymentSqlTranslationTests
{
    [Fact]
    public void Billing_detail_query_uses_one_sql_left_join()
    {
        var options = new DbContextOptionsBuilder<PlatformDbContext>()
            .UseSqlServer("Server=localhost;Database=__translation_only;Trusted_Connection=True;TrustServerCertificate=True")
            .Options;
        using var dbContext = new PlatformDbContext(options);

        var query = PlatformPaymentReadStore.WithLedger(
            dbContext.PlatformPaymentRequests.AsNoTracking()
                .Where(request => request.Id == Guid.NewGuid()),
            dbContext.PlatformPaymentLedgerEntries.AsNoTracking());
        var sql = query.ToQueryString();

        Assert.Contains("LEFT JOIN", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("[billing].[school_platform_payment_ledger]", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("[billing].[school_platform_payment_requests]", sql, StringComparison.OrdinalIgnoreCase);
    }
}
