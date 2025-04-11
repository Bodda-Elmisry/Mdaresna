using Mdaresna.Doamin.DTOs.SchoolManagement;
using Mdaresna.Doamin.Helpers;
using Mdaresna.Doamin.Models.SchoolManagement.SchoolManagement;
using Mdaresna.Infrastructure.Data;
using Mdaresna.Infrastructure.Repositories.Base;
using Mdaresna.Repository.IRepositories.SchoolManagement.SchoolManagement.Query;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mdaresna.Infrastructure.Repositories.SchoolManagement.SchoolManagement.Query
{
    internal class SchoolEmployeeQueryRepository : BaseQueryRepository<SchoolEmployee>, ISchoolEmployeeQueryRepository
    {
        private readonly AppDbContext context;

        public SchoolEmployeeQueryRepository(AppDbContext context) : base(context)
        {
            this.context = context;
        }

        public async Task<bool> IsExist(Guid schoolId, Guid employeeId)
        {
            return await context.SchoolEmployees.AnyAsync(s => s.SchoolId == schoolId && s.EmployeeId == employeeId);
        }

        public async Task<IEnumerable<TeacherSchoolResultDTO>> GetEmployeeSchoolsAsync(Guid employeeId)
        {
            var schools = await context.SchoolEmployees
                .Where(s => s.EmployeeId == employeeId)
                .Select(s => new TeacherSchoolResultDTO
                {
                    Id = s.School.Id,
                    Name = s.School.Name,
                    ImageUrl = s.School.ImageUrl
                }).ToListAsync();


            return schools;
        }

        public async Task<SchoolEmployee?> GetSchoolEmployeeByIdAsync(Guid schoolId, Guid employeeId)
        {
            return await context.SchoolEmployees.FirstOrDefaultAsync(s => s.SchoolId == schoolId && s.EmployeeId == employeeId);

        }

        public async Task<IEnumerable<EmployeeResultDTO>> GetSchoolEmployeesAsync(Guid schoolId, string employeeName, string employeePhoneNumber, string employeeEmail)
        {
            var employeeQuery = from st in context.SchoolEmployees.Where(s => s.SchoolId == schoolId)
                               select new EmployeeResultDTO
                               {
                                   Id = st.Employee.Id,
                                   UserName = st.Employee.UserName,
                                   FirstName = st.Employee.FirstName,
                                   MiddelName = st.Employee.MiddelName,
                                   LastName = st.Employee.LastName,
                                   PhoneNumber = st.Employee.PhoneNumber,
                                   BirthDay = st.Employee.BirthDay,
                                   Email = st.Employee.Email,
                                   Address = st.Employee.Address,
                                   City = st.Employee.City,
                                   Region = st.Employee.Region,
                                   Contry = st.Employee.Contry,
                                   ImageUrl = !string.IsNullOrEmpty(st.Employee.ImageUrl) ? $"{SettingsHelper.GetAppUrl()}/{st.Employee.ImageUrl.Replace("\\", "/")}" : null
                               };

            employeeQuery = !string.IsNullOrEmpty(employeeEmail) ? employeeQuery.Where(t => t.Email.Contains(employeeEmail)) : employeeQuery;
            employeeQuery = !string.IsNullOrEmpty(employeePhoneNumber) ? employeeQuery.Where(t => t.PhoneNumber.Contains(employeePhoneNumber)) : employeeQuery;
            employeeQuery = !string.IsNullOrEmpty(employeeName) ? employeeQuery.Where(t => (t.FirstName + t.MiddelName + t.LastName).Contains(employeeName.Replace(" ", ""))) : employeeQuery;



            var query = employeeQuery.ToQueryString();
            var employees = await employeeQuery.ToListAsync();
            return employees;
        }
    }
}
