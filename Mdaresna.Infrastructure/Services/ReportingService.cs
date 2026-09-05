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
            var headerRow = await (
                from student in studentQuery
                join school in schoolQuery on student.SchoolId equals school.Id
                join classroom in classroomQuery on student.ClassRoomId equals classroom.Id
                join grade in gradeQuery on classroom.GradeId equals grade.Id
                join activeYear in activeYearQuery on school.Id equals activeYear.SchoolId into activeYears
                from activeYear in activeYears.DefaultIfEmpty()
                where student.Id == Request.StudentId
                select new StudentWeeklyReportHeaderRow
                {
                    StudentCode = student.Code,
                    StudentName = student.FirstName + " " + student.LastName,
                    SchoolName = school.Name,
                    ClassroomName = classroom.Name,
                    GradeName = grade.Name,
                    Year = activeYear != null ? activeYear.Name : string.Empty
                })
                .FirstOrDefaultAsync();

            if (headerRow is null)
                return null;

            // These collections must be loaded independently. Joining several one-to-many
            // sources in one SQL query multiplies their row counts and can exhaust memory.
            var attendanceRows = await ReportingServiceHelper.GetStudentAttendanceQuery(
                    unitOfWork,
                    Request.StudentId,
                    weekStart,
                    weekEndExclusive)
                .ToListAsync();

            var absencePermitRows = await ReportingServiceHelper.GetStudentAbsencePermitQuery(
                    unitOfWork,
                    Request.StudentId,
                    weekStart,
                    weekEndExclusive)
                .ToListAsync();

            var assignmentRows = await ReportingServiceHelper.GetStudentAssignmentQuery(
                    unitOfWork,
                    Request.StudentId,
                    weekStart,
                    weekEndExclusive)
                .ToListAsync();

            var examRows = await ReportingServiceHelper.GetStudentExamQuery(
                    unitOfWork,
                    Request.StudentId,
                    weekStart,
                    weekEndExclusive)
                .ToListAsync();

            var activityRows = await ReportingServiceHelper.GetStudentActivityQuery(
                    unitOfWork,
                    Request.StudentId,
                    weekStart,
                    weekEndExclusive)
                .ToListAsync();

            var noteRows = await ReportingServiceHelper.GetStudentNoteQuery(
                    unitOfWork,
                    Request.StudentId,
                    weekStart,
                    weekEndExclusive)
                .ToListAsync();

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

            var attendanceByDate = attendanceRows
                .GroupBy(q => q.AttendanceDate.Date)
                .ToDictionary(
                    q => q.Key,
                    q => q.First());

            var permitByDate = absencePermitRows
                .GroupBy(q => q.PermitDate.Date)
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
                    var isAttend = hasAttendance && attendanceValue!.AttendanceIsAttend;
                    var isAbsencePermit = !isAttend && hasPermit;
                    var attendanceStatus = isAttend
                        ? "Present"
                        : isAbsencePermit
                            ? "Permit"
                            : hasAttendance
                                ? "Absent"
                                : "NotRegistered";

                    return new StudentAttendanceWeeklyReportResponseDTO
                    {
                        WeekDate = weekDate,
                        WeekDay = hasAttendance
                            ? attendanceValue!.AttendanceWeekDay ?? currentDate.DayOfWeek.ToString()
                            : currentDate.DayOfWeek.ToString(),
                        IsAttend = isAttend,
                        IsExcption = isAbsencePermit,
                        IsAbsencePermit = isAbsencePermit,
                        AbsencePermitReason = isAbsencePermit ? permitValue!.PermitReason : null,
                        AttendanceStatus = attendanceStatus
                    };
                })
                .ToList();

            reportResult.AssignmentReport = assignmentRows
                .GroupBy(q => q.AssignmentId)
                .Select(q => q.First())
                .OrderBy(q => q.AssignmentDate)
                .Select(q => new StudentAssignmentWeeklyReportResponseDTO
                {
                    WeekDate = q.AssignmentDate.ToString("dd/MM/yyyy"),
                    WeekDay = q.AssignmentWeekDay ?? q.AssignmentDate.DayOfWeek.ToString(),
                    Subject = q.Subject ?? string.Empty,
                    TeacherName = $"{q.TeacherFirstName ?? string.Empty} {q.TeacherLastName ?? string.Empty}".Trim(),
                    IsDelievared = q.IsDelivered ?? false,
                    DeliveredDate = q.DeliveredDate != null
                        ? q.DeliveredDate.Value.ToString("dd/MM/yyyy")
                        : string.Empty,
                    AssignmentRate = q.AssignmentRate,
                    StudentResult = q.StudentResult ?? 0,
                    IsEvaluated = q.IsDelivered == true,
                    Details = q.Details ?? string.Empty
                })
                .ToList();

            reportResult.ExamReport = examRows
                .GroupBy(q => q.ExamId)
                .Select(q => q.First())
                .OrderBy(q => q.ExamDate)
                .Select(q => new StudentExamWeeklyReportResponseDTO
                {
                    WeekDate = q.ExamDate.ToString("dd/MM/yyyy"),
                    WeekDay = q.ExamWeekDay ?? q.ExamDate.DayOfWeek.ToString(),
                    Subject = q.Subject ?? string.Empty,
                    TeacherName = $"{q.TeacherFirstName ?? string.Empty} {q.TeacherLastName ?? string.Empty}".Trim(),
                    IsAttend = q.IsAttend ?? false,
                    ExamRate = q.ExamRate,
                    StudentResult = q.StudentResult ?? 0,
                    IsEvaluated = q.StudentResult.HasValue,
                    Details = q.Details ?? string.Empty
                })
                .ToList();

            reportResult.ActivityReport = activityRows
                .GroupBy(q => q.ActivityId)
                .Select(q => q.First())
                .OrderBy(q => q.ActivityDate)
                .Select(q => new StudentActivityWeeklyReportResponseDTO
                {
                    WeekDate = q.ActivityDate.ToString("dd/MM/yyyy"),
                    WeekDay = q.ActivityWeekDay ?? q.ActivityDate.DayOfWeek.ToString(),
                    Subject = q.Subject ?? string.Empty,
                    TeacherName = $"{q.TeacherFirstName ?? string.Empty} {q.TeacherLastName ?? string.Empty}".Trim(),
                    IsAttend = q.IsAttend ?? false,
                    ActivityRate = q.ExamRate,
                    StudentResult = q.StudentResult ?? 0,
                    IsEvaluated = q.StudentResult.HasValue,
                    Details = q.Details ?? string.Empty
                })
                .ToList();

            reportResult.NoteReport = noteRows
                .GroupBy(q => q.NoteId)
                .Select(q => q.First())
                .OrderBy(q => q.NoteDate)
                .Select(q => new StudentNoteWeeklyReportResponseDTO
                {
                    WeekDate = q.NoteDate.ToString("dd/MM/yyyy"),
                    WeekDay = q.NoteDate.DayOfWeek.ToString(),
                    Subject = q.Subject ?? string.Empty,
                    TeacherName = $"{q.TeacherFirstName ?? string.Empty} {q.TeacherLastName ?? string.Empty}".Trim(),
                    Details = q.Details ?? string.Empty
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

        private sealed class StudentWeeklyReportHeaderRow
        {
            public string StudentName { get; set; } = string.Empty;
            public string StudentCode { get; set; } = string.Empty;
            public string SchoolName { get; set; } = string.Empty;
            public string ClassroomName { get; set; } = string.Empty;
            public string GradeName { get; set; } = string.Empty;
            public string Year { get; set; } = string.Empty;
        }
    }
}
