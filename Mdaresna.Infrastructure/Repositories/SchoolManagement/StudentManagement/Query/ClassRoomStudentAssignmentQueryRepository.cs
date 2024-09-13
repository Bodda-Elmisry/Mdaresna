using Mdaresna.Doamin.DTOs.Common;
using Mdaresna.Doamin.DTOs.StudentManagement;
using Mdaresna.Doamin.Models.SchoolManagement.ClassRoomManagement;
using Mdaresna.Doamin.Models.SchoolManagement.StudentManagement;
using Mdaresna.Infrastructure.Data;
using Mdaresna.Infrastructure.Repositories.Base;
using Mdaresna.Repository.IRepositories.SchoolManagement.StudentManagement.Query;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mdaresna.Infrastructure.Repositories.SchoolManagement.StudentManagement.Query
{
    public class ClassRoomStudentAssignmentQueryRepository : BaseQueryRepository<ClassRoomStudentAssignment>, 
        IClassRoomStudentAssignmentQueryRepository
    {
        private readonly AppDbContext context;
        private readonly AppSettingDTO appSettings;

        public ClassRoomStudentAssignmentQueryRepository(AppDbContext context,
                                                IOptions<AppSettingDTO> appSettings) : base(context)
        {
            this.context = context;
            this.appSettings = appSettings.Value;
        }

        public async Task<IEnumerable<ClassRoomStudentAssignmentResultDTO>> GetStudentAssignmentsListAsync(Guid StudentId,
                                                                                                     Guid? AssignementId,
                                                                                                     decimal? ResultFrom,
                                                                                                     decimal? ResultTo,
                                                                                                     bool? IsDelivered,
                                                                                                     DateTime? DeliveredDateFrom,
                                                                                                     DateTime? DeliveredDateTo,
                                                                                                     int pageNumber)
        {
            #region Create Query
            int pagesize = appSettings.PageSize != null ? appSettings.PageSize.Value : 30;

            var query = context.ClassRoomStudentAssignments.Include(s => s.Student).Include(s => s.Assignment)
                                                           .Include(s => s.Assignment.Course)
                                                           .Include(s => s.Assignment.ClassRoom)
                                                           .Include(s => s.Assignment.Supervisor)
                                                           .Where(s => s.StudentId == StudentId);

            if (AssignementId != null)
                query = query.Where(s => s.AssignmentId == AssignementId);

            if (ResultFrom != null && ResultTo != null)
                query = query.Where(s => s.Result >= ResultFrom && s.Result <= ResultTo);
            else if (ResultFrom != null && ResultTo == null)
                query = query.Where(s => s.Result == ResultFrom);

            if (IsDelivered != null)
                query = query.Where(s => s.IsDelivered == IsDelivered);

            DeliveredDateFrom = DeliveredDateFrom != null ? new DateTime(DeliveredDateFrom.Value.Year, DeliveredDateFrom.Value.Month, DeliveredDateFrom.Value.Day) : null;

            DeliveredDateTo = DeliveredDateTo != null ? new DateTime(DeliveredDateTo.Value.Year, DeliveredDateTo.Value.Month, DeliveredDateTo.Value.Day) : null;

            if (DeliveredDateFrom != null && DeliveredDateTo != null)
                query = query.Where(s => s.DeliveredDate != null && s.DeliveredDate.Value >= DeliveredDateFrom.Value && s.DeliveredDate.Value <= DeliveredDateTo.Value);
            else if (DeliveredDateFrom != null && DeliveredDateTo == null)
            {
                query = query.Where(s => s.DeliveredDate != null && s.DeliveredDate.Value == DeliveredDateFrom.Value);
            }
            query = query.OrderByDescending(q => q.Assignment.AssignmentDate)
             .Skip((pageNumber - 1) * pagesize)
             .Take(pagesize);

            #endregion

            var result = await query
                                .Select(s => new ClassRoomStudentAssignmentResultDTO
                                {
                                    AssignmentId = s.AssignmentId,
                                    AssignmentDate = s.Assignment.AssignmentDate,
                                    WeekDay = s.Assignment.WeekDay,
                                    Details = s.Assignment.Details,
                                    CourseId = s.Assignment.CourseId,
                                    CourseName = s.Assignment.Course.Name,
                                    ClassRoomId = s.Assignment.ClassRoomId,
                                    ClassRoomName = s.Assignment.ClassRoom.Name,
                                    SupervisorId = s.Assignment.SupervisorId,
                                    SupervisorName = $"{s.Assignment.Supervisor.FirstName} {s.Assignment.Supervisor.MiddelName} {s.Assignment.Supervisor.LastName}",
                                    AssignmentRate = s.Assignment.Rate,
                                    StudentId = s.StudentId,
                                    StudentName = $"{s.Student.FirstName} {s.Student.MiddelName} {s.Student.LastName}",
                                    Result = s.Result,
                                    IsDelivered = s.IsDelivered,
                                    DeliveredDate = s.DeliveredDate
                                })
                                .ToListAsync();

            return result;
        }

        public async Task<ClassRoomStudentAssignment?> GetClassRoomStudentAssignmentAsync(Guid studentId, Guid AssignmentId)
        {
            return await context.ClassRoomStudentAssignments.FirstOrDefaultAsync(s=> s.StudentId == studentId && s.AssignmentId == AssignmentId);
        }

        public async Task<ClassRoomStudentAssignmentResultDTO?> GetClassRoomStudentAssignmentViewAsync(Guid studentId, Guid AssignmentId)
        {
            var assignment = await context.ClassRoomStudentAssignments.Include(s => s.Student).Include(s => s.Assignment)
                                                           .Include(s => s.Assignment.Course)
                                                           .Include(s => s.Assignment.ClassRoom)
                                                           .Include(s => s.Assignment.Supervisor)
                                                           .FirstOrDefaultAsync(s => s.StudentId == studentId && s.AssignmentId == AssignmentId);

            return assignment == null ? null : new ClassRoomStudentAssignmentResultDTO
            {
                AssignmentId = assignment.AssignmentId,
                AssignmentDate = assignment.Assignment.AssignmentDate,
                WeekDay = assignment.Assignment.WeekDay,
                Details = assignment.Assignment.Details,
                CourseId = assignment.Assignment.CourseId,
                CourseName = assignment.Assignment.Course.Name,
                ClassRoomId = assignment.Assignment.ClassRoomId,
                ClassRoomName = assignment.Assignment.ClassRoom.Name,
                SupervisorId = assignment.Assignment.SupervisorId,
                SupervisorName = $"{assignment.Assignment.Supervisor.FirstName} {assignment.Assignment.Supervisor.MiddelName} {assignment.Assignment.Supervisor.LastName}",
                AssignmentRate = assignment.Assignment.Rate,
                StudentId = assignment.StudentId,
                StudentName = $"{assignment.Student.FirstName} {assignment.Student.MiddelName} {assignment.Student.LastName}",
                Result = assignment.Result,
                IsDelivered = assignment.IsDelivered,
                DeliveredDate = assignment.DeliveredDate
            };
        }















    }
}
