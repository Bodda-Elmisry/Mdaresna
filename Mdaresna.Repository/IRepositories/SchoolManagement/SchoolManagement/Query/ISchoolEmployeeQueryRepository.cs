using Mdaresna.Doamin.DTOs.SchoolManagement;
using Mdaresna.Doamin.Models.SchoolManagement.SchoolManagement;
using Mdaresna.Repository.IRepositories.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mdaresna.Repository.IRepositories.SchoolManagement.SchoolManagement.Query
{
    public interface ISchoolEmployeeQueryRepository : IBaseQueryRepository<SchoolEmployee>
    {
        Task<bool> IsExist(Guid schoolId, Guid employeeId);
        Task<IEnumerable<TeacherSchoolResultDTO>> GetEmployeeSchoolsAsync(Guid employeeId);
        Task<IEnumerable<EmployeeResultDTO>> GetSchoolEmployeesAsync(Guid schoolId, string employeeName, string employeePhoneNumber, string employeeEmail);
        Task<SchoolEmployee?> GetSchoolEmployeeByIdAsync(Guid schoolId, Guid employeeId);
    }
}
