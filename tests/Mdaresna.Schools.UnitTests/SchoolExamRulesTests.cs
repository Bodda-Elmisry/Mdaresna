using Mdaresna.Schools.Domain.Exams;

namespace Mdaresna.Schools.UnitTests;

public sealed class SchoolExamRulesTests
{
    [Theory]
    [InlineData(10, 5, true)]
    [InlineData(10, null, true)]
    [InlineData(0, null, false)]
    [InlineData(10, 11, false)]
    [InlineData(10, -1, false)]
    public void Score_definition_must_be_positive_and_pass_score_inside_total(
        int totalScore, int? passScore, bool expected)
    {
        Assert.Equal(expected, SchoolExamRules.HasValidScoreDefinition(totalScore, passScore));
    }

    [Fact]
    public void Adjacent_exam_windows_do_not_overlap()
    {
        var day = new DateTimeOffset(2026, 10, 1, 8, 0, 0, TimeSpan.Zero);

        Assert.False(SchoolExamRules.Overlaps(
            day,
            day.AddHours(1),
            day.AddHours(1),
            day.AddHours(2)));
    }

    [Fact]
    public void Intersecting_exam_windows_overlap()
    {
        var day = new DateTimeOffset(2026, 10, 1, 8, 0, 0, TimeSpan.Zero);

        Assert.True(SchoolExamRules.Overlaps(
            day,
            day.AddHours(2),
            day.AddHours(1),
            day.AddHours(3)));
    }

    [Theory]
    [InlineData(ExamResultDisposition.Scored, 7, 10, true)]
    [InlineData(ExamResultDisposition.Scored, 11, 10, false)]
    [InlineData(ExamResultDisposition.Scored, null, 10, false)]
    [InlineData(ExamResultDisposition.AbsentUnexcused, null, 10, true)]
    [InlineData(ExamResultDisposition.AbsentUnexcused, 0, 10, false)]
    [InlineData(ExamResultDisposition.AbsentExcused, null, 10, true)]
    public void Result_validation_depends_on_disposition(
        ExamResultDisposition disposition, int? score, int totalScore, bool expected)
    {
        Assert.Equal(expected, SchoolExamRules.HasValidResult(disposition, score, totalScore));
    }

    [Theory]
    [InlineData(ExamSeriesStatus.Draft, true)]
    [InlineData(ExamSeriesStatus.Approved, true)]
    [InlineData(ExamSeriesStatus.Published, false)]
    [InlineData(ExamSeriesStatus.Cancelled, false)]
    public void Only_draft_or_approved_series_can_be_published(ExamSeriesStatus status, bool expected)
    {
        Assert.Equal(expected, SchoolExamRules.CanPublish(status));
    }

    [Theory]
    [InlineData("2026-09-01", "2026-09-15", "2026-12-31", true)]
    [InlineData("2026-12-01", "2026-09-15", "2026-12-10", true)]
    [InlineData("2027-01-01", "2026-09-15", "2026-12-31", false)]
    [InlineData("2026-09-15", "2026-09-15", "2026-12-31", false)]
    public void Assessment_month_must_be_normalized_and_intersect_the_term(
        string month, string startsOn, string endsOn, bool expected)
    {
        Assert.Equal(expected, SchoolExamRules.MonthIntersectsTerm(
            DateOnly.Parse(month), DateOnly.Parse(startsOn), DateOnly.Parse(endsOn)));
    }

    [Theory]
    [InlineData(1, 3, ExamKind.TermFinal)]
    [InlineData(2, 3, ExamKind.TermFinal)]
    [InlineData(3, 3, ExamKind.YearFinal)]
    public void Final_exam_kind_is_derived_from_the_selected_term(
        int selectedTerm, int lastTerm, ExamKind expected)
    {
        Assert.Equal(expected, SchoolExamRules.ResolveFinalKind(selectedTerm, lastTerm));
    }
}
