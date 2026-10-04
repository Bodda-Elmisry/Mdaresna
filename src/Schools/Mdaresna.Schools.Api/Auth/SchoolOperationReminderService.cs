using Mdaresna.Schools.Domain.Exams;
using Mdaresna.Schools.Domain.Identity;
using Mdaresna.Schools.Domain.Students;
using Mdaresna.Schools.Infrastructure.Identity;
using Mdaresna.Schools.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Mdaresna.Schools.Api.Auth;

// Disabled without an explicit school allow-list. No optional, never-started evaluations are chased.
public sealed class SchoolOperationReminderService(IServiceScopeFactory scopes, IConfiguration configuration,
    TimeProvider time, ILogger<SchoolOperationReminderService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromMinutes(5));
        while (await timer.WaitForNextTickAsync(stoppingToken))
            foreach (var code in configuration.GetSection("SchoolNotifications:SchoolCodes").Get<string[]>() ?? [])
                try
                {
                    await using var scope = scopes.CreateAsyncScope();
                    var factory = scope.ServiceProvider.GetRequiredService<ISchoolDbContextFactory>();
                    await using var db = await factory.CreateAsync(code, stoppingToken); if (db is null) continue;
                    await Process(db, time.GetUtcNow(), stoppingToken);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { return; }
                catch (Exception) { logger.LogWarning("School operation reminder processing failed; retrying next interval."); }
    }

    public static int DueLevel(DateTimeOffset updated, DateTimeOffset now) => updated <= now.AddHours(-48) ? 2 : updated <= now.AddHours(-24) ? 1 : 0;

    public static IQueryable<StudentLessonEvaluationRegister> EvaluationTasks(SchoolsDbContext db, DateTimeOffset due, string type, DateTimeOffset? cursor, Guid last)
    {
        var query = db.StudentLessonEvaluationRegisters.AsNoTracking().Where(r => r.Status == StudentLessonEvaluationRegisterStatus.Draft && r.UpdatedAtUtc <= due &&
            r.WeeklyTimetableSlot.IsActive && !r.WeeklyTimetableSlot.IsBreak &&
            !db.SchoolUserNotifications.Any(n => n.Type == type && n.EventKey != null && n.EventKey.StartsWith("evaluation:" + r.Id.ToString() + ":") && n.CreatedAtUtc >= r.UpdatedAtUtc));
        if (cursor.HasValue) query = query.Where(r => r.UpdatedAtUtc > cursor || r.UpdatedAtUtc == cursor && r.Id.CompareTo(last) > 0);
        return query.OrderBy(r => r.UpdatedAtUtc).ThenBy(r => r.Id).Take(100);
    }

    public static async Task Process(SchoolsDbContext db, DateTimeOffset now, CancellationToken ct)
    {
        var cutoff = now.AddHours(-24);
        // Keyset batches bound memory and keep inaccessible old tasks from starving later tasks.
        foreach (var escalation in new[] { false, true })
        {
            var due = escalation ? now.AddHours(-48) : cutoff;
            var type = escalation ? "SchoolTaskEscalated" : "SchoolTaskOverdue";
            DateTimeOffset? cursor = null; Guid last = Guid.Empty;
            for (var batch = 0; batch < 10; batch++)
            {
                var rows = await EvaluationTasks(db, due, type, cursor, last).ToArrayAsync(ct);
                foreach (var r in rows)
                {
                    var replacement = await db.TeacherSubstitutions.AsNoTracking().Where(s => s.IsActive && s.WeeklyTimetableSlotId == r.WeeklyTimetableSlotId && s.LessonDate == r.LessonDate)
                        .Select(s => (Guid?)s.SubstituteTeacherAssignment.TeacherGradeSubjectScope.TeacherUserId).FirstOrDefaultAsync(ct);
                    var primary = await db.WeeklyTimetableSlots.Where(s => s.Id == r.WeeklyTimetableSlotId).Select(s => (Guid?)s.PrimaryTeacherScope!.TeacherUserId).SingleAsync(ct);
                    var teacher = replacement ?? primary;
                    if (!teacher.HasValue) continue;
                    var users = escalation ? await SchoolOperationNotifications.Supervisors(db, new[] { teacher.Value }, "school.lesson_evaluations.view", ct)
                        : (await SchoolOperationNotifications.Teachers(db, r.ClassSectionId, null, "school.lesson_evaluations.manage", ct)).Where(u => u == teacher).ToArray();
                    await Remind(db, users, "school.lesson_evaluations.view", $"evaluation:{r.Id:D}:{r.Revision}:{escalation}", type,
                        "تقييم حصة أعيد فتحه ولم يعتمد", "Reopened lesson evaluation awaiting finalization", "ClassWorkspace",
                        $"{r.ClassSectionId:D}/evaluation/{r.LessonDate:yyyy-MM-dd}/{r.WeeklyTimetableSlotId:D}", now, ct);
                }
                await db.SaveChangesAsync(ct);
                if (rows.Length < 100) break; cursor = rows[^1].UpdatedAtUtc; last = rows[^1].Id;
            }

            cursor = null; last = Guid.Empty;
            for (var batch = 0; batch < 10; batch++)
            {
                var query = db.ClassActivities.AsNoTracking().Include(a => a.ClassSectionSubject).Where(a => a.Status == ClassActivityStatus.Published && a.UpdatedAtUtc <= due &&
                    a.Participants.Any(p => p.Status == ActivityParticipationStatus.NotRecorded || a.IsGraded && p.Status == ActivityParticipationStatus.Participated && !p.Score.HasValue) &&
                    !db.SchoolUserNotifications.Any(n => n.Type == type && n.RelatedEntityType == "ClassActivity" && n.RelatedEntityId == a.Id.ToString() && n.CreatedAtUtc >= a.UpdatedAtUtc));
                if (cursor.HasValue) query = query.Where(a => a.UpdatedAtUtc > cursor || a.UpdatedAtUtc == cursor && a.Id.CompareTo(last) > 0);
                var rows = await query.OrderBy(a => a.UpdatedAtUtc).ThenBy(a => a.Id).Take(100).ToArrayAsync(ct);
                foreach (var a in rows)
                {
                    var zone = string.IsNullOrWhiteSpace(a.TimeZoneIdSnapshot) ? "UTC" : a.TimeZoneIdSnapshot;
                    var local = TimeZoneInfo.ConvertTime(now, TimeZoneInfo.FindSystemTimeZoneById(zone));
                    if (a.ActivityDate >= DateOnly.FromDateTime(local.DateTime)) continue;
                    var users = escalation ? await SchoolOperationNotifications.Admins(db, "school.activities.view", ct)
                        : await SchoolOperationNotifications.Teachers(db, a.ClassSectionId, a.ClassSectionSubject?.GradeSubjectOfferingId, "school.activities.grade", ct);
                    await Remind(db, users, "school.activities.view", $"activity-task:{a.Id:D}:{a.UpdatedAtUtc.Ticks}:{escalation}", type,
                        "نشاط سابق يحتاج استكمال المشاركة أو الدرجات", "Past activity awaiting participation or scores", "ClassActivity", a.Id.ToString("D"), now, ct);
                }
                await db.SaveChangesAsync(ct);
                if (rows.Length < 100) break; cursor = rows[^1].UpdatedAtUtc; last = rows[^1].Id;
            }

            cursor = null; last = Guid.Empty;
            for (var batch = 0; batch < 10; batch++)
            {
                var query = db.ExamPapers.AsNoTracking().Where(p => p.ResultsStatus == ExamResultsStatus.EntryOpen && p.UpdatedAtUtc <= due &&
                    !db.SchoolUserNotifications.Any(n => n.Type == type && n.EventKey != null && n.EventKey.StartsWith("results:" + p.Id.ToString() + ":") && n.CreatedAtUtc >= p.UpdatedAtUtc));
                if (cursor.HasValue) query = query.Where(p => p.UpdatedAtUtc > cursor || p.UpdatedAtUtc == cursor && p.Id.CompareTo(last) > 0);
                var rows = await query.OrderBy(p => p.UpdatedAtUtc).ThenBy(p => p.Id).Take(100).ToArrayAsync(ct);
                foreach (var p in rows)
                {
                    var users = escalation ? await SchoolOperationNotifications.Admins(db, "school.exams.view", ct) : await SchoolOperationNotifications.AuthorizedUsers(db, "school.exams.results.enter")
                        .Where(u => db.ClassSectionTeacherScopes.Any(s => s.IsActive && s.TeacherGradeSubjectScope.IsActive && s.TeacherGradeSubjectScope.TeacherUserId == u.Id && s.TeacherGradeSubjectScope.GradeSubjectOfferingId == p.GradeSubjectOfferingId &&
                            db.ExamPaperTargets.Any(t => t.ExamPaperId == p.Id && (t.ClassSectionId == s.ClassSectionId || !t.ClassSectionId.HasValue && t.GradeOfferingId == s.ClassSection.GradeOfferingId))))
                        .Select(u => u.Id).Distinct().ToArrayAsync(ct);
                    if (!escalation) users = users.Concat(await SchoolOperationNotifications.Admins(db, "school.exams.results.enter", ct)).Distinct().ToArray();
                    foreach (var user in users)
                    {
                        var isAdmin = await db.LocalUserRoles.AnyAsync(r => r.UserId == user && r.RoleId == SchoolIdentitySeed.SchoolAdminRoleId && r.Role.IsActive, ct);
                        var classId = await db.ClassSections.Where(c => c.IsActive && db.ExamPaperTargets.Any(t => t.ExamPaperId == p.Id && (t.ClassSectionId == c.Id || !t.ClassSectionId.HasValue && t.GradeOfferingId == c.GradeOfferingId)) &&
                            (isAdmin || db.ClassSectionTeacherScopes.Any(s => s.ClassSectionId == c.Id && s.IsActive && s.TeacherGradeSubjectScope.IsActive && s.TeacherGradeSubjectScope.TeacherUserId == user && s.TeacherGradeSubjectScope.GradeSubjectOfferingId == p.GradeSubjectOfferingId)))
                            .OrderBy(c => c.Id).Select(c => (Guid?)c.Id).FirstOrDefaultAsync(ct);
                        if (!classId.HasValue) continue;
                        await Remind(db, new[] { user }, "school.exams.view", $"results:{p.Id:D}:{p.UpdatedAtUtc.Ticks}:{escalation}", type,
                            $"{p.TitleAr} — الرصد مفتوح ولم يرسل للاعتماد", $"{p.TitleEn} — results awaiting submission", "ClassWorkspace", $"{classId:D}/exams", now, ct);
                    }
                }
                await db.SaveChangesAsync(ct);
                if (rows.Length < 100) break; cursor = rows[^1].UpdatedAtUtc; last = rows[^1].Id;
            }
        }
    }

    private static Task Remind(SchoolsDbContext db, Guid[] users, string permission, string key, string type, string ar, string en, string entity, string id, DateTimeOffset now, CancellationToken ct) =>
        SchoolOperationNotifications.Send(db, users, Guid.Empty, permission, key, type,
            type == "SchoolTaskEscalated" ? "مهمة مدرسية تحتاج متابعة" : "تذكير بمهمة مدرسية مفتوحة", type == "SchoolTaskEscalated" ? "School task needs follow-up" : "Open school task reminder",
            $"{ar}. افتح المهمة لمراجعة الحالة الحالية.", $"{en}. Open the task to review its current state.", entity, id, now, ct);
}
