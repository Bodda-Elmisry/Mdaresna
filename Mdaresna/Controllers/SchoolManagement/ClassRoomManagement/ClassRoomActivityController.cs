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
    [Route("ClassRoomActivity")]
    public class ClassRoomActivityController : Controller
    {
        private readonly IClassRoomActivityQueryService classRoomActivityQueryService;
        private readonly IClassRoomActivityCommandService classRoomActivityCommandService;

        public ClassRoomActivityController(IClassRoomActivityQueryService classRoomActivityQueryService,
                                           IClassRoomActivityCommandService classRoomActivityCommandService)
        {
            this.classRoomActivityQueryService = classRoomActivityQueryService;
            this.classRoomActivityCommandService = classRoomActivityCommandService;
        }

        [HttpPost("GetClassroomActivitysList")]
        public async Task<IActionResult> GetClassroomActivitysList([FromBody] GetClassRoomActivityDTO dTO)
        {
            try
            {
                var items = await classRoomActivityQueryService.GetClassRoomActivitiesList(dTO.classRoomId,
                                                                                              dTO.SupervisorId,
                                                                                              dTO.courseId,
                                                                                              dTO.details,
                                                                                              dTO.rate,
                                                                                              dTO.fromdate,
                                                                                              dTO.todate,
                                                                                              dTO.pageNumber);
                return Ok(items);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("GetClassroomActivitybyid")]
        public async Task<IActionResult> GetClassroomActivityById([FromBody] ActivityIdDTO dTO)
        {
            try
            {
                var item = await classRoomActivityQueryService.GetClassRoomActivityById(dTO.ActivityId);

                return Ok(item);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("CreateClassRoomActivity")]
        public async Task<IActionResult> CreateClassRoomAssignement([FromBody] CreateClassRoomActivityDTO dTO)
        {
            try
            {
                var activity = new ClassRoomActivity
                {
                    ActivityDate = dTO.ActivityDate,
                    ClassRoomId = dTO.ClassRoomId,
                    CourseId = dTO.CourseId,
                    Details = dTO.Details,
                    Rate = dTO.Rate,
                    SupervisorId = dTO.SupervisorId,
                    WeekDay = dTO.WeekDay
                };

                var added = await classRoomActivityCommandService.Create(activity, dTO.StudentIds);

                if (added)
                    return Ok(await classRoomActivityQueryService.GetClassRoomActivityById(activity.Id));

                return BadRequest("Error in creating assignement");


            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("UpdateClassRoomActivity")]
        public async Task<IActionResult> UpdateClassRoomAssignement([FromBody] UpdateClassRoomActivityDTO dTO)
        {
            try
            {
                var activity = await classRoomActivityQueryService.GetByIdAsync(dTO.Id);

                if (activity == null)
                    return BadRequest("Can't fiend activity to update");

                activity.ActivityDate = dTO.ActivityDate;
                activity.ClassRoomId = dTO.ClassRoomId;
                activity.CourseId = dTO.CourseId;
                activity.Details = dTO.Details;
                activity.Rate = dTO.Rate;
                activity.SupervisorId = dTO.SupervisorId;
                activity.WeekDay = dTO.WeekDay;

                var updated = classRoomActivityCommandService.Update(activity);

                if (updated)
                    return Ok(await classRoomActivityQueryService.GetClassRoomActivityById(dTO.Id));

                return BadRequest("Error in updating activity");

            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
