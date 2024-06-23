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
    public class StudentQueryRepository : BaseQueryRepository<Student>, IStudentQueryRepository
    {
        private readonly AppDbContext context;

        public StudentQueryRepository(AppDbContext context) : base(context)
        {
            this.context = context;
        }

        public async Task<IEnumerable<Student>> GetStudentsBySchoolIdAsync(Guid schoolId)
        {
            return await context.Students.Where(s=> s.SchoolId == schoolId).ToListAsync();
        }

        public async Task<IEnumerable<Student>> GetStudentsBySchoolIdAndClassRoomIdAsync(Guid schoolId, Guid classroomId)
        {
            return await context.Students.Where(s => s.SchoolId == schoolId && s.ClassRoomId == classroomId).ToListAsync();
        }



    }
}
