using Mdaresna.Schools.Domain.School;
using Mdaresna.Schools.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Mdaresna.Schools.Infrastructure.Identity;

public sealed class SchoolCapacityException(string code) : Exception(code)
{
    public string Code { get; } = code;
}

public static class SchoolCapacityService
{
    public static async Task<SchoolSubscriptionState?> LockStudentStateAsync(SchoolsDbContext db, CancellationToken ct)
    {
        var ids = await db.Subscriptions.Where(x => x.IsEnforced && x.CapacityMetric == "Student").Select(x => x.SystemId).ToArrayAsync(ct);
        if (ids.Length > 1) throw new SchoolCapacityException("subscription.capacity_conflict");
        return ids.Length == 0 ? null : await LockAsync(db, ids[0], ct);
    }

    public static async Task ReconcileStudentYearsAsync(SchoolsDbContext db, SchoolSubscriptionState state, Guid actor, CancellationToken ct)
    {
        // The caller holds the capacity row lock while saving the academic-year change.
        // Count people across all branches and active years, not enrollment rows.
        var desired = (await db.Students.Where(s => s.IsActive && s.Enrollments.Any(e =>
            e.Status == Mdaresna.Schools.Domain.Students.StudentEnrollmentStatus.Active && e.GradeOffering.ProgramAcademicYear.IsActive))
            .Select(s => s.Id).ToArrayAsync(ct)).ToHashSet();
        if (desired.Count > state.BaseCapacity + state.AddOnCapacity) throw new SchoolCapacityException("subscription.capacity_exhausted");
        var allocations = await db.CapacityAllocations.Where(x => x.SystemId == state.SystemId).ToDictionaryAsync(x => x.SubjectId, ct);
        var now = DateTimeOffset.UtcNow;
        foreach (var subject in allocations.Keys.Union(desired).ToArray())
        {
            var active = desired.Contains(subject);
            allocations.TryGetValue(subject, out var allocation);
            if ((allocation?.IsActive ?? false) == active) continue;
            if (allocation == null) { allocation = new() { Id = Guid.NewGuid(), SystemId = state.SystemId, SubjectId = subject }; db.CapacityAllocations.Add(allocation); }
            allocation.IsActive = active; allocation.UpdatedAtUtc = now;
            db.CapacityMovements.Add(new() { Id = Guid.NewGuid(), SystemId = state.SystemId, SubjectId = subject, ActorId = actor,
                Action = active ? "AcademicYearAllocate" : "AcademicYearRelease", OccurredAtUtc = now });
        }
        state.UsedCapacity = desired.Count; state.LocalVersion++;
    }
    public static async Task<SchoolSubscriptionState> LockAsync(SchoolsDbContext db, Guid systemId, CancellationToken ct)
    {
        if (!db.Database.IsRelational()) return await db.Subscriptions.SingleAsync(x => x.SystemId == systemId, ct);
        if (db.Database.CurrentTransaction == null) throw new InvalidOperationException("A school transaction is required for capacity changes.");
        var query = db.Database.IsNpgsql()
            ? db.Subscriptions.FromSqlInterpolated($"SELECT * FROM school.subscription_states WHERE \"SystemId\" = {systemId} FOR UPDATE")
            : db.Subscriptions.FromSqlInterpolated($"SELECT * FROM [school].[subscription_states] WITH (UPDLOCK, HOLDLOCK) WHERE [SystemId] = {systemId}");
        var item = (await query.ToListAsync(ct)).Single();
        if (db.Entry(item).State == EntityState.Unchanged) await db.Entry(item).ReloadAsync(ct);
        return item;
    }
    public static async Task ChangeAsync(SchoolsDbContext db, string metric, Guid subjectId, bool active, Guid actor, bool requireEntitlement, CancellationToken ct)
    {
        var candidates = await db.Subscriptions.AsNoTracking().Where(x => x.CapacityMetric == metric).Select(x => new { x.SystemId, x.IsEnforced }).ToArrayAsync(ct);
        var ids = candidates.Where(x => x.IsEnforced).Select(x => x.SystemId).ToArray();
        if (ids.Length == 0)
        {
            if (candidates.Length == 1)
            {
                var pending = await LockAsync(db, candidates[0].SystemId, ct);
                if (pending.IsEnforced) ids = [pending.SystemId];
            }
        }
        if (ids.Length == 0)
        {
            if (requireEntitlement) throw new SchoolCapacityException("subscription.not_synced");
            return; // Explicit transitional mode until this school's reviewed baseline is enabled.
        }
        if (ids.Length != 1) throw new SchoolCapacityException("subscription.capacity_conflict");
        var state = await LockAsync(db, ids[0], ct);
        var allocation = await db.CapacityAllocations.SingleOrDefaultAsync(x => x.SystemId == state.SystemId && x.SubjectId == subjectId, ct);
        if (active)
        {
            var now = DateTimeOffset.UtcNow;
            if (state.IsSuspended) throw new SchoolCapacityException("subscription.suspended");
            if (now < state.ValidFromUtc || now >= state.ValidUntilUtc) throw new SchoolCapacityException("subscription.expired");
            if (metric == "Student" && !state.CapabilityCodes.Split('\n').Contains("academic.students")) throw new SchoolCapacityException("subscription.not_entitled");
        }
        if ((allocation?.IsActive ?? false) == active) return;
        if (active)
        {
            if (state.UsedCapacity >= checked(state.BaseCapacity + state.AddOnCapacity)) throw new SchoolCapacityException("subscription.capacity_exhausted");
            if (allocation == null) { allocation = new() { Id = Guid.NewGuid(), SystemId = state.SystemId, SubjectId = subjectId }; db.CapacityAllocations.Add(allocation); }
            state.UsedCapacity = checked(state.UsedCapacity + 1);
        }
        else state.UsedCapacity = checked(state.UsedCapacity - 1);
        if (state.UsedCapacity < 0) throw new SchoolCapacityException("subscription.capacity_conflict");
        allocation!.IsActive = active; allocation.UpdatedAtUtc = DateTimeOffset.UtcNow; state.LocalVersion++;
        db.CapacityMovements.Add(new() { Id = Guid.NewGuid(), SystemId = state.SystemId, SubjectId = subjectId,
            ActorId = actor, Action = active ? "Allocate" : "Release", OccurredAtUtc = DateTimeOffset.UtcNow });
    }
}
