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

        [HttpPost("CreateClassRoomAssignement")]
        public async Task<IActionResult> CreateClassRoomAssignement([FromBody] CreateClassRoomAssignmentDTO dTO)
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

                return BadRequest("Error in creating assignement");


            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("UpdateClassRoomAssignement")]
        public async Task<IActionResult> UpdateClassRoomAssignement([FromBody] UpdateClassRoomAssignmentDTO dTO)
        {
            try
            {
                var assignemt = await classRoomAssignmentQueryService.GetByIdAsync(dTO.Id);

                if (assignemt == null)
                    return BadRequest("Can't fiend assignement to update");

                dTO.AssignmentDate = dTO.AssignmentDate;
                dTO.ClassRoomId = dTO.ClassRoomId;
                dTO.CourseId = dTO.CourseId;
                dTO.Details = dTO.Details;
                dTO.Rate = dTO.Rate;
                dTO.SupervisorId = dTO.SupervisorId;
                dTO.WeekDay = dTO.WeekDay;

                var updated = classRoomAssignmentCommandService.Update(assignemt);

                if (updated)
                    return Ok(await classRoomAssignmentQueryService.GetClassRoomAssignmentById(dTO.Id));

                return BadRequest("Error in updating assignement");

            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }













    }
}
