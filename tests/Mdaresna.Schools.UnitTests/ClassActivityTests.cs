using Mdaresna.Schools.Domain.Students;

namespace Mdaresna.Schools.UnitTests;

public sealed class ClassActivityTests
{
    [Fact]
    public void Score_definition_requires_a_positive_total_only_for_graded_activities()
    {
        Assert.False(ClassActivityRules.HasValidScoreDefinition(true, null));
        Assert.False(ClassActivityRules.HasValidScoreDefinition(true, 0));
        Assert.False(ClassActivityRules.HasValidScoreDefinition(true, -1));
        Assert.True(ClassActivityRules.HasValidScoreDefinition(true, 20));
        Assert.False(ClassActivityRules.HasValidScoreDefinition(true, 10001));
        Assert.True(ClassActivityRules.HasValidScoreDefinition(false, null));
        Assert.False(ClassActivityRules.HasValidScoreDefinition(false, 20));
    }

    [Fact]
    public void Selected_student_audience_reconciliation_keeps_common_rows_and_only_changes_the_difference()
    {
        var kept = Guid.NewGuid();
        var removed = Guid.NewGuid();
        var added = Guid.NewGuid();

        var changes = ClassActivityRules.CalculateAudienceChanges(ActivityAudienceMode.SelectedStudents,
            [kept, removed], [kept, added]);

        Assert.Equal([added], changes.AddedEnrollmentIds);
        Assert.Equal([removed], changes.RemovedEnrollmentIds);
    }

    [Fact]
    public void Switching_to_all_class_removes_draft_selections_without_adding_replacements()
    {
        var changes = ClassActivityRules.CalculateAudienceChanges(ActivityAudienceMode.AllClass,
            [Guid.NewGuid(), Guid.NewGuid()], [Guid.NewGuid()]);

        Assert.Empty(changes.AddedEnrollmentIds);
        Assert.Equal(2, changes.RemovedEnrollmentIds.Count);
    }

    [Fact]
    public void Existing_activity_access_bypasses_historical_subject_state_for_school_admin()
    {
        var allowed = ClassActivityRules.CanAccessExistingActivity(ActivityScope.Subject, true, null, []);

        Assert.True(allowed);
    }

    [Fact]
    public void Existing_subject_activity_access_uses_the_current_matching_grade_subject_scope()
    {
        var activityOffering = Guid.NewGuid();

        Assert.True(ClassActivityRules.CanAccessExistingActivity(ActivityScope.Subject, false,
            activityOffering, [activityOffering]));
        Assert.False(ClassActivityRules.CanAccessExistingActivity(ActivityScope.Subject, false,
            activityOffering, [Guid.NewGuid()]));
        Assert.False(ClassActivityRules.CanAccessExistingActivity(ActivityScope.Subject, false,
            null, [activityOffering]));
    }

    [Fact]
    public void Existing_general_activity_access_requires_a_current_scope_in_the_class()
    {
        Assert.True(ClassActivityRules.CanAccessExistingActivity(ActivityScope.General, false,
            null, [Guid.NewGuid()]));
        Assert.False(ClassActivityRules.CanAccessExistingActivity(ActivityScope.General, false,
            null, []));
    }

    [Fact]
    public void Completion_requires_a_participation_state_for_every_snapshot_student()
    {
        var activity = new ClassActivity
        {
            Participants =
            [
                new ClassActivityParticipant { Status = ActivityParticipationStatus.Participated },
                new ClassActivityParticipant { Status = ActivityParticipationStatus.NotRecorded }
            ]
        };

        Assert.False(activity.HasRecordedEveryParticipant());
        activity.Participants.Last().Status = ActivityParticipationStatus.Excused;
        Assert.True(activity.HasRecordedEveryParticipant());
    }

    [Fact]
    public void Graded_completion_requires_scores_only_for_participating_students()
    {
        var activity = new ClassActivity
        {
            IsGraded = true,
            TotalScore = 20,
            Participants =
            [
                new ClassActivityParticipant { Status = ActivityParticipationStatus.Participated },
                new ClassActivityParticipant { Status = ActivityParticipationStatus.DidNotParticipate },
                new ClassActivityParticipant { Status = ActivityParticipationStatus.Excused }
            ]
        };

        Assert.False(activity.HasEveryRequiredScore());
        activity.Participants.First().Score = 18;
        Assert.True(activity.HasEveryRequiredScore());
        activity.Participants.First().Score = 21;
        Assert.False(activity.HasEveryRequiredScore());
    }

    [Fact]
    public void Ungraded_activity_never_requires_student_scores()
    {
        var activity = new ClassActivity
        {
            IsGraded = false,
            Participants = [new ClassActivityParticipant { Status = ActivityParticipationStatus.Participated }]
        };

        Assert.True(activity.HasEveryRequiredScore());
    }
}
