using Mdaresna.Doamin.Models.SchoolManagement.SchoolManagement;
using Mdaresna.Infrastructure.Services.Base;
using Mdaresna.Repository.IRepositories.Base;
using Mdaresna.Repository.IServices.SchoolManagement.SchoolManagement.Query;

using Mdaresna.Repository.IRepositories.SchoolManagement.SchoolManagement.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mdaresna.Doamin.DTOs.SchoolManagement;

namespace Mdaresna.Infrastructure.Services.SchoolManagement.SchoolManagement.Query
{
    internal class SchoolEmployeeQueryService : BaseQueryService<SchoolEmployee>, ISchoolEmployeeQueryService
    {
        private readonly ISchoolEmployeeQueryRepository schoolEmployeeQueryRepository;

        public SchoolEmployeeQueryService(IBaseQueryRepository<SchoolEmployee> queryRepository, 
                                          IBaseSharedRepository<SchoolEmployee> sharedRepository,
                                          ISchoolEmployeeQueryRepository schoolEmployeeQueryRepository) 
            : base(queryRepository, sharedRepository)
        {
            this.schoolEmployeeQueryRepository = schoolEmployeeQueryRepository;
        }

        public async Task<IEnumerable<TeacherSchoolResultDTO>> GetEmployeeSchoolsAsync(Guid employeeId)
        {
            return await schoolEmployeeQueryRepository.GetEmployeeSchoolsAsync(employeeId);
        }

        public async Task<SchoolEmployee?> GetSchoolEmployeeByIdAsync(Guid schoolId, Guid employeeId)
        {
            return await schoolEmployeeQueryRepository.GetSchoolEmployeeByIdAsync(schoolId, employeeId);
        }

        public async Task<IEnumerable<EmployeeResultDTO>> GetSchoolEmployeesAsync(Guid schoolId, string employeeName, string employeePhoneNumber, string employeeEmail)
        {
            return await schoolEmployeeQueryRepository.GetSchoolEmployeesAsync(schoolId, employeeName, employeePhoneNumber, employeeEmail);
        }

        public async Task<bool> IsExist(Guid schoolId, Guid employeeId)
        {
            return await schoolEmployeeQueryRepository.IsExist(schoolId, employeeId);
        }
    }
}
