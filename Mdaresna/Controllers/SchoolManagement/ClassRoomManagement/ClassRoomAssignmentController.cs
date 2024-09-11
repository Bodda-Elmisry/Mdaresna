using Mdaresna.Doamin.Models.SchoolManagement.ClassRoomManagement;
using Mdaresna.DTOs.Common;
using Mdaresna.DTOs.SchoolManagementDTO.ClassRoomManagementDTO;
using Mdaresna.Repository.IServices.SchoolManagement.ClassRoomManagement.Command;
using Mdaresna.Repository.IServices.SchoolManagement.ClassRoomManagement.Query;
using Microsoft.AspNetCore.Mvc;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Mdaresna.Controllers.SchoolManagement.ClassRoomManagement
{
    [Route("ClassRoomAssignment")]
    public class ClassRoomAssignmentController : Controller
    {
        private readonly IClassRoomAssignmentCommandService classRoomAssignmentCommandService;
        private readonly IClassRoomAssignmentQueryService classRoomAssignmentQueryService;

        public ClassRoomAssignmentController(IClassRoomAssignmentCommandService classRoomAssignmentCommandService,
                                             IClassRoomAssignmentQueryService classRoomAssignmentQueryService)
        {
            this.classRoomAssignmentCommandService = classRoomAssignmentCommandService;
            this.classRoomAssignmentQueryService = classRoomAssignmentQueryService;
        }

        [HttpPost("GetClassroomAssignmentsList")]
        public async Task<IActionResult> GetClassroomAssignmentsList([FromBody] GetClassRoomAssignmentListDTO dTO)
        {
            try
            {
                var items = await classRoomAssignmentQueryService.GetClassRoomAssignmentsList(dTO.classRoomId,
                                                                                              dTO.SupervisorId,
                                                                                              dTO.courseId,
                                                                                              dTO.details,
                                                                                              dTO.rate,
                                                                                              dTO.fromdate,
                                                                                              dTO.todate,
                                                                                              dTO.pageNumber);
                return Ok(items);
            }
            catch(Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("GetClassroomAssignmentbyid")]
        public async Task<IActionResult> GetClassroomAssignmentById([FromBody] AssignmentIdDTO dTO)
        {
            try
            {
                var item = await classRoomAssignmentQueryService.GetClassRoomAssignmentById(dTO.AssignmentId);

                return Ok(item);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("CreateClassRoomAssignment")]
        public async Task<IActionResult> CreateClassRoomAssignment([FromBody] CreateClassRoomAssignmentDTO dTO)
        {
            try
            {
                var assingment = new ClassRoomAssignment
                {
                    AssignmentDate = dTO.AssignmentDate,
                    ClassRoomId = dTO.ClassRoomId,
                    CourseId = dTO.CourseId,
                    Details = dTO.Details,
                    Rate = dTO.Rate,
                    SupervisorId = dTO.SupervisorId,
                    WeekDay = dTO.WeekDay
                };

                var added = await classRoomAssignmentCommandService.Create(assingment, dTO.StudentIds);

                if(added)
                    return Ok(await classRoomAssignmentQueryService.GetClassRoomAssignmentById(assingment.Id));

                return BadRequest("Error in creating assignment");


            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("UpdateClassRoomAssignment")]
        public async Task<IActionResult> UpdateClassRoomAssignment([FromBody] UpdateClassRoomAssignmentDTO dTO)
        {
            try
            {
                var assignemt = await classRoomAssignmentQueryService.GetByIdAsync(dTO.Id);

                if (assignemt == null)
                    return BadRequest("Can't fiend assignment to update");

                assignemt.AssignmentDate = dTO.AssignmentDate;
                assignemt.ClassRoomId = dTO.ClassRoomId;
                assignemt.CourseId = dTO.CourseId;
                assignemt.Details = dTO.Details;
                assignemt.Rate = dTO.Rate;
                assignemt.SupervisorId = dTO.SupervisorId;
                assignemt.WeekDay = dTO.WeekDay;

                var updated = classRoomAssignmentCommandService.Update(assignemt);

                if (updated)
                    return Ok(await classRoomAssignmentQueryService.GetClassRoomAssignmentById(dTO.Id));

                return BadRequest("Error in updating assignment");

            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }













    }
}
