using Mdaresna.Doamin.DTOs.Identity;
using Mdaresna.Doamin.DTOs.SchoolManagement;
using Mdaresna.Doamin.Models.Identity;
using Mdaresna.Doamin.Models.SchoolManagement.SchoolManagement;
using Mdaresna.DTOs.Common;
using Mdaresna.DTOs.SchoolManagementDTO.SchoolManagementDTO;
using Mdaresna.Infrastructure.Services.SchoolManagement.SchoolManagement.Command;
using Mdaresna.Infrastructure.Services.SchoolManagement.SchoolManagement.Query;
using Mdaresna.Repository.IServices.IdentityManagement.Command;
using Mdaresna.Repository.IServices.IdentityManagement.Query;
using Mdaresna.Repository.IServices.SchoolManagement.SchoolManagement.Command;
using Mdaresna.Repository.IServices.SchoolManagement.SchoolManagement.Query;
using Microsoft.AspNetCore.Mvc;
using System.Data;

namespace Mdaresna.Controllers.SchoolManagement.SchoolManagement
{
    [Route("SchoolEmployee")]
    public class SchoolEmployeeController : Controller
    {
        private readonly ISchoolEmployeeCommandService schoolEmployeeCommandService;
        private readonly ISchoolEmployeeQueryService schoolEmployeeQueryService;
        private readonly IUserRoleCommandService userRoleCommandService;
        private readonly IUserRoleQueryService userRoleQueryService;
        private readonly IRoleQueryService roleQueryService;

        public SchoolEmployeeController(ISchoolEmployeeCommandService schoolEmployeeCommandService,
                                        ISchoolEmployeeQueryService schoolEmployeeQueryService,
                                        IUserRoleCommandService userRoleCommandService,
                                        IUserRoleQueryService userRoleQueryService,
                                        IRoleQueryService roleQueryService)
        {
            this.schoolEmployeeCommandService = schoolEmployeeCommandService;
            this.schoolEmployeeQueryService = schoolEmployeeQueryService;
            this.userRoleCommandService = userRoleCommandService;
            this.userRoleQueryService = userRoleQueryService;
            this.roleQueryService = roleQueryService;
        }

        [HttpPost("AddSchoolEmployee")]
        public async Task<IActionResult> AddSchoolEmployee([FromBody] SchoolIdEmployeeIdDTO dto)
        {
            try
            {
                var teacher = await schoolEmployeeQueryService.GetSchoolEmployeeByIdAsync(dto.SchoolId, dto.EmployeeId);

                if (teacher != null)
                    return Conflict("Employee already assignd to school");

                var sEmployee= new SchoolEmployee
                {
                    SchoolId = dto.SchoolId,
                    EmployeeId = dto.EmployeeId
                };
                var added = schoolEmployeeCommandService.Create(sEmployee);
                if (added)
                {

                    //var userrole = new UserRole
                    //{
                    //    RoleId = dto.RoleId,
                    //    UserId = dto.EmployeeId,
                    //    SchoolId = dto.SchoolId
                    //};
                    //var teacherRoleAssignd = await userRoleQueryService.CheckRoleExist(userrole);
                    //if (!teacherRoleAssignd)
                    //{

                    //    var roleadded = userRoleCommandService.Create(userrole);
                    //    if (!roleadded)
                    //        return BadRequest("Error in adding employee role");
                    //}

                    ////List<UserRoleDTO> roleIds = new List<UserRoleDTO>();
                    ////foreach(var role in dto.RoleIds)
                    ////{
                    ////    var userrole = new UserRole
                    ////    {
                    ////        RoleId = role,
                    ////        UserId = dto.EmployeeId,
                    ////        SchoolId = dto.SchoolId
                    ////    };
                    ////    var teacherRoleAssignd = await userRoleQueryService.CheckRoleExist(userrole);
                    ////    if (!teacherRoleAssignd)
                    ////    {
                    ////        var userRoleDTO = new UserRoleDTO
                    ////        {
                    ////            RoleId = role,
                    ////            UserId = dto.EmployeeId,
                    ////            SchoolId = dto.SchoolId
                    ////        };
                    ////        roleIds.Add(userRoleDTO);
                    ////        //var roleadded = userRoleCommandService.Create(userrole);
                    ////        //if (!roleadded)
                    ////        //    return BadRequest("Error in adding employee role");
                    ////    }
                    ////}

                    ////if (roleIds.Count() > 0)
                    ////{
                    ////    var roleadded = await userRoleCommandService.Create(roleIds);
                    ////}



                    return Ok(dto);
                }
                return BadRequest("Can't add employee to school");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("UpdateSchoolEmployee")]
        public async Task<IActionResult> UpdateSchoolEmployee([FromBody] SchoolIdEmployeeIdDTO dto)
        {
            try
            {
                var sEmployee= await schoolEmployeeQueryService.GetSchoolEmployeeByIdAsync(dto.SchoolId, dto.EmployeeId);

                if (sEmployee == null)
                    return BadRequest("Can't update employee");



                var updated = schoolEmployeeCommandService.Update(sEmployee);

                if (updated)
                    return Ok(sEmployee);

                return BadRequest("Error in update school employee");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("RemoveEmployeeFromSchool")]
        public async Task<IActionResult> RemoveSchoolEmployee([FromBody] SchoolIdEmployeeIdDTO dto)
        {
            try
            {
                var sEmployee= new SchoolEmployee
                {
                    SchoolId = dto.SchoolId,
                    EmployeeId = dto.EmployeeId
                };

                var deleted = await schoolEmployeeCommandService.DeleteAsync(sEmployee);
                if (deleted)
                    return Ok("Employee removed from school");

                return BadRequest("Error in removing");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("GetSchoolEmployees")]
        public async Task<IActionResult> GetSchoolEmployees([FromBody] GetSchoolEmployeesDTO dto)
        {
            try
            {

                var employees = await schoolEmployeeQueryService.GetSchoolEmployeesAsync(dto.SchoolId, dto.EmployeeName, dto.EmployeePhoneNumber, dto.EmployeeEmail);
                return Ok(employees);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("GetEmployeeSchools")]
        public async Task<IActionResult> GetEmployeeSchools([FromBody] EmployeeIdDTO employeeIdDTO)
        {
            try
            {
                var schools = await schoolEmployeeQueryService.GetEmployeeSchoolsAsync(employeeIdDTO.EmployeeId);
                return Ok(schools);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }







    }
}
