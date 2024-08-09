using Mdaresna.Doamin.DTOs.ClassRoomManagement;
using Mdaresna.Doamin.Models.SchoolManagement.ClassRoomManagement;
using Mdaresna.DTOs.Common;
using Mdaresna.DTOs.SchoolManagementDTO.ClassRoomManagementDTO;
using Mdaresna.Repository.IServices.SchoolManagement.ClassRoomManagement.Command;
using Mdaresna.Repository.IServices.SchoolManagement.ClassRoomManagement.Query;
using Microsoft.AspNetCore.Mvc;

namespace Mdaresna.Controllers.SchoolManagement.ClassRoomManagement
{
    [Route("ClassRoomTeacherCourse")]
    public class ClassRoomTeacherCourseController : Controller
    {
        private readonly IClassRoomTeacherCourseQueryService classRoomTeacherCourseQueryService;
        private readonly IClassRoomTeacherCourseCommandService classRoomTeacherCourseCommandService;

        public ClassRoomTeacherCourseController(IClassRoomTeacherCourseQueryService classRoomTeacherCourseQueryService,
                                                IClassRoomTeacherCourseCommandService classRoomTeacherCourseCommandService)
        {
            this.classRoomTeacherCourseQueryService = classRoomTeacherCourseQueryService;
            this.classRoomTeacherCourseCommandService = classRoomTeacherCourseCommandService;
        }

        [HttpPost("GetInitialData")]
        public async Task<IActionResult> GetInitialData(SchoolIdDTO schoolIdDTO)
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
        public async Task<IActionResult> GetTracherData(TeacherIdDTO teacherIdDTO)
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
        public async Task<IActionResult> GetClassRoomIeacherCourse(ClassRoomIdTeacherIdCourseIdDto dto)
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

        [HttpPost("CreateClassRoomTeacherCourse")]
        public IActionResult CreateClassRoomTeacherCourse(CreateClassRoomTeacherCourseDTO dto)
        {
            try
            {

                var row = new ClassRoomTeacherCourse
                {
                    TeacherId = dto.TeacherId,
                    CourseId = dto.CourseId,
                    ClassRoomId = dto.ClassRoomId,
                };

                var added = classRoomTeacherCourseCommandService.Create(row);

                if (added)
                    return Ok(new ClassRoomTeacherCourseResultDTO
                    {
                        ClassRoomId = row.ClassRoomId,
                        ClassRoomName = row.ClassRoom.Name,
                        CourseId = row.CourseId,
                        CourseName = row.Course.Name,
                        TeacherId = row.TeacherId,
                        TeacherName = $"{row.Teacher.FirstName} {row.Teacher.MiddelName} {row.Teacher.LastName}"
                    });


                return BadRequest("Error in assining class course to teacher");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("DeleteClassRoomIeacherCourse")]
        public async Task<IActionResult> DeleteClassRoomIeacherCourse(ClassRoomIdTeacherIdCourseIdDto dto)
        {
            try
            {

                var row = await classRoomTeacherCourseQueryService.GetByIdAsync(dto.TeacherId, dto.ClassRoomId, dto.CourseId);

                if(row == null)
                    return BadRequest("Can't fiend row to delete");

                var deleted = await classRoomTeacherCourseCommandService.DeleteAsync(row);

                if (deleted)
                    return Ok("class room course teacher deleted");


                return BadRequest("Error in deleting row");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }














    }
}
