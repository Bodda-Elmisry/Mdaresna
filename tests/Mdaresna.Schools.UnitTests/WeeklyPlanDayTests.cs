using Microsoft.EntityFrameworkCore;
using System.Reflection;
using Mdaresna.Schools.Api.Controllers;
using Mdaresna.Schools.Domain.Academics;
using Mdaresna.Schools.Infrastructure.Persistence;

namespace Mdaresna.Schools.UnitTests;

public sealed class WeeklyPlanDayTests
{
    [Theory]
    [InlineData(TeachingPlanType.Monthly,"2026-10-05","2026-11-03",true)]
    [InlineData(TeachingPlanType.Monthly,"2026-10-01","2026-10-31",false)]
    [InlineData(TeachingPlanType.Weekly,"2026-10-05","2026-10-09",true)]
    [InlineData(TeachingPlanType.Weekly,"2026-10-31","2026-11-03",true)]
    [InlineData(TeachingPlanType.Weekly,"2026-10-31","2026-11-06",false)]
    public void Plans_use_configured_period_not_Gregorian_month(TeachingPlanType type,string from,string to,bool valid) =>
        Assert.Equal(valid,TeachingPlanPeriodRules.Matches(type,DateOnly.Parse(from),DateOnly.Parse(to),new(2026,10,5),new(2026,11,3)));
    [Theory]
    [InlineData("2026-10-10", "2026-10-16", true)]
    [InlineData("2026-10-13", "2026-10-19", false)]
    [InlineData("2026-09-29", "2026-10-02", true)]
    [InlineData("2026-09-29", "2026-10-05", false)]
    [InlineData("2026-10-31", "2026-11-02", true)]
    public void Term_weeks_follow_Saturday_and_clip_term_boundaries(string from, string to, bool valid) =>
        Assert.Equal(valid, WeeklyPlanDayRules.IsTermWeek(DateOnly.Parse(from), DateOnly.Parse(to), new(2026, 9, 29), new(2026, 11, 2)));
    private static readonly DateOnly Start = new(2026, 9, 29);
    [Fact] public void Week_crossing_month_accepts_multiple_days() => Assert.True(WeeklyPlanDayRules.IsValid(Start, Start.AddDays(6), [(Start, "Tuesday"), (Start.AddDays(3), "Friday")]));
    [Fact] public void Partial_last_week_accepts_one_day() => Assert.True(WeeklyPlanDayRules.IsValid(Start, Start.AddDays(2), [(Start.AddDays(2), "Details")]));
    [Fact] public void Empty_week_is_rejected() => Assert.False(WeeklyPlanDayRules.IsValid(Start, Start.AddDays(6), []));
    [Fact] public void Duplicate_dates_are_rejected() => Assert.False(WeeklyPlanDayRules.IsValid(Start, Start.AddDays(6), [(Start, "A"), (Start, "B")]));
    [Fact] public void Day_outside_week_is_rejected() => Assert.False(WeeklyPlanDayRules.IsValid(Start, Start.AddDays(6), [(Start.AddDays(7), "A")]));
    [Fact] public void Blank_day_is_rejected() => Assert.False(WeeklyPlanDayRules.IsValid(Start, Start.AddDays(6), [(Start, "  ")]));
    [Fact] public void Oversized_day_is_rejected() => Assert.False(WeeklyPlanDayRules.IsValid(Start, Start.AddDays(6), [(Start, new string('a', 4001))]));
    [Fact] public void Eight_day_period_is_rejected() => Assert.False(WeeklyPlanDayRules.IsValid(Start, Start.AddDays(7), [(Start, "A")]));
    [Fact] public void Reversed_period_is_rejected() => Assert.False(WeeklyPlanDayRules.IsValid(Start, Start.AddDays(-1), [(Start, "A")]));

    [Fact] public async Task Days_round_trip_without_rewriting_legacy_text()
    {
        await using var db = new SchoolsDbContext(new DbContextOptionsBuilder<SchoolsDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
        var plan = new TeachingPlan { Id = Guid.NewGuid(), Details = "Old weekly paragraph", TitleAr = "Plan", TitleEn = "Plan" };
        db.TeachingPlans.Add(plan);
        var apply = typeof(SchoolTeachingPlansController).GetMethod("ApplyDays", BindingFlags.Static | BindingFlags.NonPublic)!;
        apply.Invoke(null, [db, plan, new SaveTeachingPlanDayRequest[] { new(Start, "A"), new(Start.AddDays(1), "B") }]);
        await db.SaveChangesAsync();
        Assert.Equal(2, await db.Set<TeachingPlanDayDetail>().CountAsync());
        var originalId = plan.DayDetails.Single(x => x.Date == Start).Id;
        Assert.All(plan.DayDetails, x => Assert.Equal(x.Date.DayOfWeek, x.DayOfWeek));
        apply.Invoke(null, [db, plan, new SaveTeachingPlanDayRequest[] { new(Start, "Updated") }]);
        await db.SaveChangesAsync();
        Assert.Equal(originalId, (await db.Set<TeachingPlanDayDetail>().SingleAsync()).Id);
        Assert.Equal("Updated", plan.DayDetails.Single().Details);
        apply.Invoke(null, [db, plan, null]);
        await db.SaveChangesAsync();
        Assert.Single(plan.DayDetails); Assert.Equal("Old weekly paragraph", plan.Details);
    }

    [Fact] public async Task Revision_prevents_concurrent_day_edits()
    {
        var options = new DbContextOptionsBuilder<SchoolsDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
        var id = Guid.NewGuid();
        await using (var seed = new SchoolsDbContext(options))
        {
            seed.TeachingPlans.Add(new TeachingPlan { Id = id, Details = "Legacy", TitleAr = "Plan", TitleEn = "Plan" });
            await seed.SaveChangesAsync();
            var metadata = seed.Model.FindEntityType(typeof(TeachingPlanDayDetail))!;
            Assert.Contains(metadata.GetIndexes(), x => x.IsUnique && x.Properties.Select(p => p.Name).SequenceEqual(["TeachingPlanId", "Date"]));
        }
        await using var first = new SchoolsDbContext(options);
        await using var second = new SchoolsDbContext(options);
        var a = await first.TeachingPlans.SingleAsync(x => x.Id == id);
        var b = await second.TeachingPlans.SingleAsync(x => x.Id == id);
        a.Revision++; a.Details = "First"; await first.SaveChangesAsync();
        b.Revision++; b.Details = "Second";
        await Assert.ThrowsAsync<DbUpdateConcurrencyException>(() => second.SaveChangesAsync());
    }
}
