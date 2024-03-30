using Mdaresna.Doamin.Models.SchoolManagement.StudentManagement;
using Mdaresna.Infrastructure.Services.Base;
using Mdaresna.Repository.IRepositories.Base;
using Mdaresna.Repository.IServices.SchoolManagement.StudentManagement.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mdaresna.Infrastructure.Services.SchoolManagement.StudentManagement.Query
{
    public class StudentAttendanceQueryService : BaseQueryService<StudentAttendance>, IStudentAttendanceQueryService
    {
        private readonly IBaseQueryRepository<StudentAttendance> queryRepository;
        private readonly IBaseSharedRepository<StudentAttendance> sharedRepository;

        public StudentAttendanceQueryService(IBaseQueryRepository<StudentAttendance> queryRepository,
            IBaseSharedRepository<StudentAttendance> sharedRepository) 
                : base(queryRepository, sharedRepository)
        {
            this.queryRepository = queryRepository;
            this.sharedRepository = sharedRepository;
        }
    }
}
