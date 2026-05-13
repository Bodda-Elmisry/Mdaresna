using Mdaresna.Doamin.DTOs.StudentManagement;
using Mdaresna.Doamin.Models.SchoolManagement.StudentManagement;
using Mdaresna.Infrastructure.Services.Base;
using Mdaresna.Repository.IRepositories.Base;
using Mdaresna.Repository.IRepositories.SchoolManagement.StudentManagement.Query;
using Mdaresna.Repository.IServices.SchoolManagement.StudentManagement.Query;

namespace Mdaresna.Infrastructure.Services.SchoolManagement.StudentManagement.Query
{
    public class StudentAbsencePermitQueryService : BaseQueryService<StudentAbsencePermit>, IStudentAbsencePermitQueryService
    {
        private readonly IStudentAbsencePermitQueryRepository studentAbsencePermitQueryRepository;

        public StudentAbsencePermitQueryService(
            IBaseQueryRepository<StudentAbsencePermit> queryRepository,
            IBaseSharedRepository<StudentAbsencePermit> sharedRepository,
            IStudentAbsencePermitQueryRepository studentAbsencePermitQueryRepository)
            : base(queryRepository, sharedRepository)
        {
            this.studentAbsencePermitQueryRepository = studentAbsencePermitQueryRepository;
        }

        public async Task<StudentAbsencePermit?> GetActivePermitAsync(Guid studentId, DateTime date)
        {
            return await studentAbsencePermitQueryRepository.GetActivePermitAsync(studentId, date);
        }

        public async Task<IEnumerable<StudentAbsencePermitResultDTO>> GetStudentAbsencePermitsAsync(
            Guid? studentId,
            Guid? parentId,
            int pageNumber)
        {
            return await studentAbsencePermitQueryRepository.GetStudentAbsencePermitsAsync(
                studentId,
                parentId,
                pageNumber);
        }
    }
}
