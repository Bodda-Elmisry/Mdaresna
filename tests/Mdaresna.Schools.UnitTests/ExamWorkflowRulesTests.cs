using Mdaresna.Schools.Domain.Exams;
using Xunit;
namespace Mdaresna.Schools.UnitTests;
public sealed class ExamWorkflowRulesTests
{
    [Theory]
    [InlineData(ExamWorkflowStage.Schedule, "school.exams.schedule")]
    [InlineData(ExamWorkflowStage.Committees, "school.exams.committees.manage")]
    [InlineData(ExamWorkflowStage.DepartmentReview, "school.exams.approve")]
    [InlineData(ExamWorkflowStage.SchoolApproval, "school.exams.approve")]
    [InlineData(ExamWorkflowStage.Publish, "school.exams.publish")]
    public void Stage_requires_its_own_permission(ExamWorkflowStage stage, string permission) => Assert.Equal(permission, ExamWorkflowRules.Permission(stage));
    [Fact]
    public void Unknown_stage_is_rejected() => Assert.Throws<ArgumentOutOfRangeException>(() => ExamWorkflowRules.Permission((ExamWorkflowStage)100));
    [Fact]
    public void Parallel_reviews_must_all_complete() {
        var steps = new[] { new ExamWorkflowStep { Order = 0, Status = ExamWorkflowStepStatus.Completed }, new ExamWorkflowStep { Order = 2, Status = ExamWorkflowStepStatus.Completed }, new ExamWorkflowStep { Order = 2, Status = ExamWorkflowStepStatus.Active } };
        Assert.False(ExamWorkflowRules.AllEarlierCompleted(steps, 3)); steps[2].Status = ExamWorkflowStepStatus.Completed;
        Assert.True(ExamWorkflowRules.AllEarlierCompleted(steps, 3));
    }
    [Theory]
    [InlineData(ExamWorkflowStepStatus.Waiting)]
    [InlineData(ExamWorkflowStepStatus.Active)]
    [InlineData(ExamWorkflowStepStatus.Cancelled)]
    public void Incomplete_earlier_step_blocks_next_stage(ExamWorkflowStepStatus status) => Assert.False(ExamWorkflowRules.AllEarlierCompleted([new() { Order = 0, Status = status }], 1));
    [Fact]
    public void Current_and_future_steps_do_not_block_prior_stage() => Assert.True(ExamWorkflowRules.AllEarlierCompleted([new() { Order = 2, Status = ExamWorkflowStepStatus.Active }, new() { Order = 3, Status = ExamWorkflowStepStatus.Waiting }], 2));
    [Fact]
    public void Paper_review_key_is_distinct_from_series_review() => Assert.NotEqual(ExamWorkflowRules.Key(ExamWorkflowStage.DepartmentReview, Guid.NewGuid()), ExamWorkflowRules.Key(ExamWorkflowStage.DepartmentReview, null));
}
