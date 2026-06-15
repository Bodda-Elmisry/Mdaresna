using Mdaresna.Doamin.DTOs.Common;
using Mdaresna.Doamin.DTOs.StudentManagement;
using Mdaresna.Doamin.Models.SchoolManagement.StudentManagement;
using Mdaresna.Infrastructure.Data;
using Mdaresna.Infrastructure.Repositories.Base;
using Mdaresna.Repository.IRepositories.SchoolManagement.StudentManagement.Query;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mdaresna.Infrastructure.Repositories.SchoolManagement.StudentManagement.Query
{
    public class StudentAttendanceQueryRepository : BaseQueryRepository<StudentAttendance>, IStudentAttendanceQueryRepository
    {
        private readonly AppDbContext context;

        private readonly AppSettingDTO _appSettings;
        public StudentAttendanceQueryRepository(AppDbContext context,
                                                IOptions<AppSettingDTO> appSettings) 
            : base(context)
        {
            this.context = context;
            this._appSettings = appSettings.Value;
        }

        public async Task<IEnumerable<StudentAttendanceResultDTO>> GetStudentsAttendancesAsync(
            Guid? studentId,
            Guid? classRoomId,
            int pageNumber)
        {
            int pageSize = _appSettings.PageSize ?? 30;
            pageNumber = pageNumber <= 0 ? 1 : pageNumber;

            var activePermits = context.StudentAbsencePermits
                .AsNoTracking()
                .Where(p => !p.Deleted && p.Status == Mdaresna.Doamin.Enums.AbsencePermitStatusEnum.Approved);

            var attendanceQuery =
                from attendance in context.StudentAttendances.AsNoTracking()
                where !attendance.Deleted
                join permit in activePermits
                    on new
                    {
                        attendance.StudentId,
                        Date = attendance.Date.Date
                    }
                    equals new
                    {
                        permit.StudentId,
                        Date = permit.Date.Date
                    }
                    into attendancePermits
                from permit in attendancePermits.DefaultIfEmpty()
                select new
                {
                    Id = attendance.Id,
                    Date = attendance.Date,
                    WeekDay = attendance.WeekDay,

                    ClassRoomId = attendance.ClassRoomId,
                    ClassRoomName = attendance.ClassRoom != null
                        ? attendance.ClassRoom.Name
                        : "",

                    SupervisorId = attendance.SupervisorId,
                    SupervisorFirstName = attendance.Supervisor != null ? attendance.Supervisor.FirstName : "",
                    SupervisorMiddleName = attendance.Supervisor != null ? attendance.Supervisor.MiddelName : "",
                    SupervisorLastName = attendance.Supervisor != null ? attendance.Supervisor.LastName : "",

                    StudentId = attendance.StudentId,
                    StudentFirstName = attendance.Student != null ? attendance.Student.FirstName : "",
                    StudentMiddleName = attendance.Student != null ? attendance.Student.MiddelName : "",
                    StudentLastName = attendance.Student != null ? attendance.Student.LastName : "",

                    IsAttend = attendance.IsAttend,
                    IsAbsencePermit = !attendance.IsAttend && permit != null,
                    AbsencePermitReason = permit != null ? permit.Reason : null,

                    AttendanceStatus = attendance.IsAttend
                        ? "Present"
                        : permit != null
                            ? "Permit"
                            : "Absent"
                };

            var permitOnlyQuery =
                from permit in activePermits
                where !context.StudentAttendances.Any(attendance =>
                    !attendance.Deleted &&
                    attendance.StudentId == permit.StudentId &&
                    attendance.Date.Date == permit.Date.Date)
                select new
                {
                    Id = permit.Id,
                    Date = permit.Date,
                    WeekDay = "",

                    ClassRoomId = permit.ClassRoomId,
                    ClassRoomName = permit.ClassRoom != null
                        ? permit.ClassRoom.Name
                        : "",

                    SupervisorId = permit.ParentId,
                    SupervisorFirstName = permit.Parent != null ? permit.Parent.FirstName : "",
                    SupervisorMiddleName = permit.Parent != null ? permit.Parent.MiddelName : "",
                    SupervisorLastName = permit.Parent != null ? permit.Parent.LastName : "",

                    StudentId = permit.StudentId,
                    StudentFirstName = permit.Student != null ? permit.Student.FirstName : "",
                    StudentMiddleName = permit.Student != null ? permit.Student.MiddelName : "",
                    StudentLastName = permit.Student != null ? permit.Student.LastName : "",

                    IsAttend = false,
                    IsAbsencePermit = true,
                    AbsencePermitReason = permit.Reason,

                    AttendanceStatus = "Permit"
                };

            var query = attendanceQuery.Concat(permitOnlyQuery);

            if (studentId.HasValue)
            {
                query = query.Where(a => a.StudentId == studentId.Value);
            }

            if (classRoomId.HasValue)
            {
                query = query.Where(a => a.ClassRoomId == classRoomId.Value);
            }

            var rows = await query
                .OrderByDescending(a => a.Date)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return rows.Select(a => new StudentAttendanceResultDTO
                {
                    Id = a.Id,
                    Date = a.Date,
                    WeekDay = string.IsNullOrWhiteSpace(a.WeekDay)
                        ? a.Date.DayOfWeek.ToString()
                        : a.WeekDay,

                    ClassRoomId = a.ClassRoomId,
                    ClassRoomName = a.ClassRoomName,

                    SupervisorId = a.SupervisorId,
                    SupervisorName = FormatFullName(
                        a.SupervisorFirstName,
                        a.SupervisorMiddleName,
                        a.SupervisorLastName),

                    StudentId = a.StudentId,
                    StudentName = FormatFullName(
                        a.StudentFirstName,
                        a.StudentMiddleName,
                        a.StudentLastName),

                    IsAttend = a.IsAttend,
                    IsAbsencePermit = a.IsAbsencePermit,
                    AbsencePermitReason = a.AbsencePermitReason,
                    AttendanceStatus = a.AttendanceStatus
                })
                .ToList();
        }

        private static string FormatFullName(params string?[] parts)
        {
            return string.Join(" ", parts
                .Where(part => !string.IsNullOrWhiteSpace(part))
                .Select(part => part!.Trim()));
        }

        //public async Task<IEnumerable<StudentAttendanceResultDTO>> GetStudentsAttendancesAsync(Guid? studentId, Guid? classRoomId, int pageNumber)
        //{
        //    int pagesize = _appSettings.PageSize != null ? _appSettings.PageSize.Value : 30;
        //    pageNumber = pageNumber <= 0 ? 1 : pageNumber;

        //    var activePermits = context.StudentAbsencePermits.Where(p => p.Deleted == false);

        //    var attendanceQuery =
        //        from attendance in context.StudentAttendances
        //        where attendance.Deleted == false
        //        join permit in activePermits
        //            on new { attendance.StudentId, Date = attendance.Date.Date }
        //            equals new { permit.StudentId, Date = permit.Date.Date }
        //            into attendancePermits
        //        from permit in attendancePermits.DefaultIfEmpty()
        //        select new StudentAttendanceResultDTO
        //        {
        //            Id = attendance.Id,
        //            Date = attendance.Date,
        //            WeekDay = attendance.WeekDay,
        //            ClassRoomId = attendance.ClassRoomId,
        //            ClassRoomName = attendance.ClassRoom != null ? attendance.ClassRoom.Name : string.Empty,
        //            SupervisorId = attendance.SupervisorId,
        //            SupervisorName = attendance.Supervisor != null
        //                ? $"{attendance.Supervisor.FirstName} {attendance.Supervisor.MiddelName} {attendance.Supervisor.LastName}"
        //                : string.Empty,
        //            StudentId = attendance.StudentId,
        //            StudentName = attendance.Student != null
        //                ? $"{attendance.Student.FirstName} {attendance.Student.MiddelName} {attendance.Student.LastName}"
        //                : string.Empty,
        //            IsAttend = attendance.IsAttend,
        //            IsAbsencePermit = !attendance.IsAttend && permit != null,
        //            AbsencePermitReason = permit != null ? permit.Reason : null,
        //            AttendanceStatus = attendance.IsAttend
        //                ? "Present"
        //                : permit != null
        //                    ? "Permit"
        //                    : "Absent"
        //        };

        //    var permitOnlyQuery =
        //        from permit in activePermits
        //        where !context.StudentAttendances.Any(attendance =>
        //            attendance.Deleted == false &&
        //            attendance.StudentId == permit.StudentId &&
        //            attendance.Date.Date == permit.Date.Date)
        //        select new StudentAttendanceResultDTO
        //        {
        //            Id = permit.Id,
        //            Date = permit.Date,
        //            WeekDay = permit.Date.DayOfWeek.ToString(),
        //            ClassRoomId = permit.ClassRoomId,
        //            ClassRoomName = permit.ClassRoom != null ? permit.ClassRoom.Name : string.Empty,
        //            SupervisorId = permit.ParentId,
        //            SupervisorName = permit.Parent != null
        //                ? $"{permit.Parent.FirstName} {permit.Parent.MiddelName} {permit.Parent.LastName}"
        //                : string.Empty,
        //            StudentId = permit.StudentId,
        //            StudentName = permit.Student != null
        //                ? $"{permit.Student.FirstName} {permit.Student.MiddelName} {permit.Student.LastName}"
        //                : string.Empty,
        //            IsAttend = false,
        //            IsAbsencePermit = true,
        //            AbsencePermitReason = permit.Reason,
        //            AttendanceStatus = "Permit"
        //        };

        //    var query = attendanceQuery.Concat(permitOnlyQuery);
        //    Console.WriteLine(query.ToQueryString());

        //    if (studentId != null)
        //    {
        //        query = query.Where(a=> a.StudentId == studentId);
        //    }

        //    if (classRoomId != null)
        //    {
        //        query = query.Where(a => a.ClassRoomId == classRoomId);
        //    }

        //    query = query.OrderByDescending(a => a.Date)
        //                 .Skip((pageNumber - 1) * pagesize)
        //                 .Take(pagesize);

        //    return await query.ToListAsync();

        //}




    }
}
