using Mdaresna.Doamin.Models.SchoolManagement.StudentManagement;
using Mdaresna.Infrastructure.Data;
using Mdaresna.Infrastructure.Repositories.Base;
using Mdaresna.Repository.IRepositories.SchoolManagement.StudentManagement.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mdaresna.Infrastructure.Repositories.SchoolManagement.StudentManagement.Query
{
    public class StudentAttendanceQueryRepository : BaseQueryRepository<StudentAttendance>, IStudentAttendanceQueryRepository
    {
       public StudentAttendanceQueryRepository(AppDbContext context) : base(context)
        {
        }
    }
}