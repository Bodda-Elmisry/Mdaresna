using Mdaresna.Schools.Contracts.Provisioning;
using Mdaresna.Schools.Domain.School;
using Mdaresna.Schools.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Mdaresna.Schools.Infrastructure.Identity;

public static class SchoolSubscriptionSync
{
    public static async Task ApplyAsync(SchoolsDbContext db, SchoolSubscriptionSnapshotChangedV1 message, CancellationToken ct)
    {
        Validate(message);
        var school = await db.SchoolInformation.AsNoTracking().SingleAsync(ct);
        if (school.PlatformSchoolReferenceId != message.SchoolId || school.Code != message.SchoolCode) throw new ArgumentException("Subscription school scope mismatch.");
        await using var tx = db.Database.IsRelational() ? await db.Database.BeginTransactionAsync(ct) : null;
        var exists = await db.Subscriptions.AnyAsync(x => x.SystemId == message.SystemId, ct);
        var state = exists ? await SchoolCapacityService.LockAsync(db, message.SystemId, ct) : new SchoolSubscriptionState { SystemId = message.SystemId };
        if (exists && state.SubscriptionId != message.SubscriptionId) throw new ArgumentException("A subscription identity cannot be replaced by a replay.");
        if (state.Revision >= message.Revision)
        {
            if (state.Revision == message.Revision && await InitializeEmptySchoolAsync(db, state, ct))
            {
                await db.SaveChangesAsync(ct);
                if (tx != null) await tx.CommitAsync(ct);
            }
            return;
        }
        state.SubscriptionId = message.SubscriptionId; state.SystemCode = message.SystemCode; state.CapacityMetric = message.CapacityMetric;
        state.Revision = message.Revision; state.BaseCapacity = message.BaseCapacity; state.AddOnCapacity = message.AddOnCapacity;
        state.IsSuspended = message.IsSuspended; state.ValidFromUtc = message.ValidFromUtc; state.ValidUntilUtc = message.ValidUntilUtc;
        state.PlanId = message.PlanId; state.CapabilityCodes = string.Join('\n', message.Capabilities.Distinct().Order());
        state.SyncedAtUtc = DateTimeOffset.UtcNow; state.LocalVersion++;
        if (!exists) db.Subscriptions.Add(state);
        await InitializeEmptySchoolAsync(db, state, ct);
        db.CapacityMovements.Add(new() { Id = Guid.NewGuid(), SystemId = state.SystemId, Action = "SnapshotApplied", OccurredAtUtc = DateTimeOffset.UtcNow });
        await db.SaveChangesAsync(ct);
        if (tx != null) await tx.CommitAsync(ct);
    }
    private static async Task<bool> InitializeEmptySchoolAsync(SchoolsDbContext db, SchoolSubscriptionState state, CancellationToken ct)
    {
        if (state.IsEnforced || state.UsedCapacity != 0 || state.CapacityMetric != "Student" || state.IsSuspended ||
            state.ValidFromUtc > DateTimeOffset.UtcNow || state.ValidUntilUtc <= DateTimeOffset.UtcNow ||
            !state.CapabilityCodes.Split('\n').Contains("academic.students") ||
            await db.Students.AnyAsync(ct) || await db.CapacityAllocations.AnyAsync(x => x.SystemId == state.SystemId, ct) ||
            await db.Subscriptions.AnyAsync(x => x.SystemId != state.SystemId && x.CapacityMetric == "Student" && x.IsEnforced, ct)) return false;
        state.IsEnforced = true; state.UsedCapacity = 0; state.LocalVersion++;
        db.CapacityMovements.Add(new() { Id = Guid.NewGuid(), SystemId = state.SystemId, Action = "EmptyBaselineInitialized", OccurredAtUtc = DateTimeOffset.UtcNow });
        return true;
    }
    public static void Validate(SchoolSubscriptionSnapshotChangedV1 x)
    {
        if (x.SchoolId == Guid.Empty || x.SubscriptionId == Guid.Empty || x.SystemId == Guid.Empty || x.PlanId == Guid.Empty || x.PlanRevision < 1 || x.Revision < 1 ||
            x.BaseCapacity is < 1 or > 1_000_000 || x.AddOnCapacity is < 0 or > 1_000_000 || x.CapacityMetric is not ("Student" or "Employee" or "Cashbox") ||
            x.ValidFromUtc.Offset != TimeSpan.Zero || x.ValidUntilUtc.Offset != TimeSpan.Zero || x.ValidUntilUtc <= x.ValidFromUtc ||
            x.Capabilities is null || x.Capabilities.Length is < 1 or > 50 || x.Capabilities.Any(c => string.IsNullOrWhiteSpace(c) || c.Length > 100) ||
            !System.Text.RegularExpressions.Regex.IsMatch(x.SchoolCode ?? "", "^[A-Z0-9](?:[A-Z0-9-]{1,30}[A-Z0-9])$") ||
            !System.Text.RegularExpressions.Regex.IsMatch(x.SystemCode ?? "", "^[A-Z0-9](?:[A-Z0-9-]{1,30}[A-Z0-9])$"))
            throw new ArgumentException("Invalid subscription snapshot.");
    }
}
