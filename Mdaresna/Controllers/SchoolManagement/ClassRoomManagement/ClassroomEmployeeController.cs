using Mdaresna.Doamin.Models.SchoolManagement.ClassRoomManagement;
using Mdaresna.DTOs.Common;
using Mdaresna.DTOs.SchoolManagementDTO.ClassRoomManagementDTO;
using Mdaresna.Infrastructure.Services.SchoolManagement.ClassRoomManagement.Command;
using Mdaresna.Infrastructure.Services.SchoolManagement.ClassRoomManagement.Query;
using Mdaresna.Repository.IServices.SchoolManagement.ClassRoomManagement.Command;
using Mdaresna.Repository.IServices.SchoolManagement.ClassRoomManagement.Query;
using Microsoft.AspNetCore.Mvc;

namespace Mdaresna.Controllers.SchoolManagement.ClassRoomManagement
{
    [Route("ClassroomEmployee")]
    public class ClassroomEmployeeController : Controller
    {
        private readonly IClassroomEmployeeQueryService classroomEmployeeQueryService;
        private readonly IClassroomEmployeeCommandService classroomEmployeeCommandService;

        public ClassroomEmployeeController(IClassroomEmployeeQueryService classroomEmployeeQueryService,
                                           IClassroomEmployeeCommandService classroomEmployeeCommandService)
        {
            this.classroomEmployeeQueryService = classroomEmployeeQueryService;
            this.classroomEmployeeCommandService = classroomEmployeeCommandService;
        }

        [HttpPost("GetClassroomEmployee")]
        public async Task<IActionResult> GetClassroomEmployee([FromBody] ClassroomIdEmployeeIdDTO dto)
        {
            try
            {
                var row = await classroomEmployeeQueryService.GetClassroomEmployeeAsync(dto.EmployeeId, dto.ClassRoomId);


                return Ok(row);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("GetClassroomsEmployees")]
        public async Task<IActionResult> GetClassroomsEmployees([FromBody] GetClassroomsEmployeesDTO dto)
        {
            try
            {
                var rows = await classroomEmployeeQueryService.GetClassroomsEmployeesAsync(dto.EmployeeId, dto.ClassRoomId, dto.SchoolId);


                return Ok(rows);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("GetEmployeeData")]
        public async Task<IActionResult> GetEmployeeData([FromBody] EmployeeIdDTO dto)
        {
            try
            {
                var rows = await classroomEmployeeQueryService.GetEmployeeDataAsync(dto.EmployeeId);


                return Ok(rows);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("CreateClassroomEmployee")]
        public async Task<IActionResult> CreateClassroomEmployee([FromBody] CreateClassroomEmployeeDTO dto)
        {
            try
            {
                var rowExist = await classroomEmployeeQueryService.IsExistAsync(dto.EmployeeId, dto.ClassRoomId);

                if (rowExist)
                    return BadRequest("Employee already assigned to this class");

                var row = new ClassroomEmployee
                {
                    EmployeeId = dto.EmployeeId,
                    ClassRoomId = dto.ClassRoomId,
                };

                var added = classroomEmployeeCommandService.Create(row);

                if (added)
                    return Ok(await classroomEmployeeQueryService.GetClassroomEmployeeAsync(row.EmployeeId, row.ClassRoomId));


                return BadRequest("Error in assining employee to classroom");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("DeleteClassroomEmployee")]
        public async Task<IActionResult> DeleteClassroomEmployee([FromBody] ClassroomIdEmployeeIdDTO dto)
        {
            try
            {

                var row = await classroomEmployeeQueryService.GetByIdAsync(dto.EmployeeId, dto.ClassRoomId);

                if (row == null)
                    return BadRequest("Can't fiend row to delete");

                var deleted = await classroomEmployeeCommandService.DeleteAsync(row);

                if (deleted)
                    return Ok("class room employee deleted");


                return BadRequest("Error in deleting row");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

    }

}
