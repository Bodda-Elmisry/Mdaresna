using System.IdentityModel.Tokens.Jwt;
using Mdaresna.Api.Contracts;
using Mdaresna.Schools.Api.Auth;
using Mdaresna.Schools.Domain.School;
using Mdaresna.Schools.Domain.Students;
using Mdaresna.Schools.Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Mdaresna.Schools.Api.Controllers;

[ApiController, Authorize, Route("api/schools/v1/subscriptions")]
public sealed class SchoolSubscriptionsController(ISchoolDbContextFactory factory) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> List(CancellationToken ct)
    {
        if (!User.HasClaim(SchoolClaimTypes.Permission, "school.profile.manage")) return Forbid();
        await using var db = await factory.CreateAsync(User.FindFirst(SchoolClaimTypes.SchoolCode)?.Value ?? "", ct); if (db == null) return Unauthorized();
        var baselineCount = await db.Students.CountAsync(s => s.IsActive && s.Enrollments.Any(e => e.Status == StudentEnrollmentStatus.Active && e.GradeOffering.ProgramAcademicYear.IsActive), ct);
        var rows = await db.Subscriptions.AsNoTracking().OrderBy(x => x.SystemCode).Select(x => new {
            x.SystemId, x.SubscriptionId, x.SystemCode, x.CapacityMetric, x.Revision, x.LocalVersion, x.PlanId, x.BaseCapacity, x.AddOnCapacity,
            TotalCapacity = x.BaseCapacity + x.AddOnCapacity, x.UsedCapacity, AvailableCapacity = x.BaseCapacity + x.AddOnCapacity - x.UsedCapacity,
            x.IsSuspended, x.IsEnforced, x.ValidFromUtc, x.ValidUntilUtc, x.SyncedAtUtc, x.CapabilityCodes, BaselineStudentCount = baselineCount }).ToArrayAsync(ct);
        return Ok(ApiResponse<object>.Success(rows));
    }
    [HttpPost("{systemId:guid}/initialize-students"), Authorize(Policy = SchoolPermissionPolicies.StudentsManage)]
    public async Task<IActionResult> Initialize(Guid systemId, InitializeCapacityRequest input, CancellationToken ct)
    {
        if (!User.HasClaim(SchoolClaimTypes.Permission, "school.profile.manage")) return Forbid();
        await using var db = await factory.CreateAsync(User.FindFirst(SchoolClaimTypes.SchoolCode)?.Value ?? "", ct); if (db == null) return Unauthorized();
        await using var transaction = db.Database.IsRelational() ? await db.Database.BeginTransactionAsync(ct) : null;
        if (!await db.Subscriptions.AnyAsync(x => x.SystemId == systemId, ct)) return NotFound();
        var state = await SchoolCapacityService.LockAsync(db, systemId, ct);
        if (state.Revision != input.ExpectedRevision || state.IsEnforced || state.CapacityMetric != "Student" || state.IsSuspended ||
            DateTimeOffset.UtcNow < state.ValidFromUtc || DateTimeOffset.UtcNow >= state.ValidUntilUtc) return Conflict();
        if (await db.Subscriptions.AnyAsync(x => x.SystemId != systemId && x.CapacityMetric == "Student" && x.IsEnforced, ct)) return Conflict();
        // Explicit reviewed baseline: active students with a current active enrollment, distinct by person.
        var ids = await db.Students.Where(s => s.IsActive && s.Enrollments.Any(e => e.Status == StudentEnrollmentStatus.Active && e.GradeOffering.ProgramAcademicYear.IsActive))
            .Select(s => s.Id).Take(10001).ToArrayAsync(ct);
        if (ids.Length > 10000 || ids.Length != input.ExpectedUsedCapacity || ids.Length > state.BaseCapacity + state.AddOnCapacity) return Conflict();
        if (await db.CapacityAllocations.AnyAsync(x => x.SystemId == systemId, ct)) return Conflict();
        var now = DateTimeOffset.UtcNow;
        foreach (var id in ids) db.CapacityAllocations.Add(new SchoolCapacityAllocation { Id = Guid.NewGuid(), SystemId = systemId, SubjectId = id, IsActive = true, UpdatedAtUtc = now });
        state.UsedCapacity = ids.Length; state.IsEnforced = true; state.LocalVersion++;
        var actor = Guid.TryParse(User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value, out var account) ? account : Guid.Empty;
        db.CapacityMovements.Add(new() { Id = Guid.NewGuid(), SystemId = systemId, ActorId = actor, Action = "BaselineInitialized", OccurredAtUtc = now });
        await db.SaveChangesAsync(ct); if (transaction != null) await transaction.CommitAsync(ct);
        return Ok(ApiResponse<object>.Success(new { state.UsedCapacity, state.IsEnforced }));
    }
}
public sealed record InitializeCapacityRequest(long ExpectedRevision, int ExpectedUsedCapacity);
