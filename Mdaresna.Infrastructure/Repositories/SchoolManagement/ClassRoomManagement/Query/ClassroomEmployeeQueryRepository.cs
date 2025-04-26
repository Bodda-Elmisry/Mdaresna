using Mdaresna.Doamin.DTOs.ClassRoomManagement;
using Mdaresna.Doamin.Models.SchoolManagement.ClassRoomManagement;
using Mdaresna.Infrastructure.Data;
using Mdaresna.Infrastructure.Repositories.Base;
using Mdaresna.Repository.IRepositories.SchoolManagement.ClassRoomManagement.Query;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mdaresna.Infrastructure.Repositories.SchoolManagement.ClassRoomManagement.Query
{
    internal class ClassroomEmployeeQueryRepository : BaseQueryRepository<ClassroomEmployee>, IClassroomEmployeeQueryRepository
    {
        private readonly AppDbContext context;

        public ClassroomEmployeeQueryRepository(AppDbContext context) : base(context)
        {
            this.context = context;
        }

        public async Task<ClassroomEmployee?> GetByIdAsync(Guid employeeId, Guid roomId)
        {
            return await context.ClassroomEmployees.FirstOrDefaultAsync(c => c.EmployeeId == employeeId &&
                                                                                   c.ClassRoomId == roomId && 
                                                                                   c.Deleted == false);
        }

        public async Task<ClassroomEmployeeResultDTO?> GetClassroomEmployeeAsync(Guid employeeId, Guid? roomId)
        {
            var row = await context.ClassroomEmployees.Include(c => c.Employee).Include(c => c.ClassRoom).FirstOrDefaultAsync(c => c.EmployeeId == employeeId &&
                                                                                    c.ClassRoomId == roomId && c.Deleted == false);

            if (row == null)
                return null;

            return new ClassroomEmployeeResultDTO
            {
                EmployeeId = row.EmployeeId,
                EmployeeName = $"{row.Employee.FirstName} {row.Employee.MiddelName} {row.Employee.LastName}",
                ClassRoomId = row.ClassRoomId,
                ClassRoomName = row.ClassRoom.Name
            };
        }

        public async Task<IEnumerable<ClassroomEmployeeResultDTO>?> GetClassroomsEmployeesAsync(Guid? employeeId, Guid? roomId, Guid schoolId)
        {
            var query = context.ClassroomEmployees
                            .Include(c => c.Employee).Include(c => c.ClassRoom)
                            .Where(c => c.ClassRoom.SchoolId == schoolId && c.Deleted == false)
                            .Select(c => new ClassroomEmployeeResultDTO
                            {
                                EmployeeId = c.EmployeeId,
                                EmployeeName = $"{c.Employee.FirstName} {c.Employee.MiddelName} {c.Employee.LastName}",
                                ClassRoomId = c.ClassRoomId,
                                ClassRoomName = c.ClassRoom.Name
                            });

            query = (employeeId != Guid.Empty) ? query.Where(c => c.EmployeeId == employeeId) : query;

            query = (roomId != null) ? query.Where(c => c.ClassRoomId == roomId) : query;

            return await query.OrderBy(c => c.ClassRoomId).ToListAsync();
        }

        public async Task<IEnumerable<ClassroomEmployee>?> GetEmployeeClassroomsAsync(Guid employeeId, Guid schoolId)
        {
            var query = context.ClassroomEmployees
                            .Include(c => c.Employee).Include(c => c.ClassRoom)
                            .Where(c => c.ClassRoom.SchoolId == schoolId && c.EmployeeId == employeeId && c.Deleted == false);

            return await query.OrderBy(c => c.ClassRoomId).ToListAsync();
        }

        public async Task<IEnumerable<ClassroomEmployeeResultDTO>> GetEmployeeDataAsync(Guid employeeId)
        {
            var data = await context.ClassroomEmployees
                            .Where(c => c.EmployeeId == employeeId && c.Deleted == false)
                            .Select(c => new ClassroomEmployeeResultDTO
                            {
                                EmployeeId = c.EmployeeId,
                                EmployeeName = $"{c.Employee.FirstName} {c.Employee.MiddelName} {c.Employee.LastName}",
                                ClassRoomId = c.ClassRoomId,
                                ClassRoomName = c.ClassRoom.Name
                            }).ToListAsync();

            return data;
        }

        public async Task<bool> IsExistAsync(Guid employeeId, Guid roomId)
        {
            return await context.ClassroomEmployees.AnyAsync(c => c.EmployeeId == employeeId &&
                                                                      c.ClassRoomId == roomId &&
                                                                      c.Deleted == false);
        }
    }
}
