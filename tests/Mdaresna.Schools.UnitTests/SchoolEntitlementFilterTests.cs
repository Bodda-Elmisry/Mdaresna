using System.Security.Claims;
using Mdaresna.Schools.Api.Auth;
using Mdaresna.Schools.Application.Identity;
using Mdaresna.Schools.Domain.School;
using Mdaresna.Schools.Infrastructure.Identity;
using Mdaresna.Schools.Infrastructure.Persistence;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace Mdaresna.Schools.UnitTests;

public sealed class SchoolEntitlementFilterTests
{
    [Theory]
    [InlineData("SchoolStudents", "GET", true)]
    [InlineData("SchoolStudents", "POST", false)]
    [InlineData("SchoolHomework", "PUT", false)]
    [InlineData("SchoolPeopleOperations", "POST", false)]
    [InlineData("SchoolDepartments", "DELETE", false)]
    [InlineData("SchoolFacilities", "PUT", false)]
    public async Task Expired_contract_preserves_reads_and_blocks_academic_mutations(string controller, string method, bool allowed)
    {
        var factory = new Factory();
        await using (var db = factory.New())
        {
            db.Subscriptions.Add(new SchoolSubscriptionState { SystemId = Guid.NewGuid(), SubscriptionId = Guid.NewGuid(), CapacityMetric = "Student",
                IsEnforced = true, CapabilityCodes = "academic.students\nacademic.homework\nacademic.staff", ValidFromUtc = DateTimeOffset.UtcNow.AddYears(-1), ValidUntilUtc = DateTimeOffset.UtcNow.AddSeconds(-1) });
            await db.SaveChangesAsync();
        }
        var http = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity([new Claim(SchoolClaimTypes.SchoolCode, "QA-SCHOOL")], "fixture")) };
        http.Request.Method = method;
        var route = new RouteData(); route.Values["controller"] = controller;
        var action = new ActionContext(http, route, new ActionDescriptor());
        var executing = new ActionExecutingContext(action, [], new Dictionary<string, object?>(), new object());
        var reached = false;
        await new SchoolEntitlementFilter(factory, new ConfigurationBuilder().Build()).OnActionExecutionAsync(executing, () => {
            reached = true; return Task.FromResult(new ActionExecutedContext(action, [], new object()));
        });
        Assert.Equal(allowed, reached);
        if (!allowed) Assert.Equal(403, Assert.IsType<ObjectResult>(executing.Result).StatusCode);
    }
    private sealed class Factory : ISchoolDbContextFactory
    {
        private readonly DbContextOptions<SchoolsDbContext> options = new DbContextOptionsBuilder<SchoolsDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
        public SchoolsDbContext New() => new(options);
        public Task<SchoolLoginTarget?> ResolveAsync(string schoolCode, CancellationToken cancellationToken = default) => Task.FromResult<SchoolLoginTarget?>(null);
        public Task<SchoolsDbContext?> CreateAsync(string schoolCode, CancellationToken cancellationToken = default) => Task.FromResult<SchoolsDbContext?>(New());
    }
}
