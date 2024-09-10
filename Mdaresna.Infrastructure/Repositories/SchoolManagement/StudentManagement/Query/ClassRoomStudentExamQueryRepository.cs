using Mdaresna.Doamin.DTOs.StudentManagement;
using Mdaresna.Doamin.Models.SchoolManagement.StudentManagement;
using Mdaresna.Infrastructure.Data;
using Mdaresna.Infrastructure.Repositories.Base;
using Mdaresna.Repository.IRepositories.SchoolManagement.StudentManagement.Query;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mdaresna.Infrastructure.Repositories.SchoolManagement.StudentManagement.Query
{
    public class ClassRoomStudentExamQueryRepository : BaseQueryRepository<ClassRoomStudentExam>, IClassRoomStudentExamQueryRepository
    {
        private readonly AppDbContext context;

        public ClassRoomStudentExamQueryRepository(AppDbContext context) : base(context)
        {
            this.context = context;
        }

        public async Task<IEnumerable<ClassRoomStudentExamResultDTO>> GetClassRoomStudentExamsListAsync(Guid StudentId, decimal? TotalResultFrom, decimal? TotalResultTo, bool? IsAttend)
        {
            var query = context.ClassRoomStudentExams.Include(e => e.StudentId)
                                                     .Include(e => e.Exam)
                                                     .Include(e => e.Exam.Month)
                                                     .Include(e => e.Exam.ClassRoom)
                                                     .Include(e => e.Exam.Course)
                                                     .Include(e => e.Exam.Supervisor)
                                                     .Where(e => e.StudentId == StudentId);

            if (TotalResultFrom != null && TotalResultTo != null)
                query = query.Where(s => s.TotalResult >= TotalResultFrom && s.TotalResult <= TotalResultTo);
            else if (TotalResultFrom != null && TotalResultTo == null)
                query = query.Where(s => s.TotalResult == TotalResultFrom);

            if (IsAttend != null)
                query = query.Where(e => e.IsAttend == IsAttend);

            return await query.Select(s => new ClassRoomStudentExamResultDTO
            {
                ExamId = s.ExamId,
                ExamDate = s.Exam.ExamDate,
                WeekDay = s.Exam.WeekDay,
                ExamDetails = s.Exam.Details,
                ClassRoomId = s.Exam.ClassRoomId,
                ClassRoom = s.Exam.ClassRoom.Name,
                SupervisorId = s.Exam.SupervisorId,
                SupervisorName = $"{s.Exam.Supervisor.FirstName} {s.Exam.Supervisor.MiddelName} {s.Exam.Supervisor.LastName}",
                MonthId = s.Exam.MonthId,
                Month = s.Exam.Month.Name,
                CourseId = s.Exam.CourseId,
                CourseName = s.Exam.Course.Name,
                ExamRate = s.Exam.Rate,
                StudentId = s.StudentId,
                StudentName = $"{s.Student.FirstName} {s.Student.MiddelName} {s.Student.LastName}",
                TotalResults = s.TotalResult,
                IsAttend = s.IsAttend
            }).ToListAsync();


        }

        public async Task<ClassRoomStudentExamResultDTO> GetClassRoomStudentExamViewAsync(Guid studentId, Guid ExamId)
        {
            var studentExam = await context.ClassRoomStudentExams.Include(e => e.StudentId)
                                                     .Include(e => e.Exam)
                                                     .Include(e => e.Exam.Month)
                                                     .Include(e => e.Exam.ClassRoom)
                                                     .Include(e => e.Exam.Course)
                                                     .Include(e => e.Exam.Supervisor)
                                                     .FirstOrDefaultAsync(s => s.StudentId == studentId && s.ExamId == ExamId);
            return studentExam == null ? null : new ClassRoomStudentExamResultDTO
            {
                ExamId = studentExam.ExamId,
                ExamDate = studentExam.Exam.ExamDate,
                WeekDay = studentExam.Exam.WeekDay,
                ExamDetails = studentExam.Exam.Details,
                ClassRoomId = studentExam.Exam.ClassRoomId,
                ClassRoom = studentExam.Exam.ClassRoom.Name,
                SupervisorId = studentExam.Exam.SupervisorId,
                SupervisorName = $"{studentExam.Exam.Supervisor.FirstName} {studentExam.Exam.Supervisor.MiddelName} {studentExam.Exam.Supervisor.LastName}",
                MonthId = studentExam.Exam.MonthId,
                Month = studentExam.Exam.Month.Name,
                CourseId = studentExam.Exam.CourseId,
                CourseName = studentExam.Exam.Course.Name,
                ExamRate = studentExam.Exam.Rate,
                StudentId = studentExam.StudentId,
                StudentName = $"{studentExam.Student.FirstName} {studentExam.Student.MiddelName} {studentExam.Student.LastName}",
                TotalResults = studentExam.TotalResult,
                IsAttend = studentExam.IsAttend
            };
        }

        public async Task<ClassRoomStudentExam?> GetClassRoomStudentExamAsync(Guid studentId, Guid ExamId)
        {
            return await context.ClassRoomStudentExams.FirstOrDefaultAsync(s => s.StudentId == studentId && s.ExamId == ExamId);
        }





    }
}
