using System.Text.Json;
using Mdaresna.Doamin.Enums;
using System.Text.Encodings.Web;
using System.Text.Unicode;
using Mdaresna.Doamin.DTOs.ReportingDTOs.StudentMonthlyEvaluationReportDTOs;
using Mdaresna.Doamin.Models.ReportingManagement;
using Mdaresna.Doamin.Models.SettingsManagement;
using Mdaresna.Infrastructure.Data;
using Mdaresna.Repository.IBServices.ReportingManagement;
using Mdaresna.Repository.ReportingDB.IFactories;
using Microsoft.EntityFrameworkCore;

namespace Mdaresna.Infrastructure.BServices.ReportingManagement;

internal class StudentMonthelyReportGenerator : IStudentReportGenerator
{
    private const decimal ActivityEvaluationWeight = 20m;
    private const decimal AssignmentEvaluationWeight = 20m;
    private const decimal AttendanceEvaluationWeight = 20m;
    private const decimal ExamEvaluationWeight = 40m;
    private static readonly JsonSerializerOptions ReportJsonOptions = new()
    {
        Encoder = JavaScriptEncoder.Create(UnicodeRanges.All)
    };

    private readonly AppDbContext context;
    private readonly IReportingUnitOfWorkFactory? reportingUnitOfWorkFactory;
    private readonly string? reportConnectionString;

    /// <summary>
    /// Initializes the monthly evaluation report generator with the school database context.
    /// </summary>
    /// <remarks>
    /// Business: the generator runs inside a reporting worker and reads report data from one
    /// school database context at a time.
    /// </remarks>
    public StudentMonthelyReportGenerator(AppDbContext context)
    {
        this.context = context ?? throw new ArgumentNullException(nameof(context));
    }

    /// <summary>
    /// Initializes the monthly evaluation report generator with school read access and report storage access.
    /// </summary>
    /// <remarks>
    /// Business: the school database is used to calculate report values, while the report
    /// database connection is used to persist generated <see cref="StudentReport"/> rows.
    /// </remarks>
    public StudentMonthelyReportGenerator(
        AppDbContext context,
        IReportingUnitOfWorkFactory reportingUnitOfWorkFactory,
        string reportConnectionString)
        : this(context)
    {
        if (string.IsNullOrWhiteSpace(reportConnectionString))
        {
            throw new ArgumentException("Report connection string is required.", nameof(reportConnectionString));
        }

        this.reportingUnitOfWorkFactory = reportingUnitOfWorkFactory
            ?? throw new ArgumentNullException(nameof(reportingUnitOfWorkFactory));
        this.reportConnectionString = reportConnectionString;
    }

    /// <summary>
    /// Generates the report for a queued report request without an explicit cancellation token.
    /// </summary>
    /// <remarks>
    /// Business: this overload keeps backward compatibility with callers that submit a
    /// <see cref="ReportQueue"/> only. It delegates to the cancellable worker-safe overload.
    /// </remarks>
    public Task<int> GenerateAsync(ReportQueue queue)
    {
        return GenerateAsync(queue, CancellationToken.None);
    }

    /// <summary>
    /// Generates the monthly evaluation report for one queued school report request.
    /// </summary>
    /// <remarks>
    /// Business: the queue provides the school, reporting period, and optional grade/classroom
    /// scope. The report is never generated for all schools in one call.
    /// </remarks>
    public Task<int> GenerateAsync(ReportQueue queue, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(queue);

        return GenerateMonthlyEvaluationReportAsync(
            queue.SchoolId,
            queue.FromDate,
            queue.ToDate,
            queue.GradeId,
            queue.ClassroomId,
            queue.Id,
            queue.MonthId,
            queue.WeekName,
            queue.ReportType,
            cancellationToken);
    }

    /// <summary>
    /// Generates a monthly evaluation report for all eligible students and courses in one school.
    /// </summary>
    /// <remarks>
    /// Business: this is the public report entry point required by the reporting worker. The
    /// method processes a single school for the requested date range and leaves grade/classroom
    /// filters empty.
    /// </remarks>
    public Task<int> GenerateMonthlyEvaluationReportAsync(
        Guid schoolId,
        DateTime fromDate,
        DateTime toDate,
        CancellationToken cancellationToken)
    {
        return GenerateMonthlyEvaluationReportAsync(
            schoolId,
            fromDate,
            toDate,
            gradeId: null,
            classRoomId: null,
            reportQueueId: null,
            monthId: null,
            weekName: null,
            reportType: StudentReportTypesEnum.Monthly,
            cancellationToken);
    }

    /// <summary>
    /// Orchestrates the full monthly evaluation report pipeline.
    /// </summary>
    /// <remarks>
    /// Business: dates are normalized to an inclusive start and exclusive end, then each report
    /// part is calculated separately and merged in memory. This keeps the report logic readable
    /// and avoids one large query that is difficult to maintain.
    /// </remarks>
    private async Task<int> GenerateMonthlyEvaluationReportAsync(
        Guid schoolId,
        DateTime fromDate,
        DateTime toDate,
        Guid? gradeId,
        Guid? classRoomId,
        Guid? reportQueueId,
        Guid? monthId,
        string? weekName,
        StudentReportTypesEnum reportType,
        CancellationToken cancellationToken)
    {
        var normalizedFromDate = fromDate.Date;
        var toDateExclusive = toDate.Date.AddDays(1);

        if (toDateExclusive <= normalizedFromDate)
        {
            throw new ArgumentException("To date must be on or after from date.", nameof(toDate));
        }

        var baseRows = await GetBaseReportRowsAsync(
            schoolId,
            gradeId,
            classRoomId,
            cancellationToken);

        if (baseRows.Count == 0)
        {
            return 0;
        }

        var activityEvaluations = await GetActivityEvaluationsAsync(
            schoolId,
            normalizedFromDate,
            toDateExclusive,
            baseRows,
            cancellationToken);

        var assignmentEvaluations = await GetAssignmentEvaluationsAsync(
            schoolId,
            normalizedFromDate,
            toDateExclusive,
            baseRows,
            cancellationToken);

        var examEvaluations = await GetExamEvaluationsAsync(
            schoolId,
            normalizedFromDate,
            toDateExclusive,
            baseRows,
            cancellationToken);

        var attendanceEvaluations = await GetAttendanceEvaluationsAsync(
            schoolId,
            normalizedFromDate,
            toDateExclusive,
            baseRows,
            cancellationToken);

        var reportRows = MergeMonthlyEvaluationRows(
            baseRows,
            activityEvaluations,
            assignmentEvaluations,
            examEvaluations,
            attendanceEvaluations);

        return await SaveReportRowsAsync(
            reportRows,
            reportQueueId,
            monthId,
            weekName,
            reportType,
            cancellationToken);
    }

    /// <summary>
    /// Loads the base student/classroom/course rows that can appear in the final report.
    /// </summary>
    /// <remarks>
    /// Business: a row is created per student and assigned classroom course for the selected
    /// school. Optional grade and classroom filters are applied here so later queries only
    /// calculate values for the requested report scope.
    /// </remarks>
    private async Task<IReadOnlyList<MonthlyEvaluationBaseReportRow>> GetBaseReportRowsAsync(
        Guid schoolId,
        Guid? gradeId,
        Guid? classRoomId,
        CancellationToken cancellationToken)
    {
        var classroomCourses = context.ClassRoomTeacherCourses
            .AsNoTracking()
            .Where(courseMap =>
                !courseMap.Deleted &&
                !courseMap.ClassRoom.Deleted &&
                !courseMap.Course.Deleted &&
                courseMap.ClassRoom.SchoolId == schoolId &&
                courseMap.Course.SchoolId == schoolId)
            .Select(courseMap => new
            {
                courseMap.ClassRoomId,
                courseMap.CourseId
            })
            .Distinct();

        var query =
            from student in context.Students.AsNoTracking()
            join school in context.Schools.AsNoTracking() on student.SchoolId equals school.Id
            join classroom in context.ClassRooms.AsNoTracking() on student.ClassRoomId equals classroom.Id
            join classroomCourse in classroomCourses on student.ClassRoomId equals classroomCourse.ClassRoomId
            join course in context.SchoolCourses.AsNoTracking() on classroomCourse.CourseId equals course.Id
            where student.SchoolId == schoolId &&
                  course.SchoolId == schoolId &&
                  !student.Deleted &&
                  !school.Deleted &&
                  !classroom.Deleted &&
                  !course.Deleted
            select new
            {
                StudentId = student.Id,
                StudentName = (student.FirstName ?? string.Empty) + " " + (student.LastName ?? string.Empty),
                SchoolId = school.Id,
                SchoolName = school.Name,
                ClassRoomId = classroom.Id,
                ClassRoomName = classroom.Name,
                classroom.GradeId,
                CourseId = course.Id,
                CourseName = course.Name,
                course.ExcludeFromMonthlyTotal
            };

        if (gradeId.HasValue)
        {
            query = query.Where(row => row.GradeId == gradeId.Value);
        }

        if (classRoomId.HasValue)
        {
            query = query.Where(row => row.ClassRoomId == classRoomId.Value);
        }

        return await query
            .OrderBy(row => row.SchoolId)
            .ThenBy(row => row.ClassRoomId)
            .ThenBy(row => row.StudentId)
            .ThenBy(row => row.CourseId)
            .Select(row => new MonthlyEvaluationBaseReportRow
            {
                StudentId = row.StudentId,
                StudentName = row.StudentName.Trim(),
                SchoolId = row.SchoolId,
                SchoolName = row.SchoolName,
                GradeId = row.GradeId,
                ClassRoomId = row.ClassRoomId,
                ClassRoomName = row.ClassRoomName,
                CourseId = row.CourseId,
                CourseName = row.CourseName,
                ExcludeFromMonthlyTotal = row.ExcludeFromMonthlyTotal
            })
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Calculates activity evaluation values per student, classroom, and course.
    /// </summary>
    /// <remarks>
    /// Business: the denominator is the total rate of all classroom activities created for the
    /// course during the period. The numerator is only the student's attended activity results;
    /// missing attendance contributes zero and is not removed from the denominator.
    /// </remarks>
    private async Task<IReadOnlyList<EvaluationPartResult>> GetActivityEvaluationsAsync(
        Guid schoolId,
        DateTime fromDate,
        DateTime toDateExclusive,
        IReadOnlyCollection<MonthlyEvaluationBaseReportRow> baseRows,
        CancellationToken cancellationToken)
    {
        var totalRates = await context.ClassRoomActivities
            .AsNoTracking()
            .Where(activity =>
                !activity.Deleted &&
                activity.ClassRoom.SchoolId == schoolId &&
                activity.ActivityDate >= fromDate &&
                activity.ActivityDate < toDateExclusive)
            .GroupBy(activity => new
            {
                activity.ClassRoomId,
                activity.CourseId
            })
            .Select(group => new EvaluationRateTotal
            {
                ClassRoomId = group.Key.ClassRoomId,
                CourseId = group.Key.CourseId,
                TotalRate = group.Sum(activity => activity.Rate)
            })
            .ToListAsync(cancellationToken);

        var studentResults = await (
            from studentActivity in context.ClassRoomStudentActivities.AsNoTracking()
            join activity in context.ClassRoomActivities.AsNoTracking()
                on studentActivity.ActivityId equals activity.Id
            where !studentActivity.Deleted &&
                  !activity.Deleted &&
                  studentActivity.IsAttend &&
                  activity.ClassRoom.SchoolId == schoolId &&
                  activity.ActivityDate >= fromDate &&
                  activity.ActivityDate < toDateExclusive
            group studentActivity by new
            {
                studentActivity.StudentId,
                activity.ClassRoomId,
                activity.CourseId
            }
            into groupedActivities
            select new EvaluationStudentResult
            {
                StudentId = groupedActivities.Key.StudentId,
                ClassRoomId = groupedActivities.Key.ClassRoomId,
                CourseId = groupedActivities.Key.CourseId,
                StudentResult = groupedActivities.Sum(activity => activity.Result ?? 0m)
            })
            .ToListAsync(cancellationToken);

        return BuildEvaluationPartResults(
            baseRows,
            totalRates,
            studentResults,
            ActivityEvaluationWeight);
    }

    /// <summary>
    /// Calculates assignment evaluation values per student, classroom, and course.
    /// </summary>
    /// <remarks>
    /// Business: the denominator is the total rate of all classroom assignments created for the
    /// course during the period. The numerator includes only delivered assignment results; not
    /// delivered or missing assignments count as zero.
    /// </remarks>
    private async Task<IReadOnlyList<EvaluationPartResult>> GetAssignmentEvaluationsAsync(
        Guid schoolId,
        DateTime fromDate,
        DateTime toDateExclusive,
        IReadOnlyCollection<MonthlyEvaluationBaseReportRow> baseRows,
        CancellationToken cancellationToken)
    {
        var totalRates = await context.ClassRoomAssignments
            .AsNoTracking()
            .Where(assignment =>
                !assignment.Deleted &&
                assignment.ClassRoom.SchoolId == schoolId &&
                assignment.AssignmentDate >= fromDate &&
                assignment.AssignmentDate < toDateExclusive)
            .GroupBy(assignment => new
            {
                assignment.ClassRoomId,
                assignment.CourseId
            })
            .Select(group => new EvaluationRateTotal
            {
                ClassRoomId = group.Key.ClassRoomId,
                CourseId = group.Key.CourseId,
                TotalRate = group.Sum(assignment => assignment.Rate)
            })
            .ToListAsync(cancellationToken);

        var studentResults = await (
            from studentAssignment in context.ClassRoomStudentAssignments.AsNoTracking()
            join assignment in context.ClassRoomAssignments.AsNoTracking()
                on studentAssignment.AssignmentId equals assignment.Id
            where !studentAssignment.Deleted &&
                  !assignment.Deleted &&
                  studentAssignment.IsDelivered == true &&
                  assignment.ClassRoom.SchoolId == schoolId &&
                  assignment.AssignmentDate >= fromDate &&
                  assignment.AssignmentDate < toDateExclusive
            group studentAssignment by new
            {
                studentAssignment.StudentId,
                assignment.ClassRoomId,
                assignment.CourseId
            }
            into groupedAssignments
            select new EvaluationStudentResult
            {
                StudentId = groupedAssignments.Key.StudentId,
                ClassRoomId = groupedAssignments.Key.ClassRoomId,
                CourseId = groupedAssignments.Key.CourseId,
                StudentResult = groupedAssignments.Sum(assignment => assignment.Result)
            })
            .ToListAsync(cancellationToken);

        return BuildEvaluationPartResults(
            baseRows,
            totalRates,
            studentResults,
            AssignmentEvaluationWeight);
    }

    /// <summary>
    /// Calculates exam evaluation values per student, classroom, and course.
    /// </summary>
    /// <remarks>
    /// Business: the denominator is the total rate of all classroom exams created for the course
    /// during the period. The numerator includes only attended exam results; absent or missing
    /// exam rows count as zero. Exam evaluation uses the forty-point weight.
    /// </remarks>
    private async Task<IReadOnlyList<EvaluationPartResult>> GetExamEvaluationsAsync(
        Guid schoolId,
        DateTime fromDate,
        DateTime toDateExclusive,
        IReadOnlyCollection<MonthlyEvaluationBaseReportRow> baseRows,
        CancellationToken cancellationToken)
    {
        var totalRates = await context.ClassRoomExams
            .AsNoTracking()
            .Where(exam =>
                !exam.Deleted &&
                exam.ClassRoom.SchoolId == schoolId &&
                exam.ExamDate >= fromDate &&
                exam.ExamDate < toDateExclusive)
            .GroupBy(exam => new
            {
                exam.ClassRoomId,
                exam.CourseId
            })
            .Select(group => new EvaluationRateTotal
            {
                ClassRoomId = group.Key.ClassRoomId,
                CourseId = group.Key.CourseId,
                TotalRate = group.Sum(exam => exam.Rate)
            })
            .ToListAsync(cancellationToken);

        var studentResults = await (
            from studentExam in context.ClassRoomStudentExams.AsNoTracking()
            join exam in context.ClassRoomExams.AsNoTracking()
                on studentExam.ExamId equals exam.Id
            where !studentExam.Deleted &&
                  !exam.Deleted &&
                  studentExam.IsAttend &&
                  exam.ClassRoom.SchoolId == schoolId &&
                  exam.ExamDate >= fromDate &&
                  exam.ExamDate < toDateExclusive
            group studentExam by new
            {
                studentExam.StudentId,
                exam.ClassRoomId,
                exam.CourseId
            }
            into groupedExams
            select new EvaluationStudentResult
            {
                StudentId = groupedExams.Key.StudentId,
                ClassRoomId = groupedExams.Key.ClassRoomId,
                CourseId = groupedExams.Key.CourseId,
                StudentResult = groupedExams.Sum(exam => exam.TotalResult ?? 0m)
            })
            .ToListAsync(cancellationToken);

        return BuildEvaluationPartResults(
            baseRows,
            totalRates,
            studentResults,
            ExamEvaluationWeight);
    }

    /// <summary>
    /// Calculates attendance evaluation values per student and classroom.
    /// </summary>
    /// <remarks>
    /// Business: attendance is not course-specific. Actual school days are distinct attendance
    /// dates found for each classroom in the period. Accepted student days are distinct attended
    /// dates plus distinct absence permit dates, capped to the classroom's actual school days.
    /// </remarks>
    private async Task<IReadOnlyList<AttendanceEvaluationResult>> GetAttendanceEvaluationsAsync(
        Guid schoolId,
        DateTime fromDate,
        DateTime toDateExclusive,
        IReadOnlyCollection<MonthlyEvaluationBaseReportRow> baseRows,
        CancellationToken cancellationToken)
    {
        var classRoomIds = baseRows
            .Select(row => row.ClassRoomId)
            .Distinct()
            .ToHashSet();

        var studentClassRooms = baseRows
            .Select(row => new StudentClassRoomKey(row.StudentId, row.ClassRoomId))
            .Distinct()
            .ToList();

        var classroomSchoolDays = await context.StudentAttendances
            .AsNoTracking()
            .Where(attendance =>
                !attendance.Deleted &&
                attendance.ClassRoom.SchoolId == schoolId &&
                attendance.Date >= fromDate &&
                attendance.Date < toDateExclusive)
            .Select(attendance => new
            {
                attendance.ClassRoomId,
                Date = attendance.Date.Date
            })
            .Distinct()
            .ToListAsync(cancellationToken);

        var attendedDays = await context.StudentAttendances
            .AsNoTracking()
            .Where(attendance =>
                !attendance.Deleted &&
                attendance.IsAttend &&
                attendance.Student.SchoolId == schoolId &&
                attendance.Date >= fromDate &&
                attendance.Date < toDateExclusive)
            .Select(attendance => new
            {
                attendance.StudentId,
                attendance.ClassRoomId,
                Date = attendance.Date.Date
            })
            .Distinct()
            .ToListAsync(cancellationToken);

        var absencePermitDays = await context.StudentAbsencePermits
            .AsNoTracking()
            .Where(permit =>
                !permit.Deleted &&
                permit.Status == Mdaresna.Doamin.Enums.AbsencePermitStatusEnum.Approved &&
                permit.Student.SchoolId == schoolId &&
                permit.ClassRoom.SchoolId == schoolId &&
                permit.Date >= fromDate &&
                permit.Date < toDateExclusive)
            .Select(permit => new
            {
                permit.StudentId,
                permit.ClassRoomId,
                Date = permit.Date.Date
            })
            .Distinct()
            .ToListAsync(cancellationToken);

        var schoolDaysByClassRoom = classroomSchoolDays
            .Where(day => classRoomIds.Contains(day.ClassRoomId))
            .GroupBy(day => day.ClassRoomId)
            .ToDictionary(
                group => group.Key,
                group => group.Select(day => day.Date).ToHashSet());

        var acceptedDaysByStudentClassRoom = attendedDays
            .Concat(absencePermitDays)
            .Where(day => classRoomIds.Contains(day.ClassRoomId))
            .GroupBy(day => new StudentClassRoomKey(day.StudentId, day.ClassRoomId))
            .ToDictionary(
                group => group.Key,
                group => group.Select(day => day.Date).ToHashSet());

        return studentClassRooms
            .Select(studentClassRoom =>
            {
                schoolDaysByClassRoom.TryGetValue(
                    studentClassRoom.ClassRoomId,
                    out var classroomDays);

                acceptedDaysByStudentClassRoom.TryGetValue(
                    studentClassRoom,
                    out var acceptedDays);

                var classroomDaysCount = classroomDays?.Count ?? 0;
                var acceptedDaysCount = classroomDays is null || acceptedDays is null
                    ? 0
                    : acceptedDays.Count(day => classroomDays.Contains(day));

                return new AttendanceEvaluationResult
                {
                    StudentId = studentClassRoom.StudentId,
                    ClassRoomId = studentClassRoom.ClassRoomId,
                    Evaluation = CalculateWeightedEvaluation(
                        acceptedDaysCount,
                        classroomDaysCount,
                        AttendanceEvaluationWeight)
                };
            })
            .ToList();
    }

    /// <summary>
    /// Combines base report rows with activity, assignment, exam, and attendance evaluations.
    /// </summary>
    /// <remarks>
    /// Business: missing evaluation parts become zero. Attendance is joined by student/classroom,
    /// while activity, assignment, and exam are joined by student/classroom/course. The total is
    /// the sum of the weighted parts and drives the final assessment label.
    /// </remarks>
    private static IReadOnlyList<StudentMonthlyEvaluationReportRow> MergeMonthlyEvaluationRows(
        IReadOnlyList<MonthlyEvaluationBaseReportRow> baseRows,
        IReadOnlyList<EvaluationPartResult> activityEvaluations,
        IReadOnlyList<EvaluationPartResult> assignmentEvaluations,
        IReadOnlyList<EvaluationPartResult> examEvaluations,
        IReadOnlyList<AttendanceEvaluationResult> attendanceEvaluations)
    {
        var activityByKey = activityEvaluations.ToDictionary(CreateEvaluationKey);
        var assignmentByKey = assignmentEvaluations.ToDictionary(CreateEvaluationKey);
        var examByKey = examEvaluations.ToDictionary(CreateEvaluationKey);
        var attendanceByKey = attendanceEvaluations.ToDictionary(CreateAttendanceKey);

        return baseRows
            .Select(row =>
            {
                var evaluationKey = new EvaluationKey(row.StudentId, row.ClassRoomId, row.CourseId);
                var attendanceKey = new AttendanceKey(row.StudentId, row.ClassRoomId);

                var activityEvaluation = activityByKey.TryGetValue(evaluationKey, out var activity)
                    ? activity.Evaluation
                    : 0m;
                var assignmentEvaluation = assignmentByKey.TryGetValue(evaluationKey, out var assignment)
                    ? assignment.Evaluation
                    : 0m;
                var examEvaluation = examByKey.TryGetValue(evaluationKey, out var exam)
                    ? exam.Evaluation
                    : 0m;
                var attendanceEvaluation = attendanceByKey.TryGetValue(attendanceKey, out var attendance)
                    ? attendance.Evaluation
                    : 0m;

                var totalEvaluation = RoundEvaluation(
                    activityEvaluation +
                    assignmentEvaluation +
                    attendanceEvaluation +
                    examEvaluation);

                return new StudentMonthlyEvaluationReportRow
                {
                    StudentId = row.StudentId,
                    StudentName = row.StudentName,
                    SchoolId = row.SchoolId,
                    SchoolName = row.SchoolName,
                    GradeId = row.GradeId,
                    ClassRoomId = row.ClassRoomId,
                    ClassRoomName = row.ClassRoomName,
                    CourseId = row.CourseId,
                    CourseName = row.CourseName,
                    ActivityEvaluation = activityEvaluation,
                    AssignmentEvaluation = assignmentEvaluation,
                    AttendanceEvaluation = attendanceEvaluation,
                    ExamEvaluation = examEvaluation,
                    TotalEvaluation = totalEvaluation,
                    Assessment = GetAssessment(totalEvaluation),
                    ExcludeFromMonthlyTotal = row.ExcludeFromMonthlyTotal
                };
            })
            .OrderBy(row => row.SchoolId)
            .ThenBy(row => row.ClassRoomId)
            .ThenBy(row => row.StudentId)
            .ThenBy(row => row.CourseId)
            .ToList();
    }

    /// <summary>
    /// Persists the generated monthly report rows as student report records.
    /// </summary>
    /// <remarks>
    /// Business: this method is the persistence boundary for the reporting worker. Each final
    /// row is saved as one <see cref="StudentReport"/> record, and the full row value is stored
    /// as JSON in <see cref="StudentReport.ReportDetails"/> for later review or publishing.
    /// When the report was created from a queue, the queue id is stored on every row so publish,
    /// review, and retry flows can identify exactly which request produced the rows.
    /// </remarks>
    private async Task<int> SaveReportRowsAsync(
        IReadOnlyCollection<StudentMonthlyEvaluationReportRow> reportRows,
        Guid? reportQueueId,
        Guid? monthId,
        string? weekName,
        StudentReportTypesEnum reportType,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (reportRows.Count == 0)
        {
            return 0;
        }

        if (reportingUnitOfWorkFactory is null || string.IsNullOrWhiteSpace(reportConnectionString))
        {
            throw new InvalidOperationException("Reporting database access is required to save student reports.");
        }

        var studentReports = CreateStudentReports(
            reportRows,
            reportQueueId,
            monthId,
            weekName,
            reportType);

        await using var unitOfWork = await reportingUnitOfWorkFactory.CreateAsync(
            reportConnectionString,
            cancellationToken);
        var repository = unitOfWork.Repository<StudentReport>();

        foreach (var studentReport in studentReports)
        {
            await repository.AddAsync(studentReport, cancellationToken);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return studentReports.Count;
    }

    /// <summary>
    /// Converts monthly evaluation rows into persisted student report entities.
    /// </summary>
    /// <remarks>
    /// Business: the report table stores the searchable report identity columns separately and
    /// keeps the complete calculated course row as JSON in ReportDetails. ReportQueueId links
    /// generated rows back to the reporting request that created them.
    /// </remarks>
    private static IReadOnlyList<StudentReport> CreateStudentReports(
        IReadOnlyCollection<StudentMonthlyEvaluationReportRow> reportRows,
        Guid? reportQueueId,
        Guid? monthId,
        string? weekName,
        StudentReportTypesEnum reportType)
    {
        var createdAt = DateTime.UtcNow;

        return reportRows
            .Select(row => new StudentReport
            {
                Id = Guid.NewGuid(),
                SchoolId = row.SchoolId,
                StudentId = row.StudentId,
                ReportQueueId = reportQueueId,
                GradeId = row.GradeId,
                ClassRoomId = row.ClassRoomId,
                MonthId = monthId,
                WeekName = weekName,
                ReportDetails = JsonSerializer.Serialize(row, ReportJsonOptions),
                CreatedAt = createdAt,
                IsActive = true,
                Version = 1,
                ReportType = reportType
            })
            .ToList();
    }

    /// <summary>
    /// Maps the final total evaluation score to the report assessment label.
    /// </summary>
    /// <remarks>
    /// Business: assessment thresholds are 90 for excellent, 80 for very good, 65 for good,
    /// 50 for accepted, and below 50 for weak.
    /// </remarks>
    private static string GetAssessment(decimal totalEvaluation)
    {
        if (totalEvaluation >= 90m)
        {
            return "امتياز";
        }

        if (totalEvaluation >= 80m)
        {
            return "جيد جدا";
        }

        if (totalEvaluation >= 65m)
        {
            return "جيد";
        }

        if (totalEvaluation >= 50m)
        {
            return "مقبول";
        }

        return "ضعيف";
    }

    /// <summary>
    /// Builds weighted evaluation results for activity, assignment, or exam parts.
    /// </summary>
    /// <remarks>
    /// Business: every base report row is returned even if no student result exists. The total
    /// rate is looked up by classroom/course, the student result by student/classroom/course,
    /// and missing values are treated as zero.
    /// </remarks>
    private static IReadOnlyList<EvaluationPartResult> BuildEvaluationPartResults(
        IReadOnlyCollection<MonthlyEvaluationBaseReportRow> baseRows,
        IReadOnlyList<EvaluationRateTotal> totalRates,
        IReadOnlyList<EvaluationStudentResult> studentResults,
        decimal weight)
    {
        var totalRateByKey = totalRates.ToDictionary(CreateCourseKey);
        var studentResultByKey = studentResults.ToDictionary(CreateEvaluationKey);

        return baseRows
            .Select(row =>
            {
                var courseKey = new CourseKey(row.ClassRoomId, row.CourseId);
                var evaluationKey = new EvaluationKey(row.StudentId, row.ClassRoomId, row.CourseId);
                var totalRate = totalRateByKey.TryGetValue(courseKey, out var rate)
                    ? rate.TotalRate
                    : 0m;
                var studentResult = studentResultByKey.TryGetValue(evaluationKey, out var result)
                    ? result.StudentResult
                    : 0m;

                return new EvaluationPartResult
                {
                    StudentId = row.StudentId,
                    ClassRoomId = row.ClassRoomId,
                    CourseId = row.CourseId,
                    TotalRate = totalRate,
                    StudentResult = studentResult,
                    Evaluation = CalculateWeightedEvaluation(studentResult, totalRate, weight)
                };
            })
            .ToList();
    }

    /// <summary>
    /// Calculates a weighted evaluation from a numerator, denominator, and part weight.
    /// </summary>
    /// <remarks>
    /// Business: score equals numerator divided by denominator multiplied by the part weight.
    /// A zero or negative denominator returns zero to avoid invalid score calculations.
    /// </remarks>
    private static decimal CalculateWeightedEvaluation(
        decimal numerator,
        decimal denominator,
        decimal weight)
    {
        if (denominator <= 0m)
        {
            return 0m;
        }

        return RoundEvaluation(numerator / denominator * weight);
    }

    /// <summary>
    /// Rounds an evaluation score to two decimal places.
    /// </summary>
    /// <remarks>
    /// Business: report scores use two decimal places and midpoint values are rounded away from
    /// zero so all evaluation parts follow the same financial-style rounding rule.
    /// </remarks>
    private static decimal RoundEvaluation(decimal value)
    {
        return Math.Round(value, 2, MidpointRounding.AwayFromZero);
    }

    /// <summary>
    /// Creates a classroom/course key for total rate lookups.
    /// </summary>
    /// <remarks>
    /// Business: total rates are shared by all students in the same classroom/course because the
    /// denominator is based on created work, not individual student participation.
    /// </remarks>
    private static CourseKey CreateCourseKey(EvaluationRateTotal total)
    {
        return new CourseKey(total.ClassRoomId, total.CourseId);
    }

    /// <summary>
    /// Creates a student/classroom/course key for raw student result lookups.
    /// </summary>
    /// <remarks>
    /// Business: student result aggregation is course-specific for activities, assignments, and
    /// exams, so the lookup must include all three identifiers.
    /// </remarks>
    private static EvaluationKey CreateEvaluationKey(EvaluationStudentResult result)
    {
        return new EvaluationKey(result.StudentId, result.ClassRoomId, result.CourseId);
    }

    /// <summary>
    /// Creates a student/classroom/course key for weighted evaluation lookups.
    /// </summary>
    /// <remarks>
    /// Business: merged report rows match calculated activity, assignment, and exam evaluations
    /// by the same student/classroom/course identity.
    /// </remarks>
    private static EvaluationKey CreateEvaluationKey(EvaluationPartResult result)
    {
        return new EvaluationKey(result.StudentId, result.ClassRoomId, result.CourseId);
    }

    /// <summary>
    /// Creates a student/classroom key for attendance evaluation lookups.
    /// </summary>
    /// <remarks>
    /// Business: attendance applies to the student in a classroom for the month, not to an
    /// individual course, so course id is intentionally excluded from this key.
    /// </remarks>
    private static AttendanceKey CreateAttendanceKey(AttendanceEvaluationResult result)
    {
        return new AttendanceKey(result.StudentId, result.ClassRoomId);
    }

    private sealed class EvaluationRateTotal
    {
        public Guid ClassRoomId { get; set; }
        public Guid CourseId { get; set; }
        public decimal TotalRate { get; set; }
    }

    private sealed class EvaluationStudentResult
    {
        public Guid StudentId { get; set; }
        public Guid ClassRoomId { get; set; }
        public Guid CourseId { get; set; }
        public decimal StudentResult { get; set; }
    }

    private readonly record struct CourseKey(Guid ClassRoomId, Guid CourseId);

    private readonly record struct EvaluationKey(Guid StudentId, Guid ClassRoomId, Guid CourseId);

    private readonly record struct AttendanceKey(Guid StudentId, Guid ClassRoomId);

    private readonly record struct StudentClassRoomKey(Guid StudentId, Guid ClassRoomId);
}
