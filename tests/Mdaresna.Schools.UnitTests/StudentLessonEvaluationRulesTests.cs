using Mdaresna.Schools.Domain.Students;

namespace Mdaresna.Schools.UnitTests;

public sealed class StudentLessonEvaluationRulesTests
{
    [Theory]
    [InlineData(StudentLessonEvaluationEntryStatus.Rated, 1, 5, true)]
    [InlineData(StudentLessonEvaluationEntryStatus.Rated, 0, 5, false)]
    [InlineData(StudentLessonEvaluationEntryStatus.Rated, 3, null, false)]
    [InlineData(StudentLessonEvaluationEntryStatus.NotRated, null, null, true)]
    [InlineData(StudentLessonEvaluationEntryStatus.Absent, null, null, true)]
    [InlineData(StudentLessonEvaluationEntryStatus.Absent, 1, 1, false)]
    public void Entry_validation_matches_status(StudentLessonEvaluationEntryStatus status, int? focus, int? behavior, bool expected) =>
        Assert.Equal(expected, StudentLessonEvaluationRules.IsValidEntry(status, focus, behavior));
}
