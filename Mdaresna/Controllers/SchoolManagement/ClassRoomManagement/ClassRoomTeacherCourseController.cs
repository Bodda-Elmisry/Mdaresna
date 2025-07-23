using Mdaresna.Doamin.DTOs.ClassRoomManagement;
using Mdaresna.Doamin.Enums;
using Mdaresna.Doamin.Models.SchoolManagement.ClassRoomManagement;
using Mdaresna.Doamin.Models.SchoolManagement.SchoolManagement;
using Mdaresna.DTOs.Common;
using Mdaresna.DTOs.SchoolManagementDTO.ClassRoomManagementDTO;
using Mdaresna.Infrastructure.Services.SchoolManagement.SchoolManagement.Query;
using Mdaresna.Infrastructure.Services.UserManagement.Query;
using Mdaresna.Repository.IFactories;
using Mdaresna.Repository.IServices.SchoolManagement.ClassRoomManagement.Command;
using Mdaresna.Repository.IServices.SchoolManagement.ClassRoomManagement.Query;
using Mdaresna.Repository.IServices.SchoolManagement.SchoolManagement.Query;
using Mdaresna.Repository.IServices.UserManagement.Query;
using Microsoft.AspNetCore.Mvc;

namespace Mdaresna.Controllers.SchoolManagement.ClassRoomManagement
{
    [Route("ClassRoomTeacherCourse")]
    public class ClassRoomTeacherCourseController : Controller
    {
        private readonly IClassRoomTeacherCourseQueryService classRoomTeacherCourseQueryService;
        private readonly IClassRoomTeacherCourseCommandService classRoomTeacherCourseCommandService;
        private readonly IClassRoomQueryService classroomQueryService;
        private readonly ISchoolQueryService schoolQueryService;
        private readonly INotificationFactory notificationFactory;
        private readonly IUserDeviceQueryService userDeviceQueryService;
        private readonly IClassroomTransactionsFactory classroomTransactionsFactory;

        public ClassRoomTeacherCourseController(IClassRoomTeacherCourseQueryService classRoomTeacherCourseQueryService,
                                                IClassRoomTeacherCourseCommandService classRoomTeacherCourseCommandService,
                                                IClassRoomQueryService classroomQueryService,
                                                ISchoolQueryService schoolQueryService,
                                                INotificationFactory notificationFactory,
                                                IUserDeviceQueryService userDeviceQueryService)
        {
            this.classRoomTeacherCourseQueryService = classRoomTeacherCourseQueryService;
            this.classRoomTeacherCourseCommandService = classRoomTeacherCourseCommandService;
            this.classroomQueryService = classroomQueryService;
            this.schoolQueryService = schoolQueryService;
            this.notificationFactory = notificationFactory;
            this.userDeviceQueryService = userDeviceQueryService;
        }

        [HttpPost("GetInitialData")]
        public async Task<IActionResult> GetInitialData([FromBody] SchoolIdDTO schoolIdDTO)
        {
            try
            {
                var result = await classRoomTeacherCourseQueryService.GetInitialDataAsync(schoolIdDTO.SchoolId);
                return Ok(result);
            }
            catch(Exception ex) 
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("GetTracherData")]
        public async Task<IActionResult> GetTracherData([FromBody] TeacherIdDTO teacherIdDTO)
        {
            try
            {
                var data = await classRoomTeacherCourseQueryService.GetTeacherDataAsync(teacherIdDTO.TeacherId);

                return Ok(data);
            }
            catch(Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("GetClassRoomIeacherCourse")]
        public async Task<IActionResult> GetClassRoomIeacherCourse([FromBody] ClassRoomIdTeacherIdCourseIdDto dto)
        {
            try
            {
                var row = await classRoomTeacherCourseQueryService.GetClassRoomIeacherCourseAsync(dto.TeacherId,dto.ClassRoomId, dto.CourseId);


                return Ok(row);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("GetTeacherClassroomsCoursesList")]
        public async Task<IActionResult> GetTeacherClassroomsCourses([FromBody] GetTeacherClassroomsCoursesDTO dto)
        {
            try
            {
                var row = await classRoomTeacherCourseQueryService.GetTeacherClassroomsCoursesAsync(dto.TeacherId, dto.SchoolId, dto.ClassroomId, dto.CourseId);


                return Ok(row);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("GetClassRoomIeacherCoursesList")]
        public async Task<IActionResult> GetClassRoomIeacherCoursesList([FromBody] ClassroomIdT6eacherIdDTO dto)
        {
            try
            {
                var rows = await classRoomTeacherCourseQueryService.GetClassRoomIeacherCoursesAsync(dto.TeacherId, dto.ClassRoomId);


                return Ok(rows);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("CreateClassRoomTeacherCourse")]
        public async Task<IActionResult> CreateClassRoomTeacherCourse([FromBody] CreateClassRoomTeacherCourseDTO dto)
        {
            try
            {
                var rowExist = await classRoomTeacherCourseQueryService.IsExistAsync(dto.TeacherId, dto.ClassRoomId, dto.CourseId);

                if (rowExist)
                    return BadRequest("Teacher already assigned to this course in this class");

                var row = new ClassRoomTeacherCourse
                {
                    TeacherId = dto.TeacherId,
                    CourseId = dto.CourseId,
                    ClassRoomId = dto.ClassRoomId,
                };
                var added = classRoomTeacherCourseCommandService.Create(row);
                if (added)
                {

                    var notificationProvider = notificationFactory.GetProvider(NotificationProvidersEnum.Mobile);
                    var devices = await userDeviceQueryService.GetByUserIdAsync(dto.TeacherId);
                    var addedRow = await classRoomTeacherCourseQueryService.GetClassRoomIeacherCourseAsync(dto.TeacherId, dto.ClassRoomId, dto.CourseId);
                    var classroom = await classroomQueryService.GetByIdAsync(dto.ClassRoomId);
                    var school = await schoolQueryService.GetByIdAsync(classroom.SchoolId);
                    if (devices.Count() > 0)
                    {
                        var tokens = devices.Select(d => d.FcmToken).ToList();
                        await notificationProvider.SendToMultiUsersAsync(tokens, "School Role", $"Classroom {addedRow.ClassRoomName} in school {school.Name} added to you|{school.Id}");
                    }

                    return Ok(await classRoomTeacherCourseQueryService.GetClassRoomIeacherCourseAsync(row.TeacherId, row.ClassRoomId, row.CourseId));
                }


                return BadRequest("Error in assining class course to teacher");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("DeleteClassRoomIeacherCourse")]
        public async Task<IActionResult> DeleteClassRoomIeacherCourse([FromBody] ClassRoomIdTeacherIdCourseIdDto dto)
        {
            try
            {

                var row = await classRoomTeacherCourseQueryService.GetByIdAsync(dto.TeacherId, dto.ClassRoomId, dto.CourseId);

                if(row == null)
                    return BadRequest("Can't fiend row to delete");

                var deleted = await classRoomTeacherCourseCommandService.DeleteAsync(row);

                if (deleted)
                {
                    var notificationProvider = notificationFactory.GetProvider(NotificationProvidersEnum.Mobile);
                    var devices = await userDeviceQueryService.GetByUserIdAsync(dto.TeacherId);
                    var classroom = await classroomQueryService.GetByIdAsync(dto.ClassRoomId);
                    var school = await schoolQueryService.GetByIdAsync(classroom.SchoolId);
                    if (devices.Count() > 0)
                    {
                        var tokens = devices.Select(d => d.FcmToken).ToList();
                        await notificationProvider.SendToMultiUsersAsync(tokens, "School Role", $"Classroom {classroom.Name} in school {school.Name} removed from you|{school.Id}");
                    }

                    return Ok("class room course teacher deleted");
                }


                return BadRequest("Error in deleting row");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }














    }
}
