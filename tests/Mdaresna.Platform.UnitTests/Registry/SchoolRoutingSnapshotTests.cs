using Mdaresna.Platform.Api.Controllers.Internal;
using Mdaresna.Platform.Infrastructure.Persistence;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Npgsql;
using Xunit;

namespace Mdaresna.Platform.UnitTests.Registry;

public sealed class SchoolRoutingSnapshotTests
{
    [RoutingPostgresFact]
    public async Task ReadTransactionWorksWithProductionRetryStrategyAndDisposesOnNotFound()
    {
        var database = "mdaresna_routing_qa_" + Guid.NewGuid().ToString("N");
        var connection = new NpgsqlConnectionStringBuilder(Environment.GetEnvironmentVariable("MDARESNA_ROUTING_PG_QA"))
            { Database = database, Pooling = false }.ConnectionString;
        await using var db = new PostgreSqlPlatformDbContext(new DbContextOptionsBuilder<PostgreSqlPlatformDbContext>()
            .UseNpgsql(connection, pg => pg.EnableRetryOnFailure()).Options);
        try
        {
            await db.Database.MigrateAsync();
            var config = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
                { ["InternalServices:ApiKey"] = "fixture-key" }).Build();
            var controller = new SchoolLoginTargetsController(null!, null!, config, db)
                { ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() } };
            controller.Request.Headers["X-Mdaresna-Internal-Key"] = "fixture-key";
            Assert.IsType<NotFoundObjectResult>(await controller.RoutingSnapshot("SCH-FIXTURE", default));
            Assert.Null(db.Database.CurrentTransaction);
            Assert.IsType<NotFoundObjectResult>(await controller.RoutingSnapshot("SCH-FIXTURE", default));
            Assert.Null(db.Database.CurrentTransaction);
            controller.Request.Headers.Remove("X-Mdaresna-Internal-Key");
            Assert.IsType<UnauthorizedResult>(await controller.RoutingSnapshot("SCH-FIXTURE", default));
        }
        finally
        {
            await db.Database.EnsureDeletedAsync();
        }
    }
}

public sealed class RoutingPostgresFactAttribute : FactAttribute
{
    public RoutingPostgresFactAttribute()
    {
        if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("MDARESNA_ROUTING_PG_QA")))
            Skip = "Requires an isolated opt-in PostgreSQL QA endpoint.";
    }
}
