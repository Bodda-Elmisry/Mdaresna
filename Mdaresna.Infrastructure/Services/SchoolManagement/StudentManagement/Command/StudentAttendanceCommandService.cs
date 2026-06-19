using Mdaresna.Doamin.DTOs.StudentManagement;
using Mdaresna.Doamin.Models.SchoolManagement.StudentManagement;
using Mdaresna.Infrastructure.Data;
using Mdaresna.Infrastructure.Helpers;
using Mdaresna.Repository.IRepositories.Base;
using Mdaresna.Repository.IServices.Base;
using Mdaresna.Repository.IServices.SchoolManagement.StudentManagement.Command;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mdaresna.Infrastructure.Services.SchoolManagement.StudentManagement.Command
{
    public class StudentAttendanceCommandService : IBaseCommandService<StudentAttendance>, IStudentAttendanceCommandService
    {
        private readonly IBaseCommandRepository<StudentAttendance> commandRepository;
        private readonly IBaseSharedRepository<StudentAttendance> sharedRepository;
        private readonly IBaseCommandBulkRepository<StudentAttendance> baseCommandBulkRepository;
        private readonly AppDbContext context;

        public StudentAttendanceCommandService(IBaseCommandRepository<StudentAttendance> commandRepository,
            IBaseSharedRepository<StudentAttendance> sharedRepository,
            IBaseCommandBulkRepository<StudentAttendance> baseCommandBulkRepository,
            AppDbContext context)
        {
            this.commandRepository = commandRepository;
            this.sharedRepository = sharedRepository;
            this.baseCommandBulkRepository = baseCommandBulkRepository;
            this.context = context;
        }
        public bool Create(StudentAttendance entity)
        {
            try
            {
                entity.Id = DataGenerationHelper.GenerateRowId();
                entity.CreateDate = DateTime.Now;
                entity.LastModifyDate = DateTime.Now;
                return commandRepository.Create(entity);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<bool> DeleteAsync(StudentAttendance entity)
        {
            try
            {
                entity = await sharedRepository.GetAsync(entity.Id);
                return commandRepository.Delete(entity);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public bool Update(StudentAttendance entity)
        {
            try
            {
                entity.LastModifyDate = DateTime.Now;
                return commandRepository.Update(entity);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        public async Task<bool> AddClassRoomAttendance(AddClassRoomAttendanceDTO attendanceDTO)
        {
            try
            {
                var submittedAttendances = (attendanceDTO.StudentsAttenndaceList ?? Enumerable.Empty<StudentAttendanceDTO>())
                    .GroupBy(item => item.StudentId)
                    .ToDictionary(group => group.Key, group => group.Last().IsAttend);

                var classroomStudentIds = await context.Students
                    .AsNoTracking()
                    .Where(student =>
                        student.ClassRoomId == attendanceDTO.ClassRoomId &&
                        student.Deleted == false)
                    .Select(student => student.Id)
                    .ToListAsync();

                var attendances = new List<StudentAttendance>();
                var now = DateTime.Now;

                foreach (var studentId in classroomStudentIds)
                {
                    submittedAttendances.TryGetValue(studentId, out var isAttend);

                    var att = new StudentAttendance
                    {
                        Id = DataGenerationHelper.GenerateRowId(),
                        Date = attendanceDTO.Date,
                        ClassRoomId = attendanceDTO.ClassRoomId,
                        StudentId = studentId,
                        IsAttend = isAttend,
                        SupervisorId = attendanceDTO.SupervisorId,
                        WeekDay = attendanceDTO.WeekDay,
                        CreateDate = now,
                        LastModifyDate = now
                    };

                    attendances.Add(att);
                }

                if (!attendances.Any())
                {
                    return true;
                }

                return await baseCommandBulkRepository.CreateBulk(attendances);

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

    }
}
