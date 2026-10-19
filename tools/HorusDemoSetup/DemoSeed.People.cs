using System.Security.Cryptography;
using Mdaresna.Schools.Domain.Academics;
using Mdaresna.Schools.Domain.Identity;
using Mdaresna.Schools.Domain.Organization;
using Mdaresna.Schools.Domain.Students;
using Mdaresna.Schools.Infrastructure.Identity;
using Mdaresna.Schools.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

internal static partial class DemoSeed
{
    private sealed record StaffRow(LocalUserAccount User, string Name, string Phone, string Title, string Department);
    private static readonly List<StaffRow> Staff = [];
    private static readonly Random NamesRandom = new(20261019);
    private static readonly HashSet<string> UsedNames = [];
    private static (string Ar, string En) Name(bool female)
    {
        var boys = new[] { ("آدم", "Adam"), ("عمر", "Omar"), ("يوسف", "Youssef"), ("سليم", "Selim"), ("ياسين", "Yassin"), ("مالك", "Malek"), ("مروان", "Marwan"), ("أحمد", "Ahmed"), ("محمد", "Mohamed"), ("كريم", "Karim"), ("زياد", "Ziad"), ("مصطفى", "Mostafa"), ("سيف", "Seif"), ("علي", "Ali"), ("حسن", "Hassan") };
        var girls = new[] { ("مريم", "Mariam"), ("ملك", "Malak"), ("سلمى", "Salma"), ("ليلى", "Laila"), ("هنا", "Hana"), ("نور", "Nour"), ("جنى", "Jana"), ("فريدة", "Farida"), ("يارا", "Yara"), ("سارة", "Sara"), ("ندى", "Nada"), ("دينا", "Dina"), ("آية", "Aya"), ("ريم", "Reem"), ("حبيبة", "Habiba") };
        var last = new[] { ("المنشاوي", "Elmenshawy"), ("حسين", "Hussein"), ("إبراهيم", "Ibrahim"), ("الرفاعي", "Elrefaey"), ("عادل", "Adel"), ("شريف", "Sherif"), ("جمال", "Gamal"), ("فؤاد", "Fouad"), ("سامي", "Samy"), ("رشاد", "Rashad"), ("حمدي", "Hamdy"), ("أنور", "Anwar"), ("كمال", "Kamal"), ("راشد", "Rashed"), ("إسماعيل", "Ismail") };
        while (true)
        {
            var first = (female ? girls : boys)[NamesRandom.Next(boys.Length)]; var father = boys[NamesRandom.Next(boys.Length)]; var family = last[NamesRandom.Next(last.Length)];
            var ar = first.Item1 + " " + father.Item1 + " " + family.Item1;
            if (UsedNames.Add(ar)) return (ar, first.Item2 + " " + father.Item2 + " " + family.Item2);
        }
    }
    private static string Phone(int index) => "0" + (1234567890L + index).ToString("D10");
    private static async Task<LocalRole> Role(SchoolsDbContext db, string key, string ar, string en, Func<string, bool> select, Guid owner)
    {
        var role = Add(db, "role/" + key, new LocalRole { Code = "horus-demo-" + key, DisplayNameAr = ar, DisplayNameEn = en, IsSystem = false });
        foreach (var permission in await db.LocalPermissions.Where(x => x.IsActive).ToArrayAsync())
            if (select(permission.Code)) db.Add(new LocalRolePermission { Role = role, Permission = permission, GrantedAtUtc = Now, GrantedByUserId = owner });
        return role;
    }
    private static async Task<LocalUserAccount> NewStaff(SchoolsDbContext db, ISchoolUserIdentityGateway identities, Guid schoolId,
        string key, string title, SchoolUserKind kind, LocalRole role, SchoolDepartment department, Guid owner, string password)
    {
        var index = Staff.Count; var name = Name(index % 2 != 0); var phone = Phone(index);
        var identity = await identities.ResolveAsync(schoolId, phone, name.Ar) ?? throw new InvalidOperationException("Could not resolve demo staff identity at index " + index);
        if (identity.DisplayName != name.Ar || identity.PhoneVerified) throw new InvalidOperationException("Demo phone overlaps an existing non-demo identity; no school setup was committed.");
        var person = Add(db, key + "/person", new Person { DisplayName = name.Ar, FirstName = name.Ar.Split(' ')[0], LastName = name.Ar.Split(' ')[^1], Status = PersonStatus.Active, GenderCode = index % 2 == 0 ? "male" : "female", DateOfBirth = new DateOnly(1982 + index % 18, 3, 15) });
        db.Add(new PersonContact { Id = Id(key + "/phone"), Person = person, Type = PersonContactType.Phone, Value = phone, NormalizedValue = identity.PrimaryPhone, IsPrimary = true, IsVerified = false, CreatedAtUtc = Now, UpdatedAtUtc = Now });
        var username = "demo-" + key.Replace('/', '-').ToLowerInvariant();
        var user = Add(db, key + "/user", new LocalUserAccount { Person = person, PlatformAccountId = identity.AccountId, UserName = username, NormalizedUserName = username.ToUpperInvariant(), Kind = kind, Status = LocalUserStatus.Active });
        db.Add(new LocalUserCredential { User = user, UserId = user.Id, PasswordHash = new PasswordHasher<LocalUserAccount>().HashPassword(user, password), SecurityStamp = Convert.ToHexString(RandomNumberGenerator.GetBytes(32)), ChangedAtUtc = Now });
        db.Add(new LocalUserRole { User = user, Role = role, AssignedAtUtc = Now, AssignedByUserId = owner });
        Add(db, key + "/membership", new DepartmentMembership { User = user, Department = department, IsPrimary = true, TitleAr = title, TitleEn = title, StartsOn = new DateOnly(2026, 9, 6), EndsOn = End });
        Staff.Add(new(user, name.Ar, phone, title, department.NameAr));
        if (Staff.Count % 10 == 0) Console.WriteLine("Demo staff prepared=" + Staff.Count);
        return user;
    }
    private static async Task BuildAsync(SchoolsDbContext db, IGlobalStudentGateway global, ISchoolUserIdentityGateway identities, Guid owner, string password)
    {
        var schoolId = await db.SchoolInformation.Select(x => x.PlatformSchoolReferenceId).SingleAsync();
        var classes = BuildAcademics(db);
        var academic = Add(db, "dept/academic", new SchoolDepartment { Code = Prefix + "-ACADEMIC", NameAr = "الإدارة الأكاديمية", NameEn = "Academic administration", Type = SchoolDepartmentType.Academic });
        var administrative = Add(db, "dept/administrative", new SchoolDepartment { Code = Prefix + "-ADMIN", NameAr = "إدارة وتشغيل المدرسة", NameEn = "School operations", Type = SchoolDepartmentType.Administrative });
        var teacherPrefixes = new[] { "school.dashboard.view", "school.attendance.", "school.homework.", "school.activities.", "school.lesson_notes.", "school.lesson_evaluations.", "school.academics.view", "school.students.view", "school.exams.view", "school.exams.results.enter", "school.social.view", "school.social.interact" };
        var teacherRole = await Role(db, "teacher", "مدرس مادة - ديمو", "Demo subject teacher", c => teacherPrefixes.Any(p => p.EndsWith('.') ? c.StartsWith(p) && !c.EndsWith("reopen") && !c.EndsWith("manage") : c == p) || c.StartsWith("school.homework.") || c.StartsWith("school.activities."), owner);
        var coRole = await Role(db, "coteacher", "معلم مساعد فصل - ديمو", "Demo class co-teacher", c => new[] { "school.dashboard.view", "school.students.view", "school.attendance.view", "school.attendance.record", "school.teaching.view", "school.curriculum.view", "school.social.view" }.Contains(c), owner);
        var supervisorRole = await Role(db, "supervisor", "مشرف مرحلة - ديمو", "Demo stage supervisor", c => c.StartsWith("school.attendance.") || new[] { "school.dashboard.view", "school.students.view", "school.teaching.view", "school.curriculum.view", "school.exams.view", "school.people.view", "school.departments.view", "school.social.view" }.Contains(c), owner);
        var employeeRole = await Role(db, "employee", "موظف تشغيل - ديمو", "Demo operations employee", c => new[] { "school.dashboard.view", "school.people.view", "school.departments.view", "school.facilities.view", "school.social.view" }.Contains(c), owner);
        var subjectDepartments = Subjects.ToDictionary(x => x.Code, x => Add(db, "dept/" + x.Code, new SchoolDepartment { ParentDepartment = academic, Code = Prefix + "-DEPT-" + x.Code, NameAr = "قسم " + x.Ar, NameEn = x.En + " department", Type = SchoolDepartmentType.Academic }));
        var headedDepartments = new HashSet<Guid>();
        foreach (var programGroup in classes.GroupBy(x => x.Program))
        foreach (var spec in Subjects)
        {
            var eligible = programGroup.Where(x => x.Quotas.ContainsKey(spec.Code)).ToArray();
            if (eligible.Length == 0) continue;
            bool Overlap(ClassData a, ClassData b) => a.Schedule.Any(x => x.Value == spec.Code && b.Schedule.GetValueOrDefault(x.Key) == spec.Code);
            var colors = new List<List<ClassData>>();
            foreach (var c in eligible.OrderByDescending(c => eligible.Count(other => other != c && Overlap(c, other))).ThenByDescending(c => c.Quotas[spec.Code]).ThenBy(c => c.Key))
            {
                var bucket = colors.Where(group => group.Sum(x => x.Quotas[spec.Code]) + c.Quotas[spec.Code] <= 28 && group.All(other => !Overlap(c, other))).OrderBy(group => group.Sum(x => x.Quotas[spec.Code])).FirstOrDefault();
                if (bucket == null) { bucket = []; colors.Add(bucket); } bucket.Add(c);
            }
            var department = subjectDepartments[spec.Code];
            var departmentSubject = Add(db, programGroup.Key.Code + "/dept-subject/" + spec.Code, new AcademicDepartmentSubject { Department = department, Subject = eligible[0].Courses[spec.Code].CurriculumGradeSubject.Subject });
            for (var i = 0; i < colors.Count; i++)
            {
                var key = (eligible[0].British ? "brit" : "nat") + "/" + spec.Code.ToLowerInvariant() + "/teacher" + (i + 1);
                var user = await NewStaff(db, identities, schoolId, key, "مدرس " + spec.Ar, SchoolUserKind.Teacher, teacherRole, department, owner, password);
                if (headedDepartments.Add(department.Id)) Add(db, key + "/head", new DepartmentLeadership { Department = department, User = user, Role = DepartmentLeadershipRole.Head, StartsOn = new(2026, 9, 6), EndsOn = End });
                if (i == 0) Add(db, key + "/coordinator", new SubjectCoordinatorAssignment { DepartmentSubject = departmentSubject, CoordinatorUser = user, EducationProgram = programGroup.Key, StartsOn = new(2026, 9, 6), EndsOn = End });
                foreach (var c in colors[i])
                {
                    var teacherScope = Add(db, c.Key + "/teacher-scope/" + spec.Code, new TeacherGradeSubjectScope { TeacherUser = user, GradeSubjectOffering = c.Courses[spec.Code] }); c.TeacherScopes[spec.Code] = teacherScope;
                    Add(db, c.Key + "/class-teacher-scope/" + spec.Code, new ClassSectionTeacherScope { ClassSection = c.Section, TeacherGradeSubjectScope = teacherScope });
                    Add(db, c.Key + "/subject-teacher/" + spec.Code, new ClassSubjectTeacherAssignment { ClassSectionSubject = c.ClassSubjects[spec.Code], TeacherGradeSubjectScope = teacherScope, Role = ClassSubjectTeacherRole.Primary });
                }
            }
        }
        foreach (var group in classes.GroupBy(x => (x.Program, x.EducationStage)))
        {
            var first = group.First(); var key = first.Key.Split('/')[0] + "/stage" + first.Stage;
            var department = Add(db, key + "/dept", new SchoolDepartment { ParentDepartment = academic, Code = Prefix + "-" + key.Replace('/', '-'), NameAr = "إشراف " + first.Program.NameAr + " - " + first.EducationStage.NameAr, NameEn = first.Program.NameEn + " - " + first.EducationStage.NameEn + " supervision", Type = SchoolDepartmentType.Academic });
            var supervisor = await NewStaff(db, identities, schoolId, key + "/supervisor", "مشرف " + first.EducationStage.NameAr, SchoolUserKind.Employee, supervisorRole, department, owner, password);
            Add(db, key + "/leader", new DepartmentLeadership { Department = department, User = supervisor, Role = DepartmentLeadershipRole.Head, StartsOn = first.Year.StartDate, EndsOn = End });
            AssignOperations(db, key + "/operations", supervisor, group.ToArray(), owner, true);
            foreach (var c in group)
            {
                var co = await NewStaff(db, identities, schoolId, c.Key + "/co", "كوتيتشر " + c.Section.NameAr, SchoolUserKind.Teacher, coRole, department, owner, password);
                AssignOperations(db, c.Key + "/co-operations", co, [c], owner, false);
                foreach (var spec in c.Quotas.Keys)
                {
                    var teacherScope = Add(db, c.Key + "/co-scope/" + spec, new TeacherGradeSubjectScope { TeacherUser = co, GradeSubjectOffering = c.Courses[spec] });
                    Add(db, c.Key + "/co-class-scope/" + spec, new ClassSectionTeacherScope { ClassSection = c.Section, TeacherGradeSubjectScope = teacherScope });
                    Add(db, c.Key + "/co-subject/" + spec, new ClassSubjectTeacherAssignment { ClassSectionSubject = c.ClassSubjects[spec], TeacherGradeSubjectScope = teacherScope, Role = ClassSubjectTeacherRole.Assistant });
                }
            }
        }
        var jobs = new[] { ("principal", "مدير المدرسة", "School principal"), ("admissions", "مسؤول القبول والتسجيل", "Admissions officer"), ("affairs", "مسؤول شؤون الطلاب", "Student affairs officer"), ("reception", "موظف الاستقبال", "Receptionist"), ("it", "مسؤول تكنولوجيا المعلومات", "IT officer"), ("library", "أمين المكتبة", "Librarian"), ("accountant", "محاسب المدرسة", "School accountant"), ("clinic", "مسؤول العيادة", "School clinic officer") };
        foreach (var (key, ar, en) in jobs)
        {
            var dept = Add(db, "admin/" + key + "/dept", new SchoolDepartment { ParentDepartment = administrative, Code = Prefix + "-ADM-" + key.ToUpperInvariant(), NameAr = ar, NameEn = en, Type = SchoolDepartmentType.Administrative });
            await NewStaff(db, identities, schoolId, "admin/" + key, ar, SchoolUserKind.Employee, employeeRole, dept, owner, password);
        }
        Add(db, "owner/membership", new DepartmentMembership { UserId = owner, Department = academic, IsPrimary = !await db.DepartmentMemberships.AnyAsync(x => x.UserId == owner && x.IsPrimary && x.IsActive), TitleAr = "صاحب المدرسة", TitleEn = "School owner", StartsOn = new(2026, 9, 6), EndsOn = End });
        Add(db, "owner/leadership", new DepartmentLeadership { UserId = owner, Department = academic, Role = DepartmentLeadershipRole.Head, StartsOn = new(2026, 9, 6), EndsOn = End });
        BuildTimetables(db, classes); BuildPlans(db, classes, owner);
        await db.SaveChangesAsync();
        await BuildStudents(db, classes, global, schoolId, owner);
        Console.WriteLine($"Prepared {classes.Count} classes and {Staff.Count} demo staff.");
    }
    private static void AssignOperations(SchoolsDbContext db, string key, LocalUserAccount user, ClassData[] classes, Guid owner, bool supervisor)
    {
        var assignment = Add(db, key, new ClassOperationalAssignment { User = user, BuildingFloorId = classes[0].Floor.Id, Responsibility = supervisor ? "إشراف المرحلة ومتابعة الحضور" : "كوتيتشر ومتابعة الفصل", StartsOn = classes[0].Year.StartDate, EndsOn = End, CreatedByUserId = owner });
        var actions = supervisor ? new[] { "class.view", "attendance.daily", "attendance.session", "attendance.finalize", "attendance.reopen", "exams.view" } : ["class.view", "attendance.daily", "attendance.session"];
        foreach (var c in classes) foreach (var action in actions) Add(db, key + "/grant/" + c.Key + "/" + action, new ClassOperationalGrant { Assignment = assignment, ClassSection = c.Section, Action = action });
        Add(db, key + "/audit", new ClassOperationalAssignmentAudit { Assignment = assignment, ActorUserId = owner, Action = "Created", SnapshotJson = "{\"source\":\"HorusDemoSetup\"}" });
    }
    private static void BuildTimetables(SchoolsDbContext db, IEnumerable<ClassData> classes)
    {
        foreach (var c in classes)
        {
            foreach (var ((day, lesson), subject) in c.Schedule)
            {
                var time = LessonTime(lesson);
                Add(db, c.Key + $"/slot/{day}/{lesson}", new WeeklyTimetableSlot { ClassSection = c.Section, ClassSectionSubject = c.ClassSubjects[subject], PrimaryTeacherScope = c.TeacherScopes[subject], DayOfWeek = (DayOfWeek)day, SlotNumber = time.Slot, StartsAt = time.Start, EndsAt = time.End });
            }
            for (var day = 0; day < 5; day++) Add(db, c.Key + "/break/" + day, new WeeklyTimetableSlot { ClassSection = c.Section, DayOfWeek = (DayOfWeek)day, SlotNumber = 4, IsBreak = true, StartsAt = new(10, 0), EndsAt = new(10, 20) });
        }
    }
}
