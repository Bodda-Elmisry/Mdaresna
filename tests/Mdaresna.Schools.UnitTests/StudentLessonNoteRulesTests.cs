using Mdaresna.Schools.Domain.Students;

namespace Mdaresna.Schools.UnitTests;

public sealed class StudentLessonNoteRulesTests
{
    [Theory]
    [InlineData(null, true)]
    [InlineData(1, true)]
    [InlineData(5, true)]
    [InlineData(0, false)]
    [InlineData(6, false)]
    public void Rating_must_be_null_or_one_to_five(int? value, bool expected) =>
        Assert.Equal(expected, StudentLessonNoteRules.IsValidRating(value));
}
