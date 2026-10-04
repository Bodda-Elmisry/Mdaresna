using Mdaresna.Schools.Api.Auth;
using Mdaresna.Schools.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Mdaresna.Schools.UnitTests;

public sealed class TeacherAssignmentNotificationTests
{
    [Fact]
    public void Repeated_unchanged_lesson_save_has_no_recipients()
    {
        var primary = Guid.NewGuid(); var substitute = Guid.NewGuid();
        Assert.Empty(TeacherAssignmentNotifications.ChangedLessonScopes(primary, new HashSet<Guid> { substitute }, primary, [substitute], false));
    }

    [Fact]
    public void New_lesson_notifies_primary_and_nominated_substitutes_once()
    {
        var primary = Guid.NewGuid(); var substitute = Guid.NewGuid();
        var recipients = TeacherAssignmentNotifications.ChangedLessonScopes(null, new HashSet<Guid>(), primary, [substitute, substitute], false);
        Assert.Equal(2, recipients.Count); Assert.Contains(primary, recipients); Assert.Contains(substitute, recipients);
    }

    [Fact]
    public void Adding_substitute_does_not_notify_unchanged_primary()
    {
        var primary = Guid.NewGuid(); var substitute = Guid.NewGuid();
        Assert.Equal(substitute, Assert.Single(TeacherAssignmentNotifications.ChangedLessonScopes(primary, new HashSet<Guid>(), primary, [substitute], false)));
    }

    [Fact]
    public void Changed_time_notifies_all_current_teachers()
    {
        var primary = Guid.NewGuid(); var substitute = Guid.NewGuid();
        Assert.Equal(2, TeacherAssignmentNotifications.ChangedLessonScopes(primary, new HashSet<Guid> { substitute }, primary, [substitute], true).Count);
    }

    [Fact]
    public void Changed_primary_notifies_new_primary()
    {
        var primary = Guid.NewGuid(); var replacement = Guid.NewGuid();
        Assert.Equal(replacement, Assert.Single(TeacherAssignmentNotifications.ChangedLessonScopes(primary, new HashSet<Guid>(), replacement, [], false)));
    }

    [Fact]
    public void Department_notification_has_dates_and_primary_membership()
    {
        var detail = TeacherAssignmentNotifications.Department(Guid.NewGuid(), "اللغة العربية", "Arabic", true, new(2026, 9, 1), new(2027, 6, 30));
        Assert.Contains("كقسمك الأساسي", detail.BodyAr); Assert.Contains("2026-09-01", detail.BodyAr);
        Assert.Contains("2027-06-30", detail.BodyEn); Assert.Equal("department", detail.Kind);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Lesson_message_distinguishes_assignment_from_standby(bool substitute)
    {
        var detail = TeacherAssignmentNotifications.Lesson(Guid.NewGuid(), "3أ", "3A", "عربي", "Arabic", "الثالث", "Third", "2026/2027", "2026/2027", DayOfWeek.Saturday, 2, new(8, 0), new(8, 45), substitute);
        Assert.Contains("السبت", detail.BodyAr); Assert.Contains("08:00", detail.BodyAr); Assert.Contains("08:45", detail.BodyEn);
        Assert.Contains(substitute ? "ليس تكليف تغطية فعليًا" : "كمدرس أساسي", detail.BodyAr);
        Assert.Equal("lesson", detail.Kind);
    }

    [Theory]
    [InlineData("department")]
    [InlineData("grade")]
    [InlineData("class")]
    [InlineData("lesson")]
    public void Notification_is_tracked_in_assignment_context_and_targets_exact_entity(string kind)
    {
        using var db = new PostgreSqlSchoolsDbContext(new DbContextOptionsBuilder<PostgreSqlSchoolsDbContext>().UseNpgsql("Host=localhost;Database=design_only;Username=design_only").Options);
        var user = Guid.NewGuid(); var id = Guid.NewGuid(); var now = DateTimeOffset.UtcNow;
        TeacherAssignmentNotifications.Add(db, user, new(kind, id, "تكليف", "Assignment", "تفاصيل التكليف", "Assignment details"), now);
        var notice = Assert.Single(db.SchoolUserNotifications.Local);
        Assert.Equal(user, notice.RecipientUserId); Assert.Equal($"{kind}/{id:D}", notice.RelatedEntityId);
        Assert.Equal("TeacherAssignment", notice.RelatedEntityType); Assert.False(notice.IsRead); Assert.Equal(now, notice.CreatedAtUtc);
        Assert.Equal(EntityState.Added, db.Entry(notice).State);
    }

    [Fact]
    public void Long_context_does_not_break_notification_storage()
    {
        using var db = new PostgreSqlSchoolsDbContext(new DbContextOptionsBuilder<PostgreSqlSchoolsDbContext>().UseNpgsql("Host=localhost;Database=design_only;Username=design_only").Options);
        TeacherAssignmentNotifications.Add(db, Guid.NewGuid(), new("grade", Guid.NewGuid(), "تكليف", "Assignment", new string('ع', 1400), new string('A', 1400)), DateTimeOffset.UtcNow);
        var notification = Assert.Single(db.SchoolUserNotifications.Local);
        Assert.True(notification.BodyAr.Length <= 1000); Assert.True(notification.BodyEn.Length <= 1000);
        Assert.EndsWith("…", notification.BodyAr);
    }
}
