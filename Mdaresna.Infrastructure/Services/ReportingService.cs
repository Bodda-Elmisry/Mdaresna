using Mdaresna.Doamin.DTOs.ReportingDTOs.StudentWeeklyReportDTOs;
using Mdaresna.Doamin.Enums;
using Mdaresna.Infrastructure.Helpers;
using Mdaresna.Repository.IServices;
using Mdaresna.Repository.IUnitOfWork;
using Microsoft.EntityFrameworkCore;

namespace Mdaresna.Infrastructure.Services
{
    internal class ReportingService : IReportingService
    {
        private readonly IQueryUnitOfWork unitOfWork;

        public ReportingService(IQueryUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }

        public async Task<StudentWeeklyReportResponseDTO?> GetStudentWeeklyReport(StudentWeeklyReportRequestDTO Request)
        {
            var (fromDate, toDate) = GetWeekRange(DateTime.Today, Request.SelectedWeek);
            var weekStart = fromDate.Date;
            var weekEndExclusive = toDate.Date.AddDays(1);

            var studentQuery = unitOfWork.StudentQueryRepository.GetQuery().AsNoTracking();
            var schoolQuery = unitOfWork.SchoolQueryRepository.GetQuery().AsNoTracking();
            var classroomQuery = unitOfWork.ClassRoomQueryRepository.GetQuery().AsNoTracking();
            var gradeQuery = unitOfWork.SchoolGradeQueryRepository.GetQuery().AsNoTracking();
            var activeYearQuery = unitOfWork.SchoolYearQueryRepository
                .GetQuery()
                .Where(q => q.IsActive && !q.Compleated)
                .AsNoTracking();
            var studentAttendanceFlatQuery = ReportingServiceHelper.GetStudentAttendanceQuery(
                unitOfWork,
                Request.StudentId,
                weekStart,
                weekEndExclusive);
            var studentAbsencePermitFlatQuery = ReportingServiceHelper.GetStudentAbsencePermitQuery(
                unitOfWork,
                Request.StudentId,
                weekStart,
                weekEndExclusive);
            var studentAssignmentFlatQuery = ReportingServiceHelper.GetStudentAssignmentQuery(
                unitOfWork,
                Request.StudentId,
                weekStart,
                weekEndExclusive);
            var studentExamFlatQuery = ReportingServiceHelper.GetStudentExamQuery(
                unitOfWork,
                Request.StudentId,
                weekStart,
                weekEndExclusive);
            var studentActivityFlatQuery = ReportingServiceHelper.GetStudentActivityQuery(
                unitOfWork,
                Request.StudentId,
                weekStart,
                weekEndExclusive);
            var studentNoteFlatQuery = ReportingServiceHelper.GetStudentNoteQuery(
                unitOfWork,
                Request.StudentId,
                weekStart,
                weekEndExclusive);

            var mainQuery =
                from student in studentQuery
                join school in schoolQuery on student.SchoolId equals school.Id
                join classroom in classroomQuery on student.ClassRoomId equals classroom.Id
                join grade in gradeQuery on classroom.GradeId equals grade.Id
                join activeYear in activeYearQuery on school.Id equals activeYear.SchoolId into activeYears
                from activeYear in activeYears.DefaultIfEmpty()
                where student.Id == Request.StudentId
                join attendance in studentAttendanceFlatQuery on student.Id equals attendance.StudentId into attendances
                from attendance in attendances.DefaultIfEmpty()
                join permit in studentAbsencePermitFlatQuery on student.Id equals permit.StudentId into permits
                from permit in permits.DefaultIfEmpty()
                join assignment in studentAssignmentFlatQuery on student.Id equals assignment.StudentId into assignments
                from assignment in assignments.DefaultIfEmpty()
                join exam in studentExamFlatQuery on student.Id equals exam.StudentId into exams
                from exam in exams.DefaultIfEmpty()
                join activity in studentActivityFlatQuery on student.Id equals activity.StudentId into activities
                from activity in activities.DefaultIfEmpty()
                join note in studentNoteFlatQuery on student.Id equals note.StudentId into notes
                from note in notes.DefaultIfEmpty()
                select new StudentWeeklyReportFlatRow
                {
                    StudentCode = student.Code,
                    StudentName = student.FirstName + " " + student.LastName,
                    SchoolName = school.Name,
                    ClassroomName = classroom.Name,
                    GradeName = grade.Name,
                    Year = activeYear != null ? activeYear.Name : string.Empty,
                    AttendanceDate = attendance != null ? attendance.AttendanceDate : null,
                    AttendanceWeekDay = attendance != null ? attendance.AttendanceWeekDay : null,
                    AttendanceIsAttend = attendance != null ? attendance.AttendanceIsAttend : null,
                    AbsencePermitDate = permit != null ? permit.PermitDate : null,
                    AbsencePermitReason = permit != null ? permit.PermitReason : null,
                    AssignmentId = assignment != null ? assignment.AssignmentId : null,
                    AssignmentDate = assignment != null ? assignment.AssignmentDate : null,
                    AssignmentWeekDay = assignment != null ? assignment.AssignmentWeekDay : null,
                    AssignmentSubject = assignment != null ? assignment.Subject : null,
                    AssignmentTeacherFirstName = assignment != null ? assignment.TeacherFirstName : null,
                    AssignmentTeacherLastName = assignment != null ? assignment.TeacherLastName : null,
                    IsDelivered = assignment != null ? assignment.IsDelivered : null,
                    DeliveredDate = assignment != null ? assignment.DeliveredDate : null,
                    AssignmentRate = assignment != null ? assignment.AssignmentRate : null,
                    StudentResult = assignment != null ? assignment.StudentResult : null,
                    AssignmentDetails = assignment != null ? assignment.Details : null,
                    ExamId = exam != null ? exam.ExamId : null,
                    ExamDate = exam != null ? exam.ExamDate : null,
                    ExamWeekDay = exam != null ? exam.ExamWeekDay : null,
                    ExamSubject = exam != null ? exam.Subject : null,
                    ExamTeacherFirstName = exam != null ? exam.TeacherFirstName : null,
                    ExamTeacherLastName = exam != null ? exam.TeacherLastName : null,
                    ExamIsAttend = exam != null ? exam.IsAttend : null,
                    ExamRate = exam != null ? exam.ExamRate : null,
                    StudentExamResult = exam != null ? exam.StudentResult : null,
                    ExamDetails = exam != null ? exam.Details : null,
                    ActivityId = activity != null ? activity.ActivityId : null,
                    ActivityDate = activity != null ? activity.ActivityDate : null,
                    ActivityWeekDay = activity != null ? activity.ActivityWeekDay : null,
                    ActivitySubject = activity != null ? activity.Subject : null,
                    ActivityTeacherFirstName = activity != null ? activity.TeacherFirstName : null,
                    ActivityTeacherLastName = activity != null ? activity.TeacherLastName : null,
                    ActivityIsAttend = activity != null ? activity.IsAttend : null,
                    ActivityRate = activity != null ? activity.ExamRate : null,
                    StudentActivityResult = activity != null ? activity.StudentResult : null,
                    ActivityDetails = activity != null ? activity.Details : null,
                    NoteId = note != null ? note.NoteId : null,
                    NoteDate = note != null ? note.NoteDate : null,
                    NoteSubject = note != null ? note.Subject : null,
                    NoteTeacherFirstName = note != null ? note.TeacherFirstName : null,
                    NoteTeacherLastName = note != null ? note.TeacherLastName : null,
                    NoteDetails = note != null ? note.Details : null
                };

            var rows = await mainQuery.ToListAsync();

            if (rows.Count == 0)
                return null;

            var headerRow = rows[0];

            var reportResult = new StudentWeeklyReportResponseDTO
            {
                StudentCode = headerRow.StudentCode,
                StudentName = headerRow.StudentName,
                SchoolName = headerRow.SchoolName,
                ClassroomName = headerRow.ClassroomName,
                GradeName = headerRow.GradeName,
                FromDate = fromDate.ToString("dd/MM/yyyy"),
                ToDate = toDate.ToString("dd/MM/yyyy"),
                Year = headerRow.Year
            };

            var attendanceByDate = rows
                .Where(q => q.AttendanceDate.HasValue)
                .GroupBy(q => q.AttendanceDate!.Value.Date)
                .ToDictionary(
                    q => q.Key,
                    q => q.First());

            var permitByDate = rows
                .Where(q => q.AbsencePermitDate.HasValue)
                .GroupBy(q => q.AbsencePermitDate!.Value.Date)
                .ToDictionary(
                    q => q.Key,
                    q => q.First());

            reportResult.AttendanceReport = Enumerable.Range(0, 7)
                .Select(offset =>
                {
                    var currentDate = weekStart.AddDays(offset);
                    var weekDate = currentDate.ToString("dd/MM/yyyy");
                    var hasAttendance = attendanceByDate.TryGetValue(currentDate.Date, out var attendanceValue);
                    var hasPermit = permitByDate.TryGetValue(currentDate.Date, out var permitValue);
                    var isAttend = hasAttendance && (attendanceValue!.AttendanceIsAttend ?? false);
                    var isAbsencePermit = !isAttend && hasPermit;

                    return new StudentAttendanceWeeklyReportResponseDTO
                    {
                        WeekDate = weekDate,
                        WeekDay = hasAttendance
                            ? attendanceValue!.AttendanceWeekDay ?? currentDate.DayOfWeek.ToString()
                            : currentDate.DayOfWeek.ToString(),
                        IsAttend = isAttend,
                        IsExcption = isAbsencePermit,
                        IsAbsencePermit = isAbsencePermit,
                        AbsencePermitReason = isAbsencePermit ? permitValue!.AbsencePermitReason : null,
                        AttendanceStatus = isAttend
                            ? "Present"
                            : isAbsencePermit
                                ? "Permit"
                                : "Absent"
                    };
                })
                .ToList();

            reportResult.AssignmentReport = rows
                .Where(q => q.AssignmentId.HasValue)
                .GroupBy(q => q.AssignmentId!.Value)
                .Select(q => q.First())
                .OrderBy(q => q.AssignmentDate)
                .Select(q => new StudentAssignmentWeeklyReportResponseDTO
                {
                    WeekDate = q.AssignmentDate!.Value.ToString("dd/MM/yyyy"),
                    WeekDay = q.AssignmentWeekDay ?? q.AssignmentDate.Value.DayOfWeek.ToString(),
                    Subject = q.AssignmentSubject ?? string.Empty,
                    TeacherName = $"{q.AssignmentTeacherFirstName ?? string.Empty} {q.AssignmentTeacherLastName ?? string.Empty}".Trim(),
                    IsDelievared = q.IsDelivered ?? false,
                    DeliveredDate = q.DeliveredDate != null
                        ? q.DeliveredDate.Value.ToString("dd/MM/yyyy")
                        : string.Empty,
                    AssignmentRate = q.AssignmentRate ?? 0,
                    StudentResult = q.StudentResult ?? 0,
                    Details = q.AssignmentDetails ?? string.Empty
                })
                .ToList();

            reportResult.ExamReport = rows
                .Where(q => q.ExamId.HasValue)
                .GroupBy(q => q.ExamId!.Value)
                .Select(q => q.First())
                .OrderBy(q => q.ExamDate)
                .Select(q => new StudentExamWeeklyReportResponseDTO
                {
                    WeekDate = q.ExamDate!.Value.ToString("dd/MM/yyyy"),
                    WeekDay = q.ExamWeekDay ?? q.ExamDate.Value.DayOfWeek.ToString(),
                    Subject = q.ExamSubject ?? string.Empty,
                    TeacherName = $"{q.ExamTeacherFirstName ?? string.Empty} {q.ExamTeacherLastName ?? string.Empty}".Trim(),
                    IsAttend = q.ExamIsAttend ?? false,
                    ExamRate = q.ExamRate ?? 0,
                    StudentResult = q.StudentExamResult ?? 0,
                    Details = q.ExamDetails ?? string.Empty
                })
                .ToList();

            reportResult.ActivityReport = rows
                .Where(q => q.ActivityId.HasValue)
                .GroupBy(q => q.ActivityId!.Value)
                .Select(q => q.First())
                .OrderBy(q => q.ActivityDate)
                .Select(q => new StudentActivityWeeklyReportResponseDTO
                {
                    WeekDate = q.ActivityDate!.Value.ToString("dd/MM/yyyy"),
                    WeekDay = q.ActivityWeekDay ?? q.ActivityDate.Value.DayOfWeek.ToString(),
                    Subject = q.ActivitySubject ?? string.Empty,
                    TeacherName = $"{q.ActivityTeacherFirstName ?? string.Empty} {q.ActivityTeacherLastName ?? string.Empty}".Trim(),
                    IsAttend = q.ActivityIsAttend ?? false,
                    ActivityRate = q.ActivityRate ?? 0,
                    StudentResult = q.StudentActivityResult ?? 0,
                    Details = q.ActivityDetails ?? string.Empty
                })
                .ToList();

            reportResult.NoteReport = rows
                .Where(q => q.NoteId.HasValue)
                .GroupBy(q => q.NoteId!.Value)
                .Select(q => q.First())
                .OrderBy(q => q.NoteDate)
                .Select(q => new StudentNoteWeeklyReportResponseDTO
                {
                    WeekDate = q.NoteDate!.Value.ToString("dd/MM/yyyy"),
                    WeekDay = q.NoteDate.Value.DayOfWeek.ToString(),
                    Subject = q.NoteSubject ?? string.Empty,
                    TeacherName = $"{q.NoteTeacherFirstName ?? string.Empty} {q.NoteTeacherLastName ?? string.Empty}".Trim(),
                    Details = q.NoteDetails ?? string.Empty
                }).ToList();

            return reportResult;
        }

        private static (DateTime FromDate, DateTime ToDate) GetWeekRange(
            DateTime referenceDate,
            ReportPeriodFilterEnum selectedWeek)
        {
            var currentWeekSaturday = GetWeekStartSaturday(referenceDate);
            var weeksOffset = selectedWeek switch
            {
                ReportPeriodFilterEnum.Current => 0,
                ReportPeriodFilterEnum.Last => 1,
                ReportPeriodFilterEnum.BeforeLast => 2,
                ReportPeriodFilterEnum.ThreeWeeksAgo => 3,
                _ => 0
            };

            var fromDate = currentWeekSaturday.AddDays(-7 * weeksOffset);
            var toDate = fromDate.AddDays(6);

            return (fromDate, toDate);
        }

        private static DateTime GetWeekStartSaturday(DateTime date)
        {
            var daysSinceSaturday =
                ((int)date.DayOfWeek - (int)DayOfWeek.Saturday + 7) % 7;

            return date.Date.AddDays(-daysSinceSaturday);
        }

        private sealed class StudentWeeklyReportFlatRow
        {
            public string StudentName { get; set; } = string.Empty;
            public string StudentCode { get; set; } = string.Empty;
            public string SchoolName { get; set; } = string.Empty;
            public string ClassroomName { get; set; } = string.Empty;
            public string GradeName { get; set; } = string.Empty;
            public string Year { get; set; } = string.Empty;
            public DateTime? AttendanceDate { get; set; }
            public string? AttendanceWeekDay { get; set; }
            public bool? AttendanceIsAttend { get; set; }
            public DateTime? AbsencePermitDate { get; set; }
            public string? AbsencePermitReason { get; set; }
            public Guid? AssignmentId { get; set; }
            public DateTime? AssignmentDate { get; set; }
            public string? AssignmentWeekDay { get; set; }
            public string? AssignmentSubject { get; set; }
            public string? AssignmentTeacherFirstName { get; set; }
            public string? AssignmentTeacherLastName { get; set; }
            public bool? IsDelivered { get; set; }
            public DateTime? DeliveredDate { get; set; }
            public decimal? AssignmentRate { get; set; }
            public decimal? StudentResult { get; set; }
            public string? AssignmentDetails { get; set; }
            public Guid? ExamId { get; set; }
            public DateTime? ExamDate { get; set; }
            public string? ExamWeekDay { get; set; }
            public string? ExamSubject { get; set; }
            public string? ExamTeacherFirstName { get; set; }
            public string? ExamTeacherLastName { get; set; }
            public bool? ExamIsAttend { get; set; }
            public decimal? ExamRate { get; set; }
            public decimal? StudentExamResult { get; set; }
            public string? ExamDetails { get; set; }
            public Guid? ActivityId { get; set; }
            public DateTime? ActivityDate { get; set; }
            public string? ActivityWeekDay { get; set; }
            public string? ActivitySubject { get; set; }
            public string? ActivityTeacherFirstName { get; set; }
            public string? ActivityTeacherLastName { get; set; }
            public bool? ActivityIsAttend { get; set; }
            public decimal? ActivityRate { get; set; }
            public decimal? StudentActivityResult { get; set; }
            public string? ActivityDetails { get; set; }
            public Guid? NoteId { get; set; }
            public DateTime? NoteDate { get; set; }
            public string? NoteSubject { get; set; }
            public string? NoteTeacherFirstName { get; set; }
            public string? NoteTeacherLastName { get; set; }
            public string? NoteDetails { get; set; }
        }
    }
}
