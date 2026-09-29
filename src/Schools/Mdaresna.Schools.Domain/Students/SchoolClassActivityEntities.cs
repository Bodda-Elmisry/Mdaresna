using Mdaresna.Schools.Domain.Academics;
using Mdaresna.Schools.Domain.Identity;

namespace Mdaresna.Schools.Domain.Students;

public enum ActivityScope { General, Subject }
public enum ClassActivityStatus { Draft, Published, Completed, Cancelled }
public enum ActivityAudienceMode { AllClass, SelectedStudents }
public enum ActivityParticipationStatus { NotRecorded, Participated, DidNotParticipate, Excused }
public enum ActivityCategory { Practical, Competition, Presentation, Project, Artistic, Sports, Social, Other }

public static class ClassActivityRules
{
    public static bool HasValidScoreDefinition(bool isGraded, decimal? totalScore) => isGraded
        ? totalScore.HasValue && totalScore.Value > 0 && totalScore.Value <= 10000
        : !totalScore.HasValue;

    public static ClassActivityAudienceChanges CalculateAudienceChanges(ActivityAudienceMode mode,
        IEnumerable<Guid> currentEnrollmentIds, IEnumerable<Guid> requestedEnrollmentIds)
    {
        var current = currentEnrollmentIds.ToHashSet();
        var requested = mode == ActivityAudienceMode.SelectedStudents
            ? requestedEnrollmentIds.ToHashSet()
            : [];
        return new ClassActivityAudienceChanges(requested.Except(current).ToArray(), current.Except(requested).ToArray());
    }

    public static bool CanAccessExistingActivity(ActivityScope scope, bool isSchoolAdmin,
        Guid? activityGradeSubjectOfferingId, IEnumerable<Guid> currentClassGradeSubjectOfferingIds)
    {
        if (isSchoolAdmin) return true;
        var currentScopes = currentClassGradeSubjectOfferingIds.ToHashSet();
        return scope switch
        {
            ActivityScope.General => currentScopes.Count != 0,
            ActivityScope.Subject => activityGradeSubjectOfferingId.HasValue &&
                                     currentScopes.Contains(activityGradeSubjectOfferingId.Value),
            _ => false
        };
    }
}

public sealed record ClassActivityAudienceChanges(IReadOnlyList<Guid> AddedEnrollmentIds, IReadOnlyList<Guid> RemovedEnrollmentIds);

public sealed class ClassActivity
{
    public Guid Id { get; set; }
    public Guid ClassSectionId { get; set; }
    public Guid? ClassSectionSubjectId { get; set; }
    public ActivityScope Scope { get; set; }
    public ActivityCategory Category { get; set; }
    public ClassActivityStatus Status { get; set; } = ClassActivityStatus.Draft;
    public ActivityAudienceMode AudienceMode { get; set; } = ActivityAudienceMode.AllClass;
    public string Title { get; set; } = string.Empty;
    public string Details { get; set; } = string.Empty;
    public DateOnly ActivityDate { get; set; }
    public TimeOnly? StartsAt { get; set; }
    public TimeOnly? EndsAt { get; set; }
    public string? Location { get; set; }
    public bool IsGraded { get; set; }
    public decimal? TotalScore { get; set; }
    public string TimeZoneIdSnapshot { get; set; } = string.Empty;
    public Guid CreatedByUserId { get; set; }
    public Guid? PublishedByUserId { get; set; }
    public Guid? CompletedByUserId { get; set; }
    public Guid? CancelledByUserId { get; set; }
    public DateTimeOffset? PublishedAtUtc { get; set; }
    public DateTimeOffset? CompletedAtUtc { get; set; }
    public DateTimeOffset? CancelledAtUtc { get; set; }
    public string? CancellationReason { get; set; }
    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset UpdatedAtUtc { get; set; }
    public ClassSection ClassSection { get; set; } = null!;
    public ClassSectionSubject? ClassSectionSubject { get; set; }
    public LocalUserAccount CreatedByUser { get; set; } = null!;
    public LocalUserAccount? PublishedByUser { get; set; }
    public LocalUserAccount? CompletedByUser { get; set; }
    public LocalUserAccount? CancelledByUser { get; set; }
    public ICollection<ClassActivityAudienceStudent> AudienceStudents { get; set; } = [];
    public ICollection<ClassActivityParticipant> Participants { get; set; } = [];
    public ICollection<ClassActivityAudit> AuditTrail { get; set; } = [];

    public bool HasRecordedEveryParticipant() => Participants.Count != 0 &&
        Participants.All(x => x.Status != ActivityParticipationStatus.NotRecorded);

    public bool HasEveryRequiredScore() => !IsGraded || TotalScore.HasValue && Participants
        .Where(x => x.Status == ActivityParticipationStatus.Participated)
        .All(x => x.Score.HasValue && x.Score >= 0 && x.Score <= TotalScore);
}

/// <summary>Stores the selected-student draft audience. Published participant rows are the immutable operational snapshot.</summary>
public sealed class ClassActivityAudienceStudent
{
    public Guid ClassActivityId { get; set; }
    public Guid StudentEnrollmentId { get; set; }
    public ClassActivity ClassActivity { get; set; } = null!;
    public StudentEnrollment StudentEnrollment { get; set; } = null!;
}

public sealed class ClassActivityParticipant
{
    public Guid Id { get; set; }
    public Guid ClassActivityId { get; set; }
    public Guid StudentEnrollmentId { get; set; }
    public ActivityParticipationStatus Status { get; set; } = ActivityParticipationStatus.NotRecorded;
    public decimal? Score { get; set; }
    public string? Note { get; set; }
    public Guid? EvaluatedByUserId { get; set; }
    public DateTimeOffset? EvaluatedAtUtc { get; set; }
    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset UpdatedAtUtc { get; set; }
    public ClassActivity ClassActivity { get; set; } = null!;
    public StudentEnrollment StudentEnrollment { get; set; } = null!;
    public LocalUserAccount? EvaluatedByUser { get; set; }
}

public sealed class ClassActivityAudit
{
    public Guid Id { get; set; }
    public Guid ClassActivityId { get; set; }
    public string Action { get; set; } = string.Empty;
    public Guid ActorUserId { get; set; }
    public string? Reason { get; set; }
    public string? PayloadJson { get; set; }
    public DateTimeOffset CreatedAtUtc { get; set; }
    public ClassActivity ClassActivity { get; set; } = null!;
    public LocalUserAccount ActorUser { get; set; } = null!;
}
