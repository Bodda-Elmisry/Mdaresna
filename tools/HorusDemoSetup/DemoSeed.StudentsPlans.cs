using System.Text.Json;
using Mdaresna.Schools.Domain.Academics;
using Mdaresna.Schools.Domain.Identity;
using Mdaresna.Schools.Domain.Students;
using Mdaresna.Schools.Infrastructure.Identity;
using Mdaresna.Schools.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

internal static partial class DemoSeed
{
    private static async Task BuildStudents(SchoolsDbContext db, List<ClassData> classes, IGlobalStudentGateway global, Guid schoolId, Guid owner)
    {
        var count = 0;
        foreach (var c in classes)
        for (var index = 0; index < 5; index++)
        {
            var key = c.Key + "/student/" + index; var female = count % 2 != 0; var name = Name(female);
            var age = c.British ? c.Stage == 0 ? c.Grade + 2 : c.Stage == 1 ? c.Grade + 4 : c.Grade + 10
                : c.Stage == 0 ? c.Grade + 3 : c.Stage == 1 ? c.Grade + 5 : c.Grade + 11;
            var born = new DateOnly(2026 - age, 5 + index % 3, 10 + index);
            var certificate = "HORUS-DEMO-" + c.Key.Replace('/', '-') + "-" + index;
            var identity = await global.ResolveOrCreateAsync(schoolId, null, name.Ar, born, null, certificate)
                ?? throw new InvalidOperationException("Could not allocate demo student identity at index " + count);
            var person = Add(db, key + "/person", new Person { DisplayName = name.Ar, FirstName = name.Ar.Split(' ')[0], LastName = name.Ar.Split(' ')[^1], DateOfBirth = born, GenderCode = female ? "female" : "male" });
            var student = Add(db, key, new Student { Person = person, GlobalStudentId = identity.GlobalStudentId, StudentCode = identity.StudentCode,
                FullNameAr = name.Ar, FullNameEn = name.En, NormalizedName = (name.Ar + " " + name.En).ToUpperInvariant(), DateOfBirth = born, Gender = female ? StudentGender.Female : StudentGender.Male, BirthCertificateNumber = certificate });
            Add(db, key + "/enrollment", new StudentEnrollment { Student = student, GradeOffering = c.Offering, ClassSection = c.Section, EnrollmentDate = c.Year.StartDate, Status = StudentEnrollmentStatus.Active });
            for (var parent = 0; parent < 2; parent++)
            {
                var parentName = Name(parent == 1);
                var guardian = Add(db, key + "/guardian/" + parent, new Guardian { FullName = parent == 0 ? name.Ar.Split(' ')[1] + " " + parentName.Ar.Split(' ')[1] + " " + name.Ar.Split(' ')[^1] : parentName.Ar,
                    Phone = Phone(Staff.Count + count * 2 + parent), Email = $"horus.demo.guardian.{count + 1}.{parent + 1}@example.invalid" });
                Add(db, key + "/guardian-link/" + parent, new StudentGuardian { Student = student, Guardian = guardian, Relationship = parent == 0 ? GuardianRelationship.Father : GuardianRelationship.Mother,
                    IsPrimary = parent == 0, CanPickup = true, IsEmergencyContact = true, IsFinancialResponsible = parent == 0 });
            }
            if (++count % 20 == 0) Console.WriteLine("Demo students prepared=" + count);
        }
        await db.SaveChangesAsync();
        var systemId = await db.Subscriptions.Where(x => x.IsEnforced && x.CapacityMetric == "Student").Select(x => x.SystemId).SingleAsync();
        var state = await SchoolCapacityService.LockAsync(db, systemId, default);
        await SchoolCapacityService.ReconcileStudentYearsAsync(db, state, owner, default);
        await db.SaveChangesAsync();
    }
    private static void BuildPlans(SchoolsDbContext db, List<ClassData> classes, Guid owner)
    {
        var today = new DateOnly(2026, 10, 19);
        foreach (var c in classes)
        foreach (var (subject, offering) in c.Courses)
        {
            TeachingPlan Plan(string tag, TeachingPlanType type, DateOnly from, DateOnly to, TeachingPlan? parent = null, AcademicTerm? term = null)
            {
                var name = offering.CurriculumGradeSubject.Subject;
                var plan = Add(db, c.Key + "/plan/" + subject + "/" + tag, new TeachingPlan { Type = type, Status = TeachingPlanStatus.Draft,
                    ProgramAcademicYear = c.Year, GradeOffering = c.Offering, GradeSubjectOffering = offering, AcademicTerm = term, ParentPlan = parent,
                    TitleAr = "مسودة ديمو - " + name.NameAr + " - " + c.Level.NameAr + " - " + tag,
                    TitleEn = "Demo draft - " + name.NameEn + " - " + c.Level.NameEn + " - " + tag,
                    Details = "خطة تجريبية جاهزة للمراجعة: أهداف التعلم، الشرح والأمثلة، النشاط العملي، التقييم الختامي، ومتابعة الفروق الفردية. لم تعتمد أو تنشر بعد.",
                    SourceAuthority = TeachingPlanSourceAuthority.School, SourceReference = "HorusDemoSetup-v1", FromDate = from, ToDate = to, CreatedByUserId = owner });
                Add(db, c.Key + "/plan-target/" + subject + "/" + tag, new TeachingPlanTarget { TeachingPlan = plan, ClassSection = c.Section });
                for (var unit = 0; unit < 3; unit++) Add(db, c.Key + "/plan-item/" + subject + "/" + tag + "/" + unit, new TeachingPlanItem { TeachingPlan = plan, SortOrder = unit + 1,
                    Title = new[] { "بناء المفاهيم الأساسية", "التطبيق والتدريب", "المراجعة والتقييم" }[unit], Details = "محتوى ديمو - يراجعه مدرس المادة قبل النشر.", FromDate = from, ToDate = to });
                Add(db, c.Key + "/plan-audit/" + subject + "/" + tag, new TeachingPlanAudit { TeachingPlan = plan, ActorUserId = owner, Action = "Created", Details = "Demo setup draft; no human approval or publication recorded." });
                return plan;
            }
            var yearly = Plan("YEAR", TeachingPlanType.Yearly, c.Year.StartDate, c.Year.EndDate);
            foreach (var term in c.Terms)
            {
                var termPlan = Plan(term.Code, TeachingPlanType.Term, term.StartDate, term.EndDate, yearly, term);
                if (today < term.StartDate || today > term.EndDate) continue;
                var monthly = Plan("OCT", TeachingPlanType.Monthly, new(2026, 10, 1), new(2026, 10, 31), termPlan, term);
                var weekly = Plan("WEEK-2026-10-17", TeachingPlanType.Weekly, new(2026, 10, 17), new(2026, 10, 23), monthly, term);
                for (var day = 0; day < 5; day++) Add(db, c.Key + "/plan-day/" + subject + "/" + day, new TeachingPlanDayDetail { TeachingPlan = weekly, Date = new DateOnly(2026, 10, 18).AddDays(day), DayOfWeek = (DayOfWeek)day, Details = "تهيئة ومراجعة، شرح وتطبيق، نشاط جماعي، وتقييم ختامي تجريبي." });
                if (c.Schedule.Any(slot => slot.Key.Day == 1 && slot.Value == subject)) Plan("DAY-2026-10-19", TeachingPlanType.Daily, today, today, weekly, term);
            }
        }
    }
    public static async Task VerifyAsync(SchoolsDbContext db, bool exportReports = true)
    {
        var programs = await db.EducationPrograms.CountAsync(x => x.Code.StartsWith(Prefix));
        var classes = await db.ClassSections.Include(x => x.GradeOffering).ThenInclude(x => x.GradeLevel).ThenInclude(x => x.EducationStage).Where(x => x.Code.StartsWith(Prefix)).ToArrayAsync();
        if (programs != 2 || classes.Length != 22 || classes.Any(c => c.Capacity != 10)) throw new InvalidOperationException("Demo programs/classes do not match the requested setup.");
        var enrollments = await db.StudentEnrollments.Where(x => x.Status == StudentEnrollmentStatus.Active).GroupBy(x => x.ClassSectionId).Select(x => new { Id = x.Key, Count = x.Count() }).ToArrayAsync();
        if (enrollments.Length != 22 || enrollments.Any(x => x.Count != 5) || await db.Students.CountAsync() != 110) throw new InvalidOperationException("Expected exactly 5 students per class (110 total).");
        var timetable = await db.WeeklyTimetableSlots.Include(x => x.PrimaryTeacherScope).Where(x => x.IsActive).ToArrayAsync();
        if (timetable.Count(x => !x.IsBreak) != 670 || timetable.Count(x => x.IsBreak) != 110)
            throw new InvalidOperationException("Unexpected weekly timetable totals.");
        if (timetable.Any(x => (int)x.DayOfWeek > 4 || x.StartsAt >= x.EndsAt)) throw new InvalidOperationException("Unexpected timetable days/times.");
        var rooms = await db.ClassRoomAssignments.Where(x => x.IsPrimary && x.IsActive).ToDictionaryAsync(x => x.ClassSectionId, x => x.RoomId);
        if (rooms.Count != 22 || rooms.Values.Distinct().Count() != 22 || await db.SchoolBuildings.CountAsync() != 2 || await db.BuildingFloors.CountAsync() != 6)
            throw new InvalidOperationException("Expected two buildings, six floors and a dedicated room for each class.");
        var days = await db.SchoolDaySchedules.Where(x => x.IsActive).ToArrayAsync();
        if (days.Length != 10 || days.GroupBy(x => x.EducationProgramId).Any(g => !g.Select(x => (int)x.DayOfWeek).Order().SequenceEqual(Enumerable.Range(0, 5))))
            throw new InvalidOperationException("Programme operating week must be Sunday through Thursday.");
        foreach (var group in timetable.Where(x => !x.IsBreak).GroupBy(x => (Room: x.RoomId ?? rooms[x.ClassSectionId], x.DayOfWeek)))
        {
            var ordered = group.OrderBy(x => x.StartsAt).ToArray();
            if (ordered.Zip(ordered.Skip(1)).Any(pair => pair.First.EndsAt > pair.Second.StartsAt)) throw new InvalidOperationException("Room timetable overlap.");
        }
        foreach (var group in timetable.Where(x => !x.IsBreak).GroupBy(x => (x.PrimaryTeacherScope!.TeacherUserId, x.DayOfWeek)))
        {
            var ordered = group.OrderBy(x => x.StartsAt).ToArray();
            if (ordered.Zip(ordered.Skip(1)).Any(pair => pair.First.EndsAt > pair.Second.StartsAt)) throw new InvalidOperationException("Teacher timetable overlap.");
        }
        foreach (var c in classes)
        for (var day = 0; day < 5; day++)
        {
            var slots = timetable.Where(x => x.ClassSectionId == c.Id && (int)x.DayOfWeek == day).OrderBy(x => x.StartsAt).ToArray();
            if (slots.Count(x => !x.IsBreak) != c.GradeOffering.GradeLevel.EducationStage.DailyLessonCount || slots.Count(x => x.IsBreak) != 1 || slots.Zip(slots.Skip(1)).Any(pair => pair.First.EndsAt > pair.Second.StartsAt)) throw new InvalidOperationException("Incomplete or overlapping class timetable.");
        }
        var counts = timetable.Where(x => !x.IsBreak).GroupBy(x => x.ClassSectionSubjectId!.Value).ToDictionary(x => x.Key, x => x.Count());
        var courses = await db.ClassSectionSubjects.Include(x => x.GradeSubjectOffering).ThenInclude(x => x.CurriculumGradeSubject).ToArrayAsync();
        if (courses.Any(x => counts.GetValueOrDefault(x.Id) != x.GradeSubjectOffering.CurriculumGradeSubject.WeeklyPeriods)) throw new InvalidOperationException("Timetable and curriculum weekly quotas disagree.");
        var license = await db.Subscriptions.SingleAsync(x => x.CapacityMetric == "Student");
        if (license.UsedCapacity != 110 || await db.CapacityAllocations.CountAsync(x => x.SystemId == license.SystemId && x.IsActive) != 110) throw new InvalidOperationException("Student seats are not reconciled.");
        Console.WriteLine($"VERIFIED: {programs} programmes; {classes.Length} classes; 110 students; {await db.LocalUsers.CountAsync(x => x.UserName.StartsWith("demo-"))} demo staff; {timetable.Count(x => !x.IsBreak)} weekly lessons; {timetable.Count(x => x.IsBreak)} breaks; seats {license.UsedCapacity}/{license.BaseCapacity + license.AddOnCapacity}.");
        if (!exportReports) return;
        var folder = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "..", "Deliverables", "Horus-Demo")); Directory.CreateDirectory(folder);
        static string Cell(string? value) => "\"" + (value ?? "").Replace("\"", "\"\"") + "\"";
        var users = await db.LocalUsers.AsNoTracking().Include(x => x.Person).ThenInclude(x => x.Contacts).Include(x => x.DepartmentMemberships).ThenInclude(x => x.Department).Where(x => x.UserName.StartsWith("demo-")).OrderBy(x => x.UserName).ToArrayAsync();
        var staffRows = users.Select(x => string.Join(",", new[] { x.UserName + "@SCH-1A769BC8A279", x.Person.DisplayName, x.Kind.ToString(), x.Person.Contacts.FirstOrDefault(p => p.Type == PersonContactType.Phone)?.Value, x.DepartmentMemberships.FirstOrDefault()?.Department.NameAr }.Select(Cell)));
        await File.WriteAllLinesAsync(Path.Combine(folder, "Horus-Demo-Staff.csv"), new[] { "Login,Name,Kind,Phone,Department" }.Concat(staffRows), new System.Text.UTF8Encoding(true));
        var studentRows = await db.StudentEnrollments.AsNoTracking().Include(x => x.Student).Include(x => x.ClassSection).ThenInclude(x => x.GradeOffering).ThenInclude(x => x.ProgramAcademicYear).ThenInclude(x => x.EducationProgram).OrderBy(x => x.ClassSection.Code).ThenBy(x => x.Student.FullNameAr).ToArrayAsync();
        await File.WriteAllLinesAsync(Path.Combine(folder, "Horus-Demo-Students.csv"), new[] { "StudentCode,Name,Programme,Class,DateOfBirth" }.Concat(studentRows.Select(x => string.Join(",", new[] { x.Student.StudentCode, x.Student.FullNameAr, x.ClassSection.GradeOffering.ProgramAcademicYear.EducationProgram.NameAr, x.ClassSection.NameAr, x.Student.DateOfBirth.ToString("yyyy-MM-dd") }.Select(Cell)))), new System.Text.UTF8Encoding(true));
        var courseNames = courses.ToDictionary(x => x.Id, x => x.GradeSubjectOffering.CurriculumGradeSubject.SubjectId);
        var subjects = await db.Subjects.ToDictionaryAsync(x => x.Id, x => x.NameAr);
        var people = await db.LocalUsers.Include(x => x.Person).ToDictionaryAsync(x => x.Id, x => x.Person.DisplayName);
        var sectionNames = classes.ToDictionary(x => x.Id, x => x.NameAr);
        var programmeNames = studentRows.GroupBy(x => x.ClassSectionId).ToDictionary(x => x.Key, x => x.First().ClassSection.GradeOffering.ProgramAcademicYear.EducationProgram.NameAr);
        var dayNames = new[] { "الأحد", "الإثنين", "الثلاثاء", "الأربعاء", "الخميس" };
        await File.WriteAllLinesAsync(Path.Combine(folder, "Horus-Demo-Timetable.csv"), new[] { "Programme,Class,Day,Slot,StartsAt,EndsAt,Subject,Teacher" }.Concat(timetable.OrderBy(x => x.ClassSectionId).ThenBy(x => x.DayOfWeek).ThenBy(x => x.StartsAt).Select(x => string.Join(",", new[] { programmeNames[x.ClassSectionId], sectionNames[x.ClassSectionId], dayNames[(int)x.DayOfWeek], x.SlotNumber.ToString(), x.StartsAt.ToString("HH:mm"), x.EndsAt.ToString("HH:mm"), x.IsBreak ? "فسحة" : subjects[courseNames[x.ClassSectionSubjectId!.Value]], x.IsBreak ? "" : people[x.PrimaryTeacherScope!.TeacherUserId] }.Select(Cell)))), new System.Text.UTF8Encoding(true));
        Console.WriteLine("Demo CSV reports: " + folder);
    }
}
