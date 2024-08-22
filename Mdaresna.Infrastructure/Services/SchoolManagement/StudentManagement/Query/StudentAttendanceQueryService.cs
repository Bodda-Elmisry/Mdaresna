using Mdaresna.Doamin.DTOs.StudentManagement;
using Mdaresna.Doamin.Models.SchoolManagement.StudentManagement;
using Mdaresna.Infrastructure.Services.Base;
using Mdaresna.Repository.IRepositories.Base;
using Mdaresna.Repository.IRepositories.SchoolManagement.StudentManagement.Query;
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
        private readonly IStudentAttendanceQueryRepository studentAttendanceQueryRepository;

        public StudentAttendanceQueryService(IBaseQueryRepository<StudentAttendance> queryRepository,
            IBaseSharedRepository<StudentAttendance> sharedRepository,
            IStudentAttendanceQueryRepository studentAttendanceQueryRepository) 
                : base(queryRepository, sharedRepository)
        {
            this.studentAttendanceQueryRepository = studentAttendanceQueryRepository;
        }

        public async Task<IEnumerable<StudentAttendanceResultDTO>> GetStudentsAttendancesAsync(Guid? studentId, Guid? classRoomId, int pageNumber)
        {
            return await studentAttendanceQueryRepository.GetStudentsAttendancesAsync(studentId, classRoomId, pageNumber);
        }
    }
}
