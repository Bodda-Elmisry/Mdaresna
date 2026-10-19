using Mdaresna.Schools.Domain.School;
using Mdaresna.Schools.Domain.Academics;
using Mdaresna.Schools.Domain.Students;
using Mdaresna.Schools.Infrastructure.Identity;
using Mdaresna.Schools.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Mdaresna.Schools.UnitTests;

public sealed class SchoolCapacityTests
{
    [Fact]
    public async Task Academic_year_reconciliation_counts_each_person_once_and_releases_when_last_active_year_stops()
    {
        await using var db = new SchoolsDbContext(new DbContextOptionsBuilder<SchoolsDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
        var state = State(); db.Subscriptions.Add(state);
        var firstYear = new ProgramAcademicYear { Id = Guid.NewGuid() };
        var secondYear = new ProgramAcademicYear { Id = Guid.NewGuid() };
        var firstGrade = new GradeOffering { Id = Guid.NewGuid(), ProgramAcademicYear = firstYear, IsActive = true };
        var secondGrade = new GradeOffering { Id = Guid.NewGuid(), ProgramAcademicYear = secondYear, IsActive = true };
        var student = new Student { Id = Guid.NewGuid(), StudentCode = "QA-001", Enrollments = [
            new StudentEnrollment { Id = Guid.NewGuid(), GradeOffering = firstGrade },
            new StudentEnrollment { Id = Guid.NewGuid(), GradeOffering = secondGrade }] };
        db.Students.Add(student); await db.SaveChangesAsync();
        await SchoolCapacityService.ReconcileStudentYearsAsync(db, state, Guid.NewGuid(), default); await db.SaveChangesAsync();
        Assert.Equal(1, state.UsedCapacity); Assert.Single(db.CapacityAllocations);
        firstYear.IsActive = false; await db.SaveChangesAsync();
        await SchoolCapacityService.ReconcileStudentYearsAsync(db, state, Guid.NewGuid(), default); await db.SaveChangesAsync();
        Assert.Equal(1, state.UsedCapacity);
        secondYear.IsActive = false; await db.SaveChangesAsync();
        await SchoolCapacityService.ReconcileStudentYearsAsync(db, state, Guid.NewGuid(), default); await db.SaveChangesAsync();
        Assert.Equal(0, state.UsedCapacity); Assert.False((await db.CapacityAllocations.SingleAsync()).IsActive);
        firstYear.IsActive = true; await db.SaveChangesAsync();
        await SchoolCapacityService.ReconcileStudentYearsAsync(db, state, Guid.NewGuid(), default); await db.SaveChangesAsync();
        Assert.Equal(1, state.UsedCapacity); Assert.True((await db.CapacityAllocations.SingleAsync()).IsActive);
    }
    [Fact]
    public async Task Allocation_is_idempotent_deactivation_releases_and_reactivation_rechecks_limit()
    {
        await using var db = new SchoolsDbContext(new DbContextOptionsBuilder<SchoolsDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
        var state = State(); db.Subscriptions.Add(state); await db.SaveChangesAsync();
        var first = Guid.NewGuid(); var second = Guid.NewGuid();
        await SchoolCapacityService.ChangeAsync(db, "Student", first, true, Guid.NewGuid(), true, default); await db.SaveChangesAsync();
        await SchoolCapacityService.ChangeAsync(db, "Student", first, true, Guid.NewGuid(), true, default); await db.SaveChangesAsync();
        Assert.Equal(1, state.UsedCapacity); Assert.Single(db.CapacityMovements);
        var error = await Assert.ThrowsAsync<SchoolCapacityException>(() => SchoolCapacityService.ChangeAsync(db, "Student", second, true, Guid.NewGuid(), true, default));
        Assert.Equal("subscription.capacity_exhausted", error.Code);
        await SchoolCapacityService.ChangeAsync(db, "Student", first, false, Guid.NewGuid(), true, default); await db.SaveChangesAsync(); Assert.Equal(0, state.UsedCapacity);
        await SchoolCapacityService.ChangeAsync(db, "Student", second, true, Guid.NewGuid(), true, default); await db.SaveChangesAsync();
        await Assert.ThrowsAsync<SchoolCapacityException>(() => SchoolCapacityService.ChangeAsync(db, "Student", first, true, Guid.NewGuid(), true, default));
        Assert.Equal(1, state.UsedCapacity);
    }
    [Fact]
    public async Task Missing_entitlement_is_rejected_in_strict_mode_and_expired_allocation_cannot_reactivate()
    {
        await using var db = new SchoolsDbContext(new DbContextOptionsBuilder<SchoolsDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
        await Assert.ThrowsAsync<SchoolCapacityException>(() => SchoolCapacityService.ChangeAsync(db, "Student", Guid.NewGuid(), true, Guid.Empty, true, default));
        var state = State(); state.ValidUntilUtc = DateTimeOffset.UtcNow.AddSeconds(-1); db.Subscriptions.Add(state); await db.SaveChangesAsync();
        var error = await Assert.ThrowsAsync<SchoolCapacityException>(() => SchoolCapacityService.ChangeAsync(db, "Student", Guid.NewGuid(), true, Guid.Empty, true, default));
        Assert.Equal("subscription.expired", error.Code);
    }
    private static SchoolSubscriptionState State() => new() { SystemId = Guid.NewGuid(), SubscriptionId = Guid.NewGuid(), SystemCode = "ACADEMIC", CapacityMetric = "Student",
        Revision = 1, LocalVersion = 1, BaseCapacity = 1, IsEnforced = true, CapabilityCodes = "academic.students",
        ValidFromUtc = DateTimeOffset.UtcNow.AddDays(-1), ValidUntilUtc = DateTimeOffset.UtcNow.AddYears(1) };
}
