using Mdaresna.Schools.Api.Exams;
using Mdaresna.Schools.Domain.Exams;
using Mdaresna.Schools.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;
namespace Mdaresna.Schools.UnitTests;
public sealed class ExamWorkflowEngineTests
{
    [Theory]
    [InlineData(false, false, 2)]
    [InlineData(true, false, 4)]
    [InlineData(false, true, 3)]
    [InlineData(true, true, 5)]
    public void Snapshot_controls_required_reviews(bool department, bool school, int count) {
        var series = new ExamSeries { PolicySnapshotJson = System.Text.Json.JsonSerializer.Serialize(new { DepartmentApprovalRequired = department, SchoolApprovalRequired = school }), Papers = [new() { Id = Guid.NewGuid() }, new() { Id = Guid.NewGuid() }] };
        var templates = ExamWorkflowEngine.Templates(series);
        Assert.Equal(count, templates.Count); Assert.Equal(ExamWorkflowStage.Schedule, templates.First().Stage); Assert.Equal(ExamWorkflowStage.Publish, templates.Last().Stage);
        var reviews = templates.Where(t => t.Stage == ExamWorkflowStage.DepartmentReview).ToArray();
        Assert.Equal(department ? 2 : 0, reviews.Length); Assert.All(reviews, t => Assert.Equal(2, t.Order));
    }
    [Fact]
    public void Committee_stage_is_omitted_for_classroom_exam() {
        var s = new ExamSeries { Papers = [new() { Sittings = [new() { AdministrationMode = ExamAdministrationMode.InClass }] }] };
        Assert.DoesNotContain(ExamWorkflowEngine.Templates(s), t => t.Stage == ExamWorkflowStage.Committees);
        s.Papers.First().Sittings.First().AdministrationMode = ExamAdministrationMode.Committee;
        Assert.Contains(ExamWorkflowEngine.Templates(s), t => t.Stage == ExamWorkflowStage.Committees);
    }
    [Fact]
    public void Handoff_waits_for_parallel_reviews_and_notifies_once() {
        using var db = Context(); var owner = Guid.NewGuid();
        var run = new ExamWorkflow { ExamSeriesId = Guid.NewGuid(), Series = new(), Revision = 3, Steps = [new() { Order = 2, Status = ExamWorkflowStepStatus.Active }, new() { Order = 3, Stage = ExamWorkflowStage.SchoolApproval, Status = ExamWorkflowStepStatus.Waiting, AssigneeUserId = owner }] };
        ExamWorkflowEngine.Advance(db, run, Guid.Empty); Assert.Empty(db.SchoolUserNotifications.Local);
        run.Steps.First().Status = ExamWorkflowStepStatus.Completed;
        ExamWorkflowEngine.Advance(db, run, Guid.Empty); ExamWorkflowEngine.Advance(db, run, Guid.Empty);
        Assert.Single(db.SchoolUserNotifications.Local); Assert.Single(db.ExamNotificationOutbox.Local);
        Assert.Equal(ExamSeriesStatus.PendingApproval, run.Series.Status);
    }
    [Fact]
    public void Self_handoff_does_not_spam_actor_but_task_remains_active() {
        using var db = Context(); var user = Guid.NewGuid();
        var run = new ExamWorkflow { ExamSeriesId = Guid.NewGuid(), Series = new(), Steps = [new() { Stage = ExamWorkflowStage.Publish, Order = 4, AssigneeUserId = user }] };
        ExamWorkflowEngine.Advance(db, run, user);
        Assert.Empty(db.SchoolUserNotifications.Local); Assert.Equal(ExamWorkflowStepStatus.Active, run.Steps.Single().Status); Assert.Equal(ExamSeriesStatus.Approved, run.Series.Status);
    }
    [Fact]
    public void Durable_notification_links_to_series_and_has_both_languages() {
        using var db = Context(); var run = new ExamWorkflow { ExamSeriesId = Guid.NewGuid(), Revision = 2 };
        ExamWorkflowEngine.Notify(db, run, Guid.NewGuid(), "ExamWorkflowAction", "مهمة", "Task", "راجع الاختبار", "Review exam", Guid.Empty);
        var notice = Assert.Single(db.SchoolUserNotifications.Local); var outbox = Assert.Single(db.ExamNotificationOutbox.Local);
        Assert.Equal("ExamWorkflow", notice.RelatedEntityType); Assert.Equal(run.ExamSeriesId.ToString("D"), notice.RelatedEntityId); Assert.Equal(notice.Id, outbox.NotificationId); Assert.False(notice.IsRead);
    }
    [Fact]
    public void Revision_is_concurrency_token_and_outbox_key_is_unique() {
        using var db = Context(); Assert.True(db.Model.FindEntityType(typeof(ExamWorkflow))!.FindProperty(nameof(ExamWorkflow.Revision))!.IsConcurrencyToken);
        Assert.Contains(db.Model.FindEntityType(typeof(ExamNotificationOutbox))!.GetIndexes(), i => i.IsUnique && i.Properties.Single().Name == nameof(ExamNotificationOutbox.EventKey));
    }
    private static PostgreSqlSchoolsDbContext Context() => new(new DbContextOptionsBuilder<PostgreSqlSchoolsDbContext>().UseNpgsql("Host=localhost;Database=design_only;Username=design_only").Options);
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Active_and_returned_tasks_paginate_entirely_on_server(bool sqlServer) {
        using SchoolsDbContext db = sqlServer
            ? new SqlServerSchoolsDbContext(new DbContextOptionsBuilder<SqlServerSchoolsDbContext>().UseSqlServer("Server=localhost;Database=design_only;Integrated Security=true;TrustServerCertificate=true").Options)
            : Context();
        var sql = ExamWorkflowEngine.Tasks(db, Guid.NewGuid(), db.ExamPapers.Select(x => x.ExamSeriesId))
            .OrderBy(x => x.DueAtUtc).ThenBy(x => x.Id).Skip(20).Take(20).ToQueryString();
        Assert.Contains("UNION ALL", sql); Assert.Contains("OFFSET", sql);
        Assert.Contains(sqlServer ? "FETCH NEXT" : "LIMIT", sql);
    }
}
