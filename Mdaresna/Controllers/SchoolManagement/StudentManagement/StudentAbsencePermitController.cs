using Mdaresna.Doamin.DTOs.StudentManagement;
using Mdaresna.Repository.IServices.SchoolManagement.StudentManagement.Command;
using Mdaresna.Repository.IServices.SchoolManagement.StudentManagement.Query;
using Microsoft.AspNetCore.Mvc;

namespace Mdaresna.Controllers.SchoolManagement.StudentManagement
{
    [Route("AbsencePermit")]
    public class StudentAbsencePermitController : Controller
    {
        private readonly IStudentAbsencePermitCommandService studentAbsencePermitCommandService;
        private readonly IStudentAbsencePermitQueryService studentAbsencePermitQueryService;

        public StudentAbsencePermitController(
            IStudentAbsencePermitCommandService studentAbsencePermitCommandService,
            IStudentAbsencePermitQueryService studentAbsencePermitQueryService)
        {
            this.studentAbsencePermitCommandService = studentAbsencePermitCommandService;
            this.studentAbsencePermitQueryService = studentAbsencePermitQueryService;
        }

        [HttpPost("Create")]
        public async Task<IActionResult> Create([FromBody] AddStudentAbsencePermitDTO permitDTO)
        {
            try
            {
                if (permitDTO.StudentId == Guid.Empty || permitDTO.ParentId == Guid.Empty)
                {
                    return BadRequest("Student and parent are required");
                }

                var result = await studentAbsencePermitCommandService.CreateAbsencePermitAsync(permitDTO);

                return result == "Absence Permit Created"
                    ? Ok(result)
                    : BadRequest(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("GetStudentPermits")]
        public async Task<IActionResult> GetStudentPermits([FromBody] GetStudentAbsencePermitsDTO permitsDTO)
        {
            try
            {
                if (permitsDTO.StudentId == null && permitsDTO.ParentId == null)
                {
                    return BadRequest("Can't get data without student or parent");
                }

                var data = await studentAbsencePermitQueryService.GetStudentAbsencePermitsAsync(
                    permitsDTO.StudentId,
                    permitsDTO.ParentId,
                    permitsDTO.PageNumber);

                return Ok(data);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("SoftDelete")]
        public async Task<IActionResult> SoftDelete([FromBody] StudentAbsencePermitIdDTO dto)
        {
            try
            {
                var deleted = await studentAbsencePermitCommandService.SoftDeleteAbsencePermitAsync(
                    dto.StudentAbsencePermitId,
                    dto.ParentId);

                return deleted
                    ? Ok("Absence Permit Deleted")
                    : BadRequest("Error in deleting absence permit");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
