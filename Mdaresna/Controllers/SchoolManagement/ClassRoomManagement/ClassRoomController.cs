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
    [Route("ClassRoom")]
    public class ClassRoomController : Controller
    {
        private readonly IClassRoomQueryService classRoomQueryService;
        private readonly IClassRoomCommandService classRoomCommandService;

        public ClassRoomController(IClassRoomQueryService classRoomQueryService,
                                   IClassRoomCommandService classRoomCommandService)
        {
            this.classRoomQueryService = classRoomQueryService;
            this.classRoomCommandService = classRoomCommandService;
        }

        [HttpPost("GetInitialData")]
        public async Task<IActionResult> GetInitialData([FromBody] SchoolIdDTO schoolId)
        {
            try
            {
                var result = await classRoomQueryService.getInitialValue(schoolId.SchoolId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("GetSchoolClasses")]
        public async Task<IActionResult> GetSchoolClasses([FromBody] SchoolIdDTO schoolId)
        {
            try
            {
                var result = await classRoomQueryService.GetBySchoolIdAsync(schoolId.SchoolId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("GetSupervisorClasses")]
        public async Task<IActionResult> GetSupervisorClasses([FromBody] GetSupervisorClassesDTO supervisorClassesDTO)
        {
            try
            {
                var result = classRoomQueryService.GetBySchoolIdAndSupervisorIdAsync(supervisorClassesDTO.SchoolId,
                                                                                     supervisorClassesDTO.SupervisorId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("DeactivateClass")]
        public async Task<IActionResult> DeactivateClassRoom([FromBody] Guid RoomId)
        {
            try
            {
                var classRoom = await classRoomQueryService.GetByIdAsync(RoomId);
                classRoom.Active = false;
                var Updated = classRoomCommandService.Update(classRoom);
                if (Updated)
                    return Ok(classRoom);
                else
                    return BadRequest("Error In Deactivate Process...");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("ActivateClass")]
        public async Task<IActionResult> ActivateClassRoom([FromBody] Guid RoomId)
        {
            try
            {
                var classRoom = await classRoomQueryService.GetByIdAsync(RoomId);
                classRoom.Active = true;
                var Updated = classRoomCommandService.Update(classRoom);
                if (Updated)
                    return Ok(classRoom);
                else
                    return BadRequest("Error In Activate Process...");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("UpdateClass")]
        public IActionResult UpdateClassRoom([FromBody] ClassRoom classRoom)
        {
            try
            {
                var updated = classRoomCommandService.Update(classRoom);
                if (updated)
                    return Ok(updated);
                else
                    return BadRequest("Error In Update Class");

            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("AddClass")]
        public IActionResult CreateNewCalssRoom([FromBody] CreateClassRoomDTO classRoomDTO)
        {
            try
            {
                var classRoom = new ClassRoom
                {
                    Name = classRoomDTO.Name,
                    SchoolId = classRoomDTO.SchoolId,
                    GradeId = classRoomDTO.GradeId,
                    LanguageId = classRoomDTO.LanguageId,
                    SupervisorId = classRoomDTO.SupervisorId,
                    maxOfStudents = classRoomDTO.maxOfStudents,
                    Active = classRoomDTO.Active,
                    WCSUrl = classRoomDTO.WCSUrl
                };

                var created = classRoomCommandService.Create(classRoom);
                if (created)
                    return Ok(classRoom);
                else
                    return BadRequest("Error In Adding New Class");





            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

    }
}
