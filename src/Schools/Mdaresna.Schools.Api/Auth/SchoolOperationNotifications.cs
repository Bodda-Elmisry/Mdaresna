using Mdaresna.Schools.Api.Time;
using Mdaresna.Schools.Domain.Academics;
using Mdaresna.Schools.Domain.Identity;
using Mdaresna.Schools.Infrastructure.Identity;
using Mdaresna.Schools.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Mdaresna.Schools.Domain.Students;

namespace Mdaresna.Schools.Api.Auth;

public static class SchoolOperationNotifications
{
    public static string DayAr(DayOfWeek day) => day switch { DayOfWeek.Sunday => "الأحد", DayOfWeek.Monday => "الاثنين", DayOfWeek.Tuesday => "الثلاثاء", DayOfWeek.Wednesday => "الأربعاء", DayOfWeek.Thursday => "الخميس", DayOfWeek.Friday => "الجمعة", _ => "السبت" };
    public static void AssignmentRemoved(SchoolsDbContext db, Guid user, TeacherAssignmentDetail detail, DateTimeOffset now) =>
        Add(db, user, Guid.Empty, $"assignment:{detail.Kind}:{detail.Id:D}:removed:{now.Ticks}", "TeacherAssignmentRemoved",
            "إلغاء إسناد داخل المدرسة", "School assignment removed", $"تم إلغاء الإسناد التالي: {detail.BodyAr.Replace("تم إسناد", "")}. راجع تكليفاتك الحالية.",
            $"The following assignment was removed: {detail.BodyEn}. Review your current assignments.", "TeacherAssignment", $"{detail.Kind}/{detail.Id:D}", now);
    public static IQueryable<LocalUserAccount> AuthorizedUsers(SchoolsDbContext db, string permission) => db.LocalUsers.Where(u => u.Status == LocalUserStatus.Active &&
        db.LocalUserRoles.Any(r => r.UserId == u.Id && r.Role.IsActive && r.Role.Permissions.Any(p => p.Permission.IsActive && p.Permission.Code == permission)));

    public static Task<Guid[]> Teachers(SchoolsDbContext db, Guid classId, Guid? offeringId, string permission, CancellationToken ct) =>
        AuthorizedUsers(db, permission).Where(u => db.ClassSectionTeacherScopes.Any(s => s.IsActive && s.ClassSectionId == classId &&
            s.TeacherGradeSubjectScope.IsActive && s.TeacherGradeSubjectScope.TeacherUserId == u.Id &&
            (!offeringId.HasValue || s.TeacherGradeSubjectScope.GradeSubjectOfferingId == offeringId)))
            .Select(u => u.Id).Distinct().ToArrayAsync(ct);

    public static Task<Guid[]> Admins(SchoolsDbContext db, string permission, CancellationToken ct) => AuthorizedUsers(db, permission)
        .Where(u => db.LocalUserRoles.Any(r => r.UserId == u.Id && r.Role.IsActive && r.RoleId == SchoolIdentitySeed.SchoolAdminRoleId))
        .Select(u => u.Id).ToArrayAsync(ct);

    public static async Task<Guid[]> Supervisors(SchoolsDbContext db, IReadOnlyCollection<Guid> teacherIds, string permission, CancellationToken ct)
    {
        var zone = await db.SchoolInformation.AsNoTracking().Select(x => x.TimeZoneId).SingleOrDefaultAsync(ct);
        var today = new SchoolClock(TimeProvider.System).Now(string.IsNullOrWhiteSpace(zone) ? "UTC" : zone).Date;
        var roots = await db.DepartmentMemberships.AsNoTracking().Where(m => teacherIds.Contains(m.UserId) && m.IsActive && m.StartsOn <= today &&
            (!m.EndsOn.HasValue || m.EndsOn >= today) && m.Department.IsActive).Select(m => m.DepartmentId).Distinct().ToArrayAsync(ct);
        var hierarchy = await db.SchoolDepartments.AsNoTracking().Where(d => d.IsActive).Select(d => new { d.Id, d.ParentDepartmentId }).ToArrayAsync(ct);
        var departmentIds = roots.ToHashSet();
        bool added;
        do { added = false; foreach (var d in hierarchy.Where(d => departmentIds.Contains(d.Id) && d.ParentDepartmentId.HasValue)) added |= departmentIds.Add(d.ParentDepartmentId!.Value); } while (added);
        var heads = await AuthorizedUsers(db, permission).Where(u => db.DepartmentLeaderships.Any(l => l.UserId == u.Id && departmentIds.Contains(l.DepartmentId) &&
            l.IsActive && l.StartsOn <= today && (!l.EndsOn.HasValue || l.EndsOn >= today) && l.Department.IsActive &&
            db.DepartmentMemberships.Any(m => m.DepartmentId == l.DepartmentId && m.UserId == u.Id && m.IsActive && m.StartsOn <= today && (!m.EndsOn.HasValue || m.EndsOn >= today))))
            .Select(u => u.Id).Distinct().ToArrayAsync(ct);
        return heads.Length > 0 ? heads : await Admins(db, permission, ct);
    }

    public static bool Add(SchoolsDbContext db, Guid user, Guid actor, string eventKey, string type, string titleAr, string titleEn,
        string bodyAr, string bodyEn, string entityType, string entityId, DateTimeOffset now)
    {
        if (user == Guid.Empty || user == actor || db.SchoolUserNotifications.Local.Any(n => n.RecipientUserId == user && n.EventKey == eventKey)) return false;
        db.SchoolUserNotifications.Add(new SchoolUserNotification { Id = Guid.NewGuid(), RecipientUserId = user, EventKey = eventKey,
            Type = type, TitleAr = Clip(titleAr, 200), TitleEn = Clip(titleEn, 200), BodyAr = Clip(bodyAr, 1000), BodyEn = Clip(bodyEn, 1000),
            RelatedEntityType = entityType, RelatedEntityId = entityId, CreatedAtUtc = now, UpdatedAtUtc = now });
        return true;
    }
    private static string Clip(string value, int max) => value.Length <= max ? value : value[..(char.IsHighSurrogate(value[max - 2]) ? max - 2 : max - 1)] + "…";

    // Concurrent roster additions must not roll back enrollment merely because a daily digest already exists.
    public static async Task Save(SchoolsDbContext db, CancellationToken ct)
    {
        for (var attempt = 0; ; attempt++)
        {
            try { await db.SaveChangesAsync(ct); return; }
            catch (DbUpdateException ex) when (attempt < 2 && IsDuplicateEvent(ex))
            {
                var pending = db.SchoolUserNotifications.Local.Where(n => db.Entry(n).State == EntityState.Added && n.EventKey != null).ToArray();
                var removed = false;
                foreach (var n in pending)
                    if (await db.SchoolUserNotifications.AsNoTracking().AnyAsync(x => x.RecipientUserId == n.RecipientUserId && x.EventKey == n.EventKey, ct))
                    { db.Entry(n).State = EntityState.Detached; removed = true; }
                if (!removed) throw;
            }
        }
    }
    public static bool IsDuplicateEvent(DbUpdateException ex) =>
        ex.InnerException is Npgsql.PostgresException pg && pg.SqlState == "23505" && pg.ConstraintName == "IX_school_user_notifications_RecipientUserId_EventKey" ||
        ex.InnerException is Microsoft.Data.SqlClient.SqlException sql && sql.Number is 2601 or 2627 && sql.Message.Contains("IX_school_user_notifications_RecipientUserId_EventKey", StringComparison.Ordinal);

    public static async Task Send(SchoolsDbContext db, IEnumerable<Guid> users, Guid actor, string permission, string eventKey, string type,
        string titleAr, string titleEn, string bodyAr, string bodyEn, string entityType, string entityId, DateTimeOffset now, CancellationToken ct)
    {
        var ids = users.Distinct().Where(x => x != actor).ToArray();
        var eligible = await AuthorizedUsers(db, permission).Where(u => ids.Contains(u.Id)).Select(u => u.Id).ToArrayAsync(ct);
        var sent = await db.SchoolUserNotifications.AsNoTracking().Where(n => n.EventKey == eventKey && eligible.Contains(n.RecipientUserId)).Select(n => n.RecipientUserId).ToArrayAsync(ct);
        foreach (var user in eligible.Except(sent)) Add(db, user, actor, eventKey, type, titleAr, titleEn, bodyAr, bodyEn, entityType, entityId, now);
    }

    public static async Task Plan(SchoolsDbContext db, TeachingPlan plan, string action, Guid actor, DateTimeOffset now, CancellationToken ct)
    {
        var classIds = plan.Targets.Select(t => t.ClassSectionId).ToArray();
        var teachers = await db.TeacherGradeSubjectScopes.AsNoTracking().Where(s => s.IsActive && s.GradeSubjectOfferingId == plan.GradeSubjectOfferingId &&
            (classIds.Length == 0 || db.ClassSectionTeacherScopes.Any(c => c.IsActive && c.TeacherGradeSubjectScopeId == s.Id && classIds.Contains(c.ClassSectionId))))
            .Select(s => s.TeacherUserId).Distinct().ToArrayAsync(ct);
        var users = action == "submit" ? await Supervisors(db, teachers.Append(plan.CreatedByUserId).ToArray(), "school.academics.manage", ct)
            : action == "publish" ? teachers.Append(plan.CreatedByUserId).ToArray() : new[] { plan.CreatedByUserId, plan.SubmittedByUserId ?? plan.CreatedByUserId };
        // Match the plan endpoint's actual access predicate, including current department periods.
        var today = DateOnly.FromDateTime(now.UtcDateTime);
        users = await AuthorizedUsers(db, "school.academics.view").Where(u => users.Contains(u.Id) &&
            (db.LocalUserRoles.Any(r => r.UserId == u.Id && r.Role.IsActive && r.RoleId == SchoolIdentitySeed.SchoolAdminRoleId) ||
             db.ClassSections.Any(c => c.IsActive && c.GradeOffering.IsActive && (classIds.Length == 0 ? c.GradeOfferingId == plan.GradeOfferingId : classIds.Contains(c.Id)) &&
                db.ClassSectionTeacherScopes.Any(s => s.ClassSectionId == c.Id && s.IsActive && s.TeacherGradeSubjectScope.IsActive &&
                 (s.TeacherGradeSubjectScope.TeacherUserId == u.Id || s.TeacherGradeSubjectScope.TeacherUser.DepartmentMemberships.Any(m => m.IsActive && m.StartsOn <= today && (!m.EndsOn.HasValue || m.EndsOn >= today) &&
                    (m.Department.Leaderships.Any(l => l.UserId == u.Id && l.IsActive && l.StartsOn <= today && (!l.EndsOn.HasValue || l.EndsOn >= today)) ||
                     m.Department.ParentDepartment != null && m.Department.ParentDepartment.Leaderships.Any(l => l.UserId == u.Id && l.IsActive && l.StartsOn <= today && (!l.EndsOn.HasValue || l.EndsOn >= today)))))))))
            .Select(u => u.Id).ToArrayAsync(ct);
        if (action == "submit" && users.All(u => u == actor)) users = await Admins(db, "school.academics.manage", ct);
        var ar = action switch { "submit" => "خطة تحتاج مراجعتك", "approve" => "تم اعتماد خطة التدريس", "return" => "أعيدت خطة التدريس للتعديل", "publish" => "تم نشر خطة التدريس", _ => "تمت أرشفة خطة التدريس" };
        var en = action switch { "submit" => "Teaching plan needs review", "approve" => "Teaching plan approved", "return" => "Teaching plan returned", "publish" => "Teaching plan published", _ => "Teaching plan archived" };
        var reason = action == "return" ? db.TeachingPlanAudits.Local.LastOrDefault(a => a.TeachingPlanId == plan.Id && a.Action == "Returned")?.Details : null;
        await Send(db, users, actor, "school.academics.view", $"plan:{plan.Id:D}:{plan.Revision}:{action}", "TeachingPlanChanged", ar, en,
            $"{plan.TitleAr} — الفترة {plan.FromDate:yyyy-MM-dd} إلى {plan.ToDate:yyyy-MM-dd}. {(reason is null ? "افتح الخطة لمراجعة حالتها والإجراء المتاح." : $"سبب الإعادة: {reason}")}",
            $"{plan.TitleEn} — {plan.FromDate:yyyy-MM-dd} to {plan.ToDate:yyyy-MM-dd}. {(reason is null ? "Open the plan to review its current status and available action." : $"Return reason: {reason}")}",
            "TeachingPlan", plan.Id.ToString("D"), now, ct);
    }

    public static async Task RosterAdded(SchoolsDbContext db, Guid classId, Guid actor, string eventKey, DateTimeOffset now, CancellationToken ct)
    {
        var section = await db.ClassSections.AsNoTracking().Where(s => s.Id == classId).Select(s => new { s.NameAr, s.NameEn }).SingleAsync(ct);
        var users = await Teachers(db, classId, null, "school.academics.view", ct);
        // One roster digest per recipient and school-local day; always opens the current roster.
        var zone = await db.SchoolInformation.AsNoTracking().Select(x => x.TimeZoneId).SingleOrDefaultAsync(ct);
        var day = new SchoolClock(TimeProvider.System).Now(string.IsNullOrWhiteSpace(zone) ? "UTC" : zone).Date;
        var key = $"roster:{classId:D}:{day:yyyy-MM-dd}";
        await Send(db, users, actor, "school.academics.view", key, "ClassRosterChanged", "تحديث قائمة طلاب الفصل", "Class roster updated",
            $"أضيف طلاب إلى {section.NameAr}. افتح الفصل لمراجعة القائمة الحالية؛ قوائم الاختبارات والواجبات المنشورة تحتاج مزامنة صريحة عند إتاحتها.",
            $"Students were added to {section.NameEn}. Review the current roster; published exam and homework audiences require explicit synchronization where supported.",
            "ClassWorkspace", $"{classId:D}/attendance", now, ct);
    }

    public static async Task Activity(SchoolsDbContext db, ClassActivity activity, string action, Guid actor, DateTimeOffset now, CancellationToken ct)
    {
        var users = (await Teachers(db, activity.ClassSectionId, activity.ClassSectionSubject?.GradeSubjectOfferingId, "school.activities.view", ct))
            .Concat(await Admins(db, "school.activities.view", ct));
        var ar = action switch { "publish" => "نشاط جديد للفصل", "cancel" => "تم إلغاء نشاط الفصل", _ => "أعيد فتح تقييم النشاط" };
        var en = action switch { "publish" => "Class activity published", "cancel" => "Class activity cancelled", _ => "Activity evaluation reopened" };
        await Send(db, users, actor, "school.activities.view", $"activity:{activity.Id:D}:{action}:{now.Ticks}", "ClassActivityChanged", ar, en,
            $"{activity.Title} — يوم {activity.ActivityDate:yyyy-MM-dd}. افتح النشاط لمراجعة التفاصيل والحالة الحالية.",
            $"{activity.Title} — {activity.ActivityDate:yyyy-MM-dd}. Open the activity to review details and current status.", "ClassActivity", activity.Id.ToString("D"), now, ct);
    }

    public static async Task FollowUp(SchoolsDbContext db, StudentLessonNote note, bool required, Guid actor, DateTimeOffset now, CancellationToken ct)
    {
        var recipients = (await Supervisors(db, new[] { note.CreatedByUserId }, "school.lesson_notes.view", ct)).Append(note.CreatedByUserId);
        await Send(db, recipients, actor, "school.lesson_notes.view", $"note:{note.Id:D}:{required}:{now.Ticks}", "LessonNoteFollowUp",
            required ? "ملاحظة طالب تحتاج متابعة" : "انتهت الحاجة لمتابعة الملاحظة", required ? "Student note needs follow-up" : "Student note follow-up cleared",
            $"تغيرت حالة متابعة ملاحظة الحصة بتاريخ {note.LessonDate:yyyy-MM-dd}. افتح الملاحظة للاطلاع داخل المدرسة؛ محتواها غير معروض في الإشعار.",
            $"A lesson note dated {note.LessonDate:yyyy-MM-dd} changed follow-up status. Open the note inside school; its content is not shown in notifications.",
            "ClassWorkspace", $"{note.ClassSectionId:D}/notes/{note.LessonDate:yyyy-MM-dd}/{note.WeeklyTimetableSlotId:D}", now, ct);
    }

    public static async Task Admission(SchoolsDbContext db, AdmissionApplication application, Guid actor, IConfiguration configuration, string schoolCode, DateTimeOffset now, CancellationToken ct)
    {
        var configured = configuration.GetSection($"SchoolNotifications:AdmissionsRecipients:{schoolCode}").Get<Guid[]>() ?? [];
        var users = configured.Length > 0 ? await AuthorizedUsers(db, "school.admissions.manage").Where(u => configured.Contains(u.Id)).Select(u => u.Id).ToArrayAsync(ct)
            : await Admins(db, "school.admissions.manage", ct);
        var statusAr = application.Status switch { AdmissionApplicationStatus.Submitted => "مقدم", AdmissionApplicationStatus.UnderReview => "قيد المراجعة", AdmissionApplicationStatus.InterviewScheduled => "مقابلة مجدولة", AdmissionApplicationStatus.Waitlisted => "قائمة الانتظار", AdmissionApplicationStatus.Accepted => "مقبول", AdmissionApplicationStatus.Rejected => "مرفوض", _ => "منسحب" };
        await Send(db, users, actor, "school.admissions.view", $"admission:{application.Id:D}:{application.Status}:{now.Ticks}", "AdmissionChanged",
            application.Status == AdmissionApplicationStatus.Submitted ? "طلب تقديم يحتاج مراجعة" : "تحديث طلب تقديم", "Admission application update",
            $"الطلب {application.ApplicationNumber} — الحالة: {statusAr}. افتح الطلب لمراجعة التفاصيل والإجراء المناسب.",
            $"Application {application.ApplicationNumber} — status: {application.Status}. Review the application and available action.", "AdmissionApplication", application.Id.ToString("D"), now, ct);
    }
}
