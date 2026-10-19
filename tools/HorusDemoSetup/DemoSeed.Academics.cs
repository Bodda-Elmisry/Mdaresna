using Mdaresna.Schools.Domain.Academics;
using Mdaresna.Schools.Domain.Facilities;
using Mdaresna.Schools.Infrastructure.Persistence;

internal static partial class DemoSeed
{
    private sealed record SubjectSpec(string Code, string Ar, string En);
    private static readonly SubjectSpec[] Subjects = [new("AR", "اللغة العربية", "Arabic"), new("EN", "اللغة الإنجليزية", "English"),
        new("MATH", "الرياضيات", "Mathematics"), new("SCI", "العلوم", "Science"), new("SOC", "الدراسات الاجتماعية", "Humanities"),
        new("REL", "التربية الدينية والقيم", "Religious education and values"), new("ICT", "الحاسب وتكنولوجيا المعلومات", "Computing"),
        new("ART", "التربية الفنية", "Art"), new("PE", "التربية الرياضية", "Physical education"), new("WORLD", "اكتشاف العالم والمهارات الحياتية", "Understanding the world")];
    private sealed class ClassData
    {
        public required string Key { get; init; }
        public required bool British { get; init; }
        public required int Stage { get; init; }
        public required int Grade { get; init; }
        public required EducationProgram Program { get; init; }
        public required ProgramAcademicYear Year { get; init; }
        public required EducationStage EducationStage { get; init; }
        public required GradeLevel Level { get; init; }
        public required GradeOffering Offering { get; init; }
        public required ClassSection Section { get; init; }
        public required SchoolRoom Room { get; init; }
        public required BuildingFloor Floor { get; init; }
        public required Dictionary<string, int> Quotas { get; init; }
        public required Dictionary<(int Day, int Lesson), string> Schedule { get; init; }
        public Dictionary<string, GradeSubjectOffering> Courses { get; } = [];
        public Dictionary<string, ClassSectionSubject> ClassSubjects { get; } = [];
        public Dictionary<string, TeacherGradeSubjectScope> TeacherScopes { get; } = [];
        public List<AcademicTerm> Terms { get; } = [];
    }
    private static Dictionary<string, int> Quotas(bool british, int stage) => (british, stage) switch
    {
        (false, 0) => new() { ["AR"] = 5, ["EN"] = 5, ["MATH"] = 5, ["WORLD"] = 4, ["ART"] = 3, ["PE"] = 3 },
        (false, 1) => new() { ["AR"] = 6, ["EN"] = 5, ["MATH"] = 5, ["SCI"] = 4, ["SOC"] = 2, ["REL"] = 2, ["ICT"] = 2, ["ART"] = 2, ["PE"] = 2 },
        (false, _) => new() { ["AR"] = 6, ["EN"] = 6, ["MATH"] = 6, ["SCI"] = 5, ["SOC"] = 4, ["REL"] = 2, ["ICT"] = 2, ["ART"] = 2, ["PE"] = 2 },
        (true, 0) => new() { ["EN"] = 6, ["AR"] = 4, ["MATH"] = 5, ["WORLD"] = 4, ["ART"] = 3, ["PE"] = 3 },
        (true, 1) => new() { ["EN"] = 6, ["AR"] = 4, ["MATH"] = 5, ["SCI"] = 4, ["SOC"] = 3, ["REL"] = 2, ["ICT"] = 2, ["ART"] = 2, ["PE"] = 2 },
        (true, _) => new() { ["EN"] = 7, ["AR"] = 3, ["MATH"] = 6, ["SCI"] = 6, ["SOC"] = 4, ["REL"] = 2, ["ICT"] = 3, ["ART"] = 2, ["PE"] = 2 }
    };
    private static Dictionary<(int Day, int Lesson), string> Schedule(string key, Dictionary<string, int> quotas)
    {
        var random = new Random(BitConverter.ToInt32(Id(key).ToByteArray(), 0));
        var list = quotas.SelectMany(x => Enumerable.Repeat(x.Key, x.Value)).ToArray();
        var perDay = list.Length / 5;
        for (var attempt = 0; attempt < 10000; attempt++)
        {
            random.Shuffle(list);
            if (Enumerable.Range(0, 5).Any(day => list.Skip(day * perDay).Take(perDay).GroupBy(x => x).Any(g => g.Count() > 2))) continue;
            return Enumerable.Range(0, list.Length).ToDictionary(i => (i / perDay, i % perDay + 1), i => list[i]);
        }
        throw new InvalidOperationException("Could not balance the weekly subject distribution.");
    }
    private static (TimeOnly Start, TimeOnly End, int Slot) LessonTime(int lesson)
    {
        var start = new TimeOnly(8, 0).AddMinutes((lesson - 1) * 40 + (lesson > 3 ? 20 : 0));
        return (start, start.AddMinutes(40), lesson + (lesson > 3 ? 1 : 0));
    }
    private static List<ClassData> BuildAcademics(SchoolsDbContext db)
    {
        var classes = new List<ClassData>();
        var branch = Add(db, "branch", new SchoolBranch { Code = Prefix + "-MAIN", NameAr = "حرم حورس الرئيسي", NameEn = "Horus main campus", Address = "حرم تجريبي - الجيزة" });
        var roomType = Add(db, "room-type-class", new SchoolRoomType { Code = Prefix + "-CLASSROOM", NameAr = "قاعة فصل", NameEn = "Classroom", IsClassroom = true });
        var labType = Add(db, "room-type-lab", new SchoolRoomType { Code = Prefix + "-LAB", NameAr = "معمل تعليمي", NameEn = "Laboratory", IsLaboratory = true });
        var officeType = Add(db, "room-type-office", new SchoolRoomType { Code = Prefix + "-OFFICE", NameAr = "مكتب إداري", NameEn = "Office" });
        var yearDefinition = Add(db, "year-definition", new AcademicYearDefinition { Code = Prefix + "-2026-27", NameAr = "العام الدراسي 2026 / 2027", NameEn = "Academic year 2026 / 2027" });
        var bookRole = Add(db, "book-role", new BookRole { Code = Prefix + "-STUDENT", NameAr = "كتاب الطالب التجريبي", NameEn = "Demo student book" });
        foreach (var british in new[] { false, true })
        {
            var code = british ? "BRIT" : "NAT";
            var program = Add(db, code, new EducationProgram { Code = Prefix + "-" + code, NameAr = british ? "النظام البريطاني" : "النظام الناشونال", NameEn = british ? "British programme" : "National programme", ProgramType = british ? EducationProgramType.InternationalBritish : EducationProgramType.General });
            var curriculum = Add(db, code + "/curriculum", new CurriculumPlan { EducationProgram = program, Code = Prefix + "-" + code + "-2026", NameAr = "الخطة الدراسية التجريبية - " + program.NameAr, NameEn = program.NameEn + " demo curriculum", VersionLabel = "2026/2027", Status = CurriculumPlanStatus.Active, IsActive = true });
            var start = new DateOnly(2026, 9, british ? 6 : 20);
            var year = Add(db, code + "/year", new ProgramAcademicYear { EducationProgram = program, AcademicYearDefinition = yearDefinition, CurriculumPlan = curriculum, Code = Prefix + "-" + code + "-2026-27", NameAr = "2026 / 2027 - " + program.NameAr, NameEn = "2026 / 2027 - " + program.NameEn, StartDate = start, EndDate = End });
            var termDates = british ? new[] { (start, new DateOnly(2026, 12, 17)), (new DateOnly(2027, 1, 3), new DateOnly(2027, 3, 25)), (new DateOnly(2027, 4, 11), End) }
                : new[] { (start, new DateOnly(2027, 1, 21)), (new DateOnly(2027, 2, 7), End) };
            var terms = new List<AcademicTerm>();
            for (var termIndex = 0; termIndex < termDates.Length; termIndex++)
            {
                var (from, to) = termDates[termIndex];
                var term = Add(db, code + "/term/" + termIndex, new AcademicTerm { ProgramAcademicYear = year, Code = "TERM-" + (termIndex + 1), NameAr = "الفصل الدراسي " + (termIndex + 1), NameEn = "Term " + (termIndex + 1), StartDate = from, EndDate = to, SortOrder = termIndex + 1 }); terms.Add(term);
                for (var month = new DateOnly(from.Year, from.Month, 1); month <= to; month = month.AddMonths(1))
                    Add(db, code + "/month/" + termIndex + "/" + month.ToString("yyyy-MM"), new AcademicPeriod { AcademicTerm = term, Code = month.ToString("yyyy-MM"), NameAr = "فترة " + month.ToString("yyyy/MM"), NameEn = month.ToString("yyyy/MM"), StartDate = month < from ? from : month, EndDate = month.AddMonths(1).AddDays(-1) > to ? to : month.AddMonths(1).AddDays(-1), SortOrder = (month.Year - from.Year) * 12 + month.Month - from.Month + 1 });
            }
            for (var day = 0; day < 5; day++) Add(db, code + "/day/" + day, new SchoolDaySchedule { EducationProgram = program, BranchId = branch.Id, DayOfWeek = (DayOfWeek)day, StartsAt = new(8, 0), EndsAt = new(13, 15) });
            var building = Add(db, code + "/building", new SchoolBuilding { Branch = branch, Code = Prefix + "-" + code + "-BLD", NameAr = "مبنى حورس - " + (british ? "البريطاني" : "الناشونال"), NameEn = "Horus " + code + " building" });
            var subjectMap = Subjects.ToDictionary(s => s.Code, s => Add(db, code + "/subject/" + s.Code, new Subject { Code = Prefix + "-" + code + "-" + s.Code, NameAr = s.Ar, NameEn = s.En }));
            for (var stageIndex = 0; stageIndex < 3; stageIndex++)
            {
                var stageAr = new[] { "رياض الأطفال", "المرحلة الابتدائية", "المرحلة الإعدادية" }[stageIndex];
                var stageEn = new[] { "Early years", "Primary", "Preparatory / Lower secondary" }[stageIndex];
                var stage = Add(db, code + "/stage/" + stageIndex, new EducationStage { EducationProgram = program, Code = Prefix + "-" + code + "-S" + stageIndex, NameAr = stageAr, NameEn = stageEn, SortOrder = stageIndex + 1, DailyLessonCount = 5 + stageIndex, DailyBreakCount = 1 });
                var floor = Add(db, code + "/floor/" + stageIndex, new BuildingFloor { Building = building, Code = "F" + stageIndex, NameAr = "طابق " + stageAr, NameEn = stageEn + " floor", SortOrder = stageIndex });
                Add(db, code + "/office/" + stageIndex, new SchoolRoom { Floor = floor, RoomType = officeType, Code = Prefix + "-" + code + "-OFF" + stageIndex, NameAr = "مكتب إشراف " + stageAr, NameEn = stageEn + " supervision office", Capacity = 4, IsSchedulable = false });
                var gradeCount = new[] { 2, 6, 3 }[stageIndex];
                for (var grade = 1; grade <= gradeCount; grade++)
                {
                    var key = code + "/" + stageIndex + "/" + grade;
                    var gradeEn = british ? stageIndex == 0 ? "FS" + grade : "Year " + (stageIndex == 1 ? grade : grade + 6) : stageIndex == 0 ? "KG" + grade : (stageIndex == 1 ? "Primary " : "Prep ") + grade;
                    var gradeAr = british ? stageIndex == 0 ? "التمهيدي " + grade : "السنة " + (stageIndex == 1 ? grade : grade + 6) : stageIndex == 0 ? "رياض أطفال " + grade : "الصف " + grade + (stageIndex == 1 ? " الابتدائي" : " الإعدادي");
                    var level = Add(db, key + "/level", new GradeLevel { EducationStage = stage, Code = Prefix + "-" + code + "-S" + stageIndex + "G" + grade, NameAr = gradeAr, NameEn = gradeEn, SortOrder = grade });
                    var offering = Add(db, key + "/offering", new GradeOffering { ProgramAcademicYear = year, GradeLevel = level, Code = level.Code + "-26", NameAr = gradeAr, NameEn = gradeEn, Capacity = 10, Status = GradeOfferingStatus.Active, IsActive = true });
                    var section = Add(db, key + "/class", new ClassSection { GradeOffering = offering, Code = level.Code + "-A", NameAr = gradeAr + " - أ", NameEn = gradeEn + " - A", Capacity = 10 });
                    var room = Add(db, key + "/room", new SchoolRoom { Floor = floor, RoomType = roomType, Code = level.Code + "-ROOM", NameAr = "قاعة " + section.NameAr, NameEn = section.NameEn + " classroom", Capacity = 10 });
                    Add(db, key + "/room-assignment", new ClassRoomAssignment { ClassSection = section, Room = room, EffectiveFrom = start, EffectiveTo = End, IsPrimary = true });
                    var quotas = Quotas(british, stageIndex);
                    var data = new ClassData { Key = key, British = british, Stage = stageIndex, Grade = grade, Program = program, Year = year, EducationStage = stage, Level = level, Offering = offering, Section = section, Room = room, Floor = floor, Quotas = quotas, Schedule = Schedule(key, quotas) }; data.Terms.AddRange(terms);
                    foreach (var (subjectKey, count) in quotas)
                    {
                        var subject = subjectMap[subjectKey];
                        var course = Add(db, key + "/curr/" + subjectKey, new CurriculumGradeSubject { CurriculumPlan = curriculum, GradeLevel = level, Subject = subject, TermNumber = 0, WeeklyPeriods = count, CountsTowardTotal = new[] { "AR", "EN", "MATH", "SCI", "SOC", "WORLD" }.Contains(subjectKey), InstructionLanguage = subjectKey is "AR" or "REL" ? "ar" : british ? "en" : "ar", SortOrder = Array.FindIndex(Subjects, x => x.Code == subjectKey) + 1 });
                        var offered = Add(db, key + "/course/" + subjectKey, new GradeSubjectOffering { GradeOffering = offering, CurriculumGradeSubject = course, Status = GradeSubjectOfferingStatus.Active, IsActive = true }); data.Courses[subjectKey] = offered;
                        data.ClassSubjects[subjectKey] = Add(db, key + "/class-subject/" + subjectKey, new ClassSectionSubject { ClassSection = section, GradeSubjectOffering = offered });
                        var book = Add(db, key + "/book/" + subjectKey, new Book { Code = level.Code + "-" + subjectKey, NameAr = "كتاب ديمو - " + subject.NameAr + " - " + gradeAr, NameEn = "Demo " + subject.NameEn + " - " + gradeEn, Publisher = "Horus demo materials" });
                        var version = Add(db, key + "/book-version/" + subjectKey, new BookVersion { Book = book, EditionCode = "DEMO-2026", VersionLabel = "2026 / 2027 Demo", PublicationYear = 2026, Language = course.InstructionLanguage ?? "ar" });
                        Add(db, key + "/curr-book/" + subjectKey, new CurriculumSubjectBook { CurriculumGradeSubject = course, BookVersion = version, BookRole = bookRole, SortOrder = 1 });
                    }
                    classes.Add(data);
                }
            }
            var ground = classes.First(c => c.Program == program).Floor;
            foreach (var lab in new[] { ("SCI-LAB", "معمل العلوم", "Science laboratory"), ("ICT-LAB", "معمل الحاسب", "Computer laboratory") })
                Add(db, code + "/" + lab.Item1, new SchoolRoom { Floor = ground, RoomType = labType, Code = Prefix + "-" + code + "-" + lab.Item1, NameAr = lab.Item2, NameEn = lab.Item3, Capacity = 10 });
        }
        return classes;
    }
}
