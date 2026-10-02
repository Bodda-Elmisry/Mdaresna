using System.Text.Json;
using Mdaresna.Schools.Domain.Exams;
using Mdaresna.Schools.Domain.Identity;
using Mdaresna.Schools.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
namespace Mdaresna.Schools.Api.Exams;
public sealed record WorkflowTemplate(ExamWorkflowStage Stage, Guid? PaperId, int Order, string NameAr, string NameEn);
public sealed record WorkflowAssignment(ExamWorkflowStage Stage, Guid? PaperId, Guid AssigneeUserId, Guid? BackupUserId, Guid? SupervisorUserId, DateTimeOffset? DueAtUtc);
public sealed record WorkflowDefault(ExamWorkflowStage Stage, Guid AssigneeUserId);
public sealed class WorkflowTask {
    public Guid Id { get; init; }
    public Guid ExamSeriesId { get; init; }
    public ExamWorkflowStage Stage { get; init; }
    public Guid? ExamPaperId { get; init; }
    public DateTimeOffset? DueAtUtc { get; init; }
    public ExamWorkflowStepStatus Status { get; init; }
    public int Revision { get; init; }
    public string NameAr { get; init; } = "";
    public string NameEn { get; init; } = "";
    public bool NeedsRevision { get; init; }
}
public static class ExamWorkflowEngine
{
    public static IQueryable<WorkflowTask> Tasks(SchoolsDbContext db, Guid actor, IQueryable<Guid> accessible) {
        var active = db.ExamWorkflowSteps.AsNoTracking().Where(x => x.AssigneeUserId == actor && x.Status == ExamWorkflowStepStatus.Active && x.Workflow.Status == ExamWorkflowStatus.Active && accessible.Contains(x.ExamSeriesId))
            .Select(x => new WorkflowTask { Id = x.Id, ExamSeriesId = x.ExamSeriesId, Stage = x.Stage, ExamPaperId = x.ExamPaperId, DueAtUtc = x.DueAtUtc,
                Status = x.Status, Revision = x.Workflow.Revision, NameAr = x.Workflow.Series.NameAr, NameEn = x.Workflow.Series.NameEn, NeedsRevision = false });
        var returned = db.ExamWorkflows.AsNoTracking().Where(x => x.Series.CreatedByUserId == actor && x.Status == ExamWorkflowStatus.Returned && accessible.Contains(x.ExamSeriesId))
            .Select(x => new WorkflowTask { Id = x.ExamSeriesId, ExamSeriesId = x.ExamSeriesId, Stage = ExamWorkflowStage.Schedule, ExamPaperId = null, DueAtUtc = null,
                Status = ExamWorkflowStepStatus.Waiting, Revision = x.Revision, NameAr = x.Series.NameAr, NameEn = x.Series.NameEn, NeedsRevision = true });
        return active.Concat(returned);
    }
    public static bool Flag(ExamSeries s, string name) {
        if (string.IsNullOrWhiteSpace(s.PolicySnapshotJson)) return false;
        using var j = JsonDocument.Parse(s.PolicySnapshotJson);
        return j.RootElement.TryGetProperty(name, out var v) && v.ValueKind == JsonValueKind.True;
    }
    public static List<WorkflowTemplate> Templates(ExamSeries s) {
        var r = new List<WorkflowTemplate> { new(ExamWorkflowStage.Schedule, null, 0, "تجهيز الجدول", "Prepare schedule") };
        if (s.Papers.Any(p => p.Sittings.Any(x => x.AdministrationMode == ExamAdministrationMode.Committee))) r.Add(new(ExamWorkflowStage.Committees, null, 1, "تجهيز اللجان والمراقبين", "Prepare committees and invigilators"));
        if (Flag(s, "DepartmentApprovalRequired")) foreach (var p in s.Papers.OrderBy(x => x.Id)) r.Add(new(ExamWorkflowStage.DepartmentReview, p.Id, 2, $"مراجعة القسم — {p.TitleAr}", $"Department review — {p.TitleEn}"));
        if (Flag(s, "SchoolApprovalRequired")) r.Add(new(ExamWorkflowStage.SchoolApproval, null, 3, "اعتماد المدرسة", "School approval"));
        r.Add(new(ExamWorkflowStage.Publish, null, 4, "نشر الاختبار", "Publish exam")); return r;
    }
    public static Task<bool> Permission(SchoolsDbContext db, Guid user, string permission, CancellationToken ct) => db.LocalUserRoles.AnyAsync(r => r.UserId == user && r.User.Status == LocalUserStatus.Active && r.Role.IsActive && r.Role.Permissions.Any(p => p.Permission.IsActive && p.Permission.Code == permission), ct);
    public static async Task<bool> Eligible(SchoolsDbContext db, Guid user, ExamWorkflowStage stage, IReadOnlyList<Guid> papers, CancellationToken ct) {
        if (!await Permission(db, user, ExamWorkflowRules.Permission(stage), ct) || !await Permission(db, user, "school.exams.view", ct)) return false;
        if (await db.LocalUserRoles.AnyAsync(r => r.UserId == user && r.RoleId == SchoolIdentitySeed.SchoolAdminRoleId && r.Role.IsActive, ct)) return true;
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        return await db.ExamPapers.CountAsync(p => papers.Contains(p.Id) && (
            db.DepartmentLeaderships.Any(l => l.UserId == user && l.IsActive && !l.IsDeleted && l.StartsOn <= today && (!l.EndsOn.HasValue || l.EndsOn >= today) && l.Department.Subjects.Any(s => s.IsActive && !s.IsDeleted && s.SubjectId == p.GradeSubjectOffering.CurriculumGradeSubject.SubjectId)) ||
            db.SubjectCoordinatorAssignments.Any(c => c.CoordinatorUserId == user && c.IsActive && !c.IsDeleted && c.StartsOn <= today && (!c.EndsOn.HasValue || c.EndsOn >= today) && c.DepartmentSubject.SubjectId == p.GradeSubjectOffering.CurriculumGradeSubject.SubjectId) ||
            stage != ExamWorkflowStage.DepartmentReview && p.Targets.Any(t => t.ClassSectionId.HasValue && db.ClassSectionTeacherScopes.Any(s => s.ClassSectionId == t.ClassSectionId && s.IsActive && !s.IsDeleted && s.TeacherGradeSubjectScope.IsActive && !s.TeacherGradeSubjectScope.IsDeleted && s.TeacherGradeSubjectScope.TeacherUserId == user && s.TeacherGradeSubjectScope.GradeSubjectOfferingId == p.GradeSubjectOfferingId))), ct) == papers.Count;
    }
    public static string StageAr(ExamWorkflowStage s) => s switch { ExamWorkflowStage.Schedule => "تجهيز الجدول", ExamWorkflowStage.Committees => "تجهيز اللجان والمراقبين", ExamWorkflowStage.DepartmentReview => "مراجعة القسم", ExamWorkflowStage.SchoolApproval => "اعتماد المدرسة", _ => "نشر الاختبار" };
    public static void Notify(SchoolsDbContext db, ExamWorkflow run, Guid user, string type, string ar, string en, string bodyAr, string bodyEn, Guid actor, Guid? step = null) {
        if (user == actor) return;
        var now = DateTimeOffset.UtcNow;
        var n = new SchoolUserNotification { Id = Guid.NewGuid(), RecipientUserId = user, Type = type, TitleAr = ar, TitleEn = en, BodyAr = bodyAr, BodyEn = bodyEn, RelatedEntityType = "ExamWorkflow", RelatedEntityId = run.ExamSeriesId.ToString("D"), CreatedAtUtc = now, UpdatedAtUtc = now };
        db.SchoolUserNotifications.Add(n);
        db.ExamNotificationOutbox.Add(new ExamNotificationOutbox { Id = Guid.NewGuid(), EventKey = $"{run.ExamSeriesId:D}:{run.Revision}:{type}:{step}:{user:D}", NotificationId = n.Id, CreatedAtUtc = now });
    }
    public static void Advance(SchoolsDbContext db, ExamWorkflow run, Guid actor) {
        var waiting = run.Steps.Where(s => s.Status == ExamWorkflowStepStatus.Waiting).OrderBy(s => s.Order).ToList();
        if (run.Steps.Any(s => s.Status == ExamWorkflowStepStatus.Active) || waiting.Count == 0) return;
        var order = waiting[0].Order; if (!ExamWorkflowRules.AllEarlierCompleted(run.Steps, order)) return;
        foreach (var s in waiting.Where(s => s.Order == order)) {
            s.Status = ExamWorkflowStepStatus.Active;
            Notify(db, run, s.AssigneeUserId, "ExamWorkflowAction", $"اختبار بانتظار {StageAr(s.Stage)}", "Exam awaiting your action", $"{run.Series.NameAr} — المطلوب: {StageAr(s.Stage)}. افتح المهمة لمراجعة التفاصيل.", $"{run.Series.NameEn} — {s.Stage}. Open the task to review details.", actor, s.Id);
        }
        run.Series.Status = order >= 4 ? ExamSeriesStatus.Approved : order >= 2 ? ExamSeriesStatus.PendingApproval : ExamSeriesStatus.Draft; run.Series.UpdatedAtUtc = DateTimeOffset.UtcNow;
    }
    public static async Task<bool> CanPublish(SchoolsDbContext db, Guid seriesId, Guid actor, CancellationToken ct) {
        var run = await db.ExamWorkflows.Include(x => x.Steps).SingleOrDefaultAsync(x => x.ExamSeriesId == seriesId, ct);
        if (run is null) return true;
        var s = run.Steps.SingleOrDefault(x => x.Stage == ExamWorkflowStage.Publish && x.Status == ExamWorkflowStepStatus.Active);
        return run.Status == ExamWorkflowStatus.Active && s is not null && s.AssigneeUserId == actor && ExamWorkflowRules.AllEarlierCompleted(run.Steps, s.Order)
            && await Eligible(db, actor, ExamWorkflowStage.Publish, await db.ExamPapers.Where(p => p.ExamSeriesId == seriesId).Select(p => p.Id).ToArrayAsync(ct), ct);
    }
    public static async Task Published(SchoolsDbContext db, ExamSeries series, Guid actor, CancellationToken ct) {
        var run = await db.ExamWorkflows.Include(x => x.Steps).SingleOrDefaultAsync(x => x.ExamSeriesId == series.Id, ct);
        run ??= new ExamWorkflow { ExamSeriesId = series.Id, Series = series, CreatedByUserId = series.CreatedByUserId, Revision = 0 };
        if (run.Revision == 0) db.ExamWorkflows.Add(run);
        run.Revision++; run.Status = ExamWorkflowStatus.Completed; run.UpdatedAtUtc = DateTimeOffset.UtcNow;
        foreach (var s in run.Steps.Where(x => x.Status == ExamWorkflowStepStatus.Active)) { s.Status = ExamWorkflowStepStatus.Completed; s.CompletedAtUtc = run.UpdatedAtUtc; }
        var users = await OperationalRecipients(db, series.Id, ct);
        users.AddRange(await db.ExamInvigilatorAssignments.Where(x => x.ExamCommittee.ExamSeriesId == series.Id && x.Status == ExamInvigilatorStatus.Assigned).Select(x => x.UserId).ToListAsync(ct)); users.AddRange(run.Steps.Select(x => x.AssigneeUserId)); users.Add(series.CreatedByUserId);
        foreach (var user in users.Distinct()) if (await Permission(db, user, "school.exams.view", ct)) Notify(db, run, user, "ExamPublished", "تم نشر الاختبار", "Exam published", $"تم نشر {series.NameAr}. راجع موعد الاختبار وتفاصيل التنفيذ الخاصة بك.", $"{series.NameEn} is published. Review your exam schedule and duties.", actor);
    }
    public static async Task Cancelled(SchoolsDbContext db, ExamSeries series, Guid actor, CancellationToken ct) {
        var run = await db.ExamWorkflows.Include(x => x.Steps).SingleOrDefaultAsync(x => x.ExamSeriesId == series.Id, ct);
        if (run is null) { run = new ExamWorkflow { ExamSeriesId = series.Id, Series = series, CreatedByUserId = series.CreatedByUserId, Revision = 0 }; db.ExamWorkflows.Add(run); }
        run.Revision++; run.Status = ExamWorkflowStatus.Cancelled; run.UpdatedAtUtc = DateTimeOffset.UtcNow;
        foreach (var s in run.Steps.Where(x => x.Status != ExamWorkflowStepStatus.Completed)) s.Status = ExamWorkflowStepStatus.Cancelled;
        foreach (var user in (await OperationalRecipients(db, series.Id, ct)).Concat(run.Steps.Select(x => x.AssigneeUserId)).Append(series.CreatedByUserId).Distinct())
            if (await Permission(db, user, "school.exams.view", ct)) Notify(db, run, user, "ExamCancelled", "تم إلغاء الاختبار", "Exam cancelled", $"{series.NameAr} — السبب: {series.CancellationReason}", $"{series.NameEn} — {series.CancellationReason}", actor);
    }
    private static async Task<List<Guid>> OperationalRecipients(SchoolsDbContext db, Guid id, CancellationToken ct) {
        var users = await db.ClassSectionTeacherScopes.Where(t => t.IsActive && !t.IsDeleted && t.TeacherGradeSubjectScope.IsActive && !t.TeacherGradeSubjectScope.IsDeleted &&
            db.ExamPapers.Any(p => p.ExamSeriesId == id && p.GradeSubjectOfferingId == t.TeacherGradeSubjectScope.GradeSubjectOfferingId && p.Targets.Any(a => a.ClassSectionId == t.ClassSectionId)))
            .Select(t => t.TeacherGradeSubjectScope.TeacherUserId).ToListAsync(ct);
        users.AddRange(await db.ExamInvigilatorAssignments.Where(x => x.ExamCommittee.ExamSeriesId == id && x.Status == ExamInvigilatorStatus.Assigned).Select(x => x.UserId).ToListAsync(ct));
        return users;
    }
    public static async Task Rescheduled(SchoolsDbContext db, ExamSeries series, Guid actor, string oldDate, string newDate, string reason, CancellationToken ct) {
        var run = await db.ExamWorkflows.Include(x => x.Steps).SingleOrDefaultAsync(x => x.ExamSeriesId == series.Id, ct);
        if (run is null) { run = new ExamWorkflow { ExamSeriesId = series.Id, Series = series, CreatedByUserId = series.CreatedByUserId, Revision = 0 }; db.ExamWorkflows.Add(run); }
        run.Revision++; run.UpdatedAtUtc = DateTimeOffset.UtcNow;
        series.UpdatedAtUtc = run.UpdatedAtUtc;
        if (series.Status != ExamSeriesStatus.Published) {
            run.Status = ExamWorkflowStatus.Returned; series.Status = ExamSeriesStatus.Draft;
            series.ApprovedByUserId = null; series.ApprovedAtUtc = null;
            foreach (var s in run.Steps) { s.Status = ExamWorkflowStepStatus.Waiting; s.CompletedAtUtc = null; s.ReturnReason = reason; }
            foreach (var a in await db.ExamApprovals.Where(x => x.ExamSeriesId == series.Id).ToListAsync(ct)) a.Status = ExamApprovalStatus.Pending;
        } else run.Status = ExamWorkflowStatus.Completed;
        foreach (var user in (await OperationalRecipients(db, series.Id, ct)).Concat(run.Steps.Select(x => x.AssigneeUserId)).Append(series.CreatedByUserId).Distinct())
            if (await Permission(db, user, "school.exams.view", ct)) Notify(db, run, user, "ExamRescheduled", "تم تعديل موعد الاختبار", "Exam rescheduled", $"{series.NameAr} — من {oldDate} إلى {newDate}. السبب: {reason}.", $"{series.NameEn} — {oldDate} → {newDate}. Reason: {reason}.", actor);
    }
}
