using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Mdaresna.Schools.Api.Controllers;
using Mdaresna.Schools.Domain.Academics;
using Mdaresna.Schools.Infrastructure.Persistence;

namespace Mdaresna.Schools.UnitTests;

public sealed class TeachingPlanCalendarTests
{
    [Theory]
    [InlineData(TeachingPlanType.Yearly, "خطة عام 2026/2027")]
    [InlineData(TeachingPlanType.Term, "خطة التيرم الأول لعام 2026/2027")]
    [InlineData(TeachingPlanType.Monthly, "خطة شهر سبتمبر لعام 2026/2027")]
    public void Titles_use_academic_year_without_subject_suffix(TeachingPlanType type, string expected)
    {
        var method = typeof(SchoolTeachingPlansController).GetMethod("Titles", BindingFlags.NonPublic | BindingFlags.Static)!;
        var date = new DateOnly(2026, 9, 1);
        var result = ((string, string))method.Invoke(null, [type, date, date, "العربية", "Arabic", "التيرم الأول", "First term", date, "2026/2027", "2026/2027", null, null])!;
        Assert.Equal(expected, result.Item1);
        Assert.Contains("2026/2027", result.Item2);
        Assert.DoesNotContain("Arabic", result.Item2);
    }

    [Fact]
    public void Monthly_title_uses_configured_period_name_not_date_month()
    {
        var method = typeof(SchoolTeachingPlansController).GetMethod("Titles", BindingFlags.NonPublic | BindingFlags.Static)!;
        var result = ((string,string))method.Invoke(null,[TeachingPlanType.Monthly,new DateOnly(2026,10,5),new DateOnly(2026,11,3),"العربية","Arabic","التيرم الأول","First term",new DateOnly(2026,9,1),"2026/2027","2026/2027","الشهر الدراسي الثاني","Second study month"])!;
        Assert.Equal("خطة شهر الشهر الدراسي الثاني لعام 2026/2027",result.Item1);
        Assert.Contains("Second study month",result.Item2);
    }

    [Theory]
    [InlineData(TeachingPlanType.Yearly)]
    [InlineData(TeachingPlanType.Term)]
    public async Task Calendar_overrides_client_dates_and_term_must_belong_to_year(TeachingPlanType type)
    {
        await using var db = new SchoolsDbContext(new DbContextOptionsBuilder<SchoolsDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
        var year = new ProgramAcademicYear { Id = Guid.NewGuid(), StartDate = new(2026, 9, 1), EndDate = new(2027, 6, 30) };
        var term = new AcademicTerm { Id = Guid.NewGuid(), ProgramAcademicYear = year, StartDate = new(2026, 9, 15), EndDate = new(2027, 1, 20) };
        var subject = new GradeSubjectOffering { Id = Guid.NewGuid(), GradeOffering = new GradeOffering { Id = Guid.NewGuid(), ProgramAcademicYear = year } };
        db.AcademicTerms.Add(term); db.GradeSubjectOfferings.Add(subject); await db.SaveChangesAsync();
        var request = new SaveTeachingPlanRequest(type, subject.Id, term.Id, null, new(2000, 1, 1), new(2000, 1, 2), "Details", TeachingPlanSourceAuthority.School, null, [], []);
        var method = typeof(SchoolTeachingPlansController).GetMethod("ResolveCalendarPeriod", BindingFlags.NonPublic | BindingFlags.Static)!;
        var result = await (Task<SaveTeachingPlanRequest>)method.Invoke(null, [db, request, CancellationToken.None])!;
        Assert.Equal(type == TeachingPlanType.Yearly ? year.StartDate : term.StartDate, result.FromDate);
        Assert.Equal(type == TeachingPlanType.Yearly ? year.EndDate : term.EndDate, result.ToDate);
        if (type == TeachingPlanType.Yearly) Assert.Null(result.AcademicTermId);
        else
        {
            var rejected = await (Task<SaveTeachingPlanRequest>)method.Invoke(null, [db, request with { AcademicTermId = Guid.NewGuid() }, CancellationToken.None])!;
            Assert.True(rejected.FromDate > rejected.ToDate);
        }
    }
}
