using Mdaresna.Schools.Domain.Identity;
using Mdaresna.Schools.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Mdaresna.Schools.Domain.Academics;

namespace Mdaresna.Schools.Api.Auth;

public sealed record TeacherAssignmentDetail(string Kind, Guid Id, string TitleAr, string TitleEn, string BodyAr, string BodyEn);

public static class TeacherAssignmentNotifications
{
    public static IReadOnlyList<Guid> ChangedLessonScopes(Guid? previousPrimary, IReadOnlySet<Guid> previousSubstitutes,
        Guid primary, IReadOnlyList<Guid> substitutes, bool changed) =>
        substitutes.Where(x => changed || !previousSubstitutes.Contains(x))
            .Concat(previousPrimary != primary || changed ? new[] { primary } : []).Distinct().ToArray();

    private sealed record ScopeContext(Guid Id, string SubjectAr, string SubjectEn, string GradeAr, string GradeEn,
        string ProgramAr, string ProgramEn, string YearAr, string YearEn);
    private static IQueryable<ScopeContext> ScopeContexts(IQueryable<GradeSubjectOffering> query) => query.Select(x => new ScopeContext(x.Id,
        x.CurriculumGradeSubject.Subject.NameAr, x.CurriculumGradeSubject.Subject.NameEn,
        x.GradeOffering.GradeLevel.NameAr, x.GradeOffering.GradeLevel.NameEn,
        x.GradeOffering.ProgramAcademicYear.EducationProgram.NameAr, x.GradeOffering.ProgramAcademicYear.EducationProgram.NameEn,
        x.GradeOffering.ProgramAcademicYear.NameAr, x.GradeOffering.ProgramAcademicYear.NameEn));
    public static void Add(SchoolsDbContext db, Guid userId, TeacherAssignmentDetail detail, DateTimeOffset now)
    {
        db.SchoolUserNotifications.Add(new SchoolUserNotification
        {
            Id = Guid.NewGuid(), RecipientUserId = userId, Type = "TeacherAssignmentAdded",
            TitleAr = detail.TitleAr, TitleEn = detail.TitleEn, BodyAr = Preview(detail.BodyAr), BodyEn = Preview(detail.BodyEn),
            RelatedEntityType = "TeacherAssignment", RelatedEntityId = $"{detail.Kind}/{detail.Id:D}",
            CreatedAtUtc = now, UpdatedAtUtc = now
        });
    }
    private static string Preview(string text) => text.Length <= 1000 ? text : text[..(char.IsHighSurrogate(text[996]) ? 996 : 997)] + "…";

    public static TeacherAssignmentDetail Department(Guid id, string ar, string en, bool primary, DateOnly start, DateOnly? end) =>
        new("department", id, "إضافة إلى قسم", "Department membership assigned",
            $"تمت إضافتك إلى قسم {ar}{(primary ? " كقسمك الأساسي" : "")}. فترة العضوية: {start:yyyy-MM-dd} — {(end.HasValue ? end.Value.ToString("yyyy-MM-dd") : "مستمرة")}. راجع تفاصيل القسم في تكليفاتي.",
            $"You were added to {en}{(primary ? " as your primary department" : "")}. Membership: {start:yyyy-MM-dd} — {(end.HasValue ? end.Value.ToString("yyyy-MM-dd") : "ongoing")}. View your department in My assignments.");

    public static async Task<TeacherAssignmentDetail> Scope(SchoolsDbContext db, Guid offeringId, Guid targetId, Guid? classId, CancellationToken ct)
    {
        var row = await ScopeContexts(db.GradeSubjectOfferings.AsNoTracking().Where(x => x.Id == offeringId)).SingleAsync(ct);
        var section = classId.HasValue ? await db.ClassSections.AsNoTracking().Where(x => x.Id == classId).Select(x => new { x.NameAr, x.NameEn }).SingleAsync(ct) : null;
        return ScopeDetail(row, targetId, section?.NameAr, section?.NameEn);
    }

    private static TeacherAssignmentDetail ScopeDetail(ScopeContext row, Guid targetId, string? classAr, string? classEn) => new(classAr is null ? "grade" : "class", targetId,
        classAr is null ? "إسناد مادة وصف" : "إسناد مادة داخل فصل", classAr is null ? "Grade subject assigned" : "Class subject assigned",
        $"تم إسناد مادة {row.SubjectAr} إليك للصف {row.GradeAr}{(classAr is null ? "" : $"، فصل {classAr}")} — {row.ProgramAr} — {row.YearAr}. {(classAr is null ? "هذا نطاق تدريس؛ إسناد الفصول والحصص يتم بشكل منفصل." : "يمكنك متابعة الفصل ضمن صلاحياتك؛ مواعيد حصصك تحدد في الجدول.")}",
        $"You were assigned {row.SubjectEn} for {row.GradeEn}{(classAr is null ? "" : $", class {classEn}")} — {row.ProgramEn} — {row.YearEn}. {(classAr is null ? "Classes and lessons are assigned separately." : "Access the class within your permissions; lesson times are assigned in the timetable.")}");

    public static TeacherAssignmentDetail Lesson(Guid id, string classAr, string classEn, string subjectAr, string subjectEn,
        string gradeAr, string gradeEn, string yearAr, string yearEn, DayOfWeek day, int number, TimeOnly start, TimeOnly end, bool substitute)
    {
        string[] days = ["الأحد", "الاثنين", "الثلاثاء", "الأربعاء", "الخميس", "الجمعة", "السبت"];
        return new("lesson", id, substitute ? "ترشيح كبديل لحصة" : "إسناد حصة في الجدول", substitute ? "Nominated as lesson substitute" : "Timetable lesson assigned",
            $"{(substitute ? "تم ترشيحك كبديل احتياطي" : "تم إسناد الحصة إليك كمدرس أساسي")} لمادة {subjectAr}، الصف {gradeAr}، فصل {classAr} — {yearAr}. كل {days[(int)day]}، الخانة {number}، من {start:HH:mm} إلى {end:HH:mm} بتوقيت المدرسة.{(substitute ? " الترشيح ليس تكليف تغطية فعليًا؛ ستصلك رسالة مستقلة عند التكليف." : "")}",
            $"{(substitute ? "You were nominated as a standby substitute" : "You were assigned as primary teacher")} for {subjectEn}, {gradeEn}, class {classEn} — {yearEn}. Every {day}, slot {number}, {start:HH:mm}–{end:HH:mm} school time.{(substitute ? " This is not actual coverage; a separate notification will confirm coverage." : "")}");
    }

    public static async Task<IReadOnlyList<TeacherAssignmentDetail>> List(SchoolsDbContext db, Guid userId, CancellationToken ct)
    {
        var result = new List<TeacherAssignmentDetail>();
        var departments = await db.DepartmentMemberships.AsNoTracking().Where(x => x.UserId == userId && x.IsActive && x.Department.IsActive)
            .Select(x => new { x.Id, x.Department.NameAr, x.Department.NameEn, x.IsPrimary, x.StartsOn, x.EndsOn }).ToListAsync(ct);
        result.AddRange(departments.Select(x => Department(x.Id, x.NameAr, x.NameEn, x.IsPrimary, x.StartsOn, x.EndsOn)));
        var scopes = await db.TeacherGradeSubjectScopes.AsNoTracking().Where(x => x.TeacherUserId == userId && x.IsActive)
            .Select(x => new { x.Id, x.GradeSubjectOfferingId }).ToListAsync(ct);
        var classes = await db.ClassSectionTeacherScopes.AsNoTracking().Where(x => x.IsActive && x.ClassSection.IsActive && x.TeacherGradeSubjectScope.IsActive && x.TeacherGradeSubjectScope.TeacherUserId == userId)
            .Select(x => new { x.Id, x.ClassSection.NameAr, x.ClassSection.NameEn, x.TeacherGradeSubjectScope.GradeSubjectOfferingId }).ToListAsync(ct);
        var offeringIds = scopes.Select(x => x.GradeSubjectOfferingId).Concat(classes.Select(x => x.GradeSubjectOfferingId)).Distinct().ToArray();
        var contexts = await ScopeContexts(db.GradeSubjectOfferings.AsNoTracking().Where(x => offeringIds.Contains(x.Id))).ToDictionaryAsync(x => x.Id, ct);
        foreach (var x in scopes) if (contexts.TryGetValue(x.GradeSubjectOfferingId, out var context)) result.Add(ScopeDetail(context, x.Id, null, null));
        foreach (var x in classes) if (contexts.TryGetValue(x.GradeSubjectOfferingId, out var context)) result.Add(ScopeDetail(context, x.Id, x.NameAr, x.NameEn));
        var lessons = await db.WeeklyTimetableSlots.AsNoTracking().Where(x => x.IsActive && !x.IsBreak && x.ClassSectionSubject != null &&
            (x.PrimaryTeacherScope!.TeacherUserId == userId || x.SubstituteTeachers.Any(s => s.IsActive && s.TeacherGradeSubjectScope.TeacherUserId == userId)))
            .OrderBy(x => x.DayOfWeek).ThenBy(x => x.StartsAt).Select(x => new
            {
                x.Id, x.DayOfWeek, x.SlotNumber, x.StartsAt, x.EndsAt,
                ClassAr = x.ClassSection.NameAr, ClassEn = x.ClassSection.NameEn,
                SubjectAr = x.ClassSectionSubject!.GradeSubjectOffering.CurriculumGradeSubject.Subject.NameAr,
                SubjectEn = x.ClassSectionSubject.GradeSubjectOffering.CurriculumGradeSubject.Subject.NameEn,
                GradeAr = x.ClassSection.GradeOffering.GradeLevel.NameAr, GradeEn = x.ClassSection.GradeOffering.GradeLevel.NameEn,
                YearAr = x.ClassSection.GradeOffering.ProgramAcademicYear.NameAr, YearEn = x.ClassSection.GradeOffering.ProgramAcademicYear.NameEn,
                Substitute = x.PrimaryTeacherScope!.TeacherUserId != userId
            }).ToListAsync(ct);
        result.AddRange(lessons.Select(x => Lesson(x.Id, x.ClassAr, x.ClassEn, x.SubjectAr, x.SubjectEn, x.GradeAr, x.GradeEn, x.YearAr, x.YearEn, x.DayOfWeek, x.SlotNumber, x.StartsAt, x.EndsAt, x.Substitute)));
        return result;
    }
}
