using Mdaresna.Repository.IUnitOfWork;
using Microsoft.EntityFrameworkCore;

namespace Mdaresna.Infrastructure.Helpers
{
    internal static class ReportingServiceHelper
    {
        public static IQueryable<StudentAttendanceFlatQueryResult> GetStudentAttendanceQuery(
            IQueryUnitOfWork unitOfWork,
            Guid studentId,
            DateTime weekStart,
            DateTime weekEndExclusive)
        {
            return unitOfWork.StudentAttendanceQueryRepository
                .GetQuery()
                .Where(q =>
                    q.StudentId == studentId &&
                    q.Date >= weekStart &&
                    q.Date < weekEndExclusive)
                .AsNoTracking()
                .Select(q => new StudentAttendanceFlatQueryResult
                {
                    StudentId = q.StudentId,
                    AttendanceDate = q.Date,
                    AttendanceWeekDay = q.WeekDay,
                    AttendanceIsAttend = q.IsAttend
                });
        }

        public static IQueryable<StudentAbsencePermitFlatQueryResult> GetStudentAbsencePermitQuery(
            IQueryUnitOfWork unitOfWork,
            Guid studentId,
            DateTime weekStart,
            DateTime weekEndExclusive)
        {
            return unitOfWork.StudentAbsencePermitQueryRepository
                .GetQuery()
                .Where(q =>
                    q.StudentId == studentId &&
                    q.Date >= weekStart &&
                    q.Date < weekEndExclusive)
                .AsNoTracking()
                .Select(q => new StudentAbsencePermitFlatQueryResult
                {
                    StudentId = q.StudentId,
                    PermitDate = q.Date,
                    PermitReason = q.Reason
                });
        }

        public static IQueryable<StudentAssignmentFlatQueryResult> GetStudentAssignmentQuery(
            IQueryUnitOfWork unitOfWork,
            Guid studentId,
            DateTime weekStart,
            DateTime weekEndExclusive)
        {
            var studentAssignments = unitOfWork.ClassRoomStudentAssignmentQueryRepository.GetQuery().AsNoTracking();
            var assignments = unitOfWork.ClassRoomAssignmentQueryRepository.GetQuery().AsNoTracking();
            var courses = unitOfWork.SchoolCourseQueryRepository.GetQuery().AsNoTracking();
            var users = unitOfWork.UserQueryRepository.GetQuery().AsNoTracking();

            return from studentAssignment in studentAssignments
                   join assignment in assignments on studentAssignment.AssignmentId equals assignment.Id
                   join course in courses on assignment.CourseId equals course.Id into assignmentCourses
                   from course in assignmentCourses.DefaultIfEmpty()
                   join user in users on assignment.SupervisorId equals user.Id into assignmentUsers
                   from user in assignmentUsers.DefaultIfEmpty()
                   where studentAssignment.StudentId == studentId &&
                         assignment.AssignmentDate >= weekStart &&
                         assignment.AssignmentDate < weekEndExclusive
                   select new StudentAssignmentFlatQueryResult
                   {
                       StudentId = studentAssignment.StudentId,
                       AssignmentId = studentAssignment.AssignmentId,
                       AssignmentDate = assignment.AssignmentDate,
                       AssignmentWeekDay = assignment.WeekDay,
                       Subject = course != null ? course.Name : null,
                       TeacherFirstName = user != null ? user.FirstName : null,
                       TeacherLastName = user != null ? user.LastName : null,
                       IsDelivered = studentAssignment.IsDelivered,
                       DeliveredDate = studentAssignment.DeliveredDate,
                       AssignmentRate = assignment.Rate,
                       StudentResult = studentAssignment.Result,
                       Details = assignment.Details
                   };
        }


        public static IQueryable<StudentExamFlatQueryResult> GetStudentExamQuery(
                IQueryUnitOfWork unitOfWork,
                Guid studentId,
                DateTime weekStart,
                DateTime weekEndExclusive)
        {
            var studentExams = unitOfWork.ClassRoomStudentExamQueryRepository.GetQuery().AsNoTracking();
            var exams = unitOfWork.ClassRoomExamQueryRepository.GetQuery().AsNoTracking();
            var courses = unitOfWork.SchoolCourseQueryRepository.GetQuery().AsNoTracking();
            var users = unitOfWork.UserQueryRepository.GetQuery().AsNoTracking();

            return from studentExam in studentExams
                   join exam in exams on studentExam.ExamId equals exam.Id
                   join course in courses on exam.CourseId equals course.Id into examCourses
                   from course in examCourses.DefaultIfEmpty()
                   join user in users on exam.SupervisorId equals user.Id into examUsers
                   from user in examUsers.DefaultIfEmpty()
                   where studentExam.StudentId == studentId &&
                         exam.ExamDate >= weekStart &&
                         exam.ExamDate < weekEndExclusive
                   select new StudentExamFlatQueryResult
                   {
                       StudentId = studentExam.StudentId,
                       ExamId = studentExam.ExamId,
                       ExamDate = exam.ExamDate,
                       ExamWeekDay = exam.WeekDay,
                       Subject = course != null ? course.Name : null,
                       TeacherFirstName = user != null ? user.FirstName : null,
                       TeacherLastName = user != null ? user.LastName : null,
                       IsAttend = studentExam.IsAttend,
                       ExamRate = exam.Rate,
                       StudentResult = studentExam.TotalResult,
                       Details = exam.Details
                   };
        }


        public static IQueryable<StudentActivityFlatQueryResult> GetStudentActivityQuery(
                IQueryUnitOfWork unitOfWork,
                Guid studentId,
                DateTime weekStart,
                DateTime weekEndExclusive)
        {
            var studentActivities = unitOfWork.ClassRoomStudentActivityQueryRepository.GetQuery().AsNoTracking();
            var activities = unitOfWork.ClassRoomActivityQueryRepository.GetQuery().AsNoTracking();
            var courses = unitOfWork.SchoolCourseQueryRepository.GetQuery().AsNoTracking();
            var users = unitOfWork.UserQueryRepository.GetQuery().AsNoTracking();

            return from studentActivity in studentActivities
                   join activity in activities on studentActivity.ActivityId equals activity.Id
                   join course in courses on activity.CourseId equals course.Id into activityCourses
                   from course in activityCourses.DefaultIfEmpty()
                   join user in users on activity.SupervisorId equals user.Id into activityUsers
                   from user in activityUsers.DefaultIfEmpty()
                   where studentActivity.StudentId == studentId &&
                         activity.ActivityDate >= weekStart &&
                         activity.ActivityDate < weekEndExclusive
                   select new StudentActivityFlatQueryResult
                   {
                       StudentId = studentActivity.StudentId,
                       ActivityId = studentActivity.ActivityId,
                       ActivityDate = activity.ActivityDate,
                       ActivityWeekDay = activity.WeekDay,
                       Subject = course != null ? course.Name : null,
                       TeacherFirstName = user != null ? user.FirstName : null,
                       TeacherLastName = user != null ? user.LastName : null,
                       IsAttend = studentActivity.IsAttend,
                       ExamRate = activity.Rate,
                       StudentResult = studentActivity.Result,
                       Details = activity.Details
                   };
        }


        public static IQueryable<StudentNoteFlatQueryResult> GetStudentNoteQuery(
                IQueryUnitOfWork unitOfWork,
                Guid studentId,
                DateTime weekStart,
                DateTime weekEndExclusive)
        {
            var notes = unitOfWork.StudentNoteQueryRepository.GetQuery().AsNoTracking();
            var courses = unitOfWork.SchoolCourseQueryRepository.GetQuery().AsNoTracking();
            var users = unitOfWork.UserQueryRepository.GetQuery().AsNoTracking();

            return from note in notes
                   join course in courses on note.CourseId equals course.Id into noteCourses
                   from course in noteCourses.DefaultIfEmpty()
                   join user in users on note.SupervisorId equals user.Id into noteUsers
                   from user in noteUsers.DefaultIfEmpty()
                   where note.StudentId == studentId &&
                         note.Date >= weekStart &&
                         note.Date < weekEndExclusive
                   select new StudentNoteFlatQueryResult
                   {
                       StudentId = note.StudentId,
                       NoteId = note.Id,
                       Subject = course != null ? course.Name : string.Empty,
                       TeacherFirstName = user != null ? user.FirstName : null,
                       TeacherLastName = user != null ? user.LastName : null,
                       Details = note.Notes,
                       NoteDate = note.Date
                   };
        }


        internal sealed class StudentAttendanceFlatQueryResult
        {
            public Guid StudentId { get; set; }
            public DateTime AttendanceDate { get; set; }
            public string? AttendanceWeekDay { get; set; }
            public bool AttendanceIsAttend { get; set; }
        }

        internal sealed class StudentAbsencePermitFlatQueryResult
        {
            public Guid StudentId { get; set; }
            public DateTime PermitDate { get; set; }
            public string? PermitReason { get; set; }
        }

        internal sealed class StudentAssignmentFlatQueryResult
        {
            public Guid StudentId { get; set; }
            public Guid AssignmentId { get; set; }
            public DateTime AssignmentDate { get; set; }
            public string? AssignmentWeekDay { get; set; }
            public string? Subject { get; set; }
            public string? TeacherFirstName { get; set; }
            public string? TeacherLastName { get; set; }
            public bool? IsDelivered { get; set; }
            public DateTime? DeliveredDate { get; set; }
            public decimal AssignmentRate { get; set; }
            public decimal? StudentResult { get; set; }
            public string? Details { get; set; }
        }

        internal sealed class StudentExamFlatQueryResult
        {
            public Guid StudentId { get; set; }
            public Guid ExamId { get; set; }
            public DateTime ExamDate { get; set; }
            public string? ExamWeekDay { get; set; }
            public string? Subject { get; set; }
            public string? TeacherFirstName { get; set; }
            public string? TeacherLastName { get; set; }
            public bool? IsAttend { get; set; }
            public decimal ExamRate { get; set; }
            public decimal? StudentResult { get; set; }
            public string? Details { get; set; }
        }

        internal sealed class StudentActivityFlatQueryResult
        {
            public Guid StudentId { get; set; }
            public Guid ActivityId { get; set; }
            public DateTime ActivityDate { get; set; }
            public string? ActivityWeekDay { get; set; }
            public string? Subject { get; set; }
            public string? TeacherFirstName { get; set; }
            public string? TeacherLastName { get; set; }
            public bool? IsAttend { get; set; }
            public decimal ExamRate { get; set; }
            public decimal? StudentResult { get; set; }
            public string? Details { get; set; }
        }

        internal sealed class StudentNoteFlatQueryResult
        {
            public Guid StudentId { get; set; }
            public Guid NoteId { get; set; }
            public DateTime NoteDate { get; set; }
            public string? Subject { get; set; }
            public string? TeacherFirstName { get; set; }
            public string? TeacherLastName { get; set; }
            public string? Details { get; set; }
        }
    }
}
