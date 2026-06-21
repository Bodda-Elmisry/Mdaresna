using Mdaresna.Doamin.Enums;
using Mdaresna.Doamin.Models.SchoolManagement.ClassRoomManagement;
using Mdaresna.DTOs.Common;
using Mdaresna.Doamin.DTOs.ClassRoomManagement;
using Mdaresna.DTOs.SchoolManagementDTO.ClassRoomManagementDTO;
using Mdaresna.Repository.IFactories;
using Mdaresna.Repository.IServices.SchoolManagement.ClassRoomManagement.Command;
using Mdaresna.Repository.IServices.SchoolManagement.ClassRoomManagement.Query;
using Mdaresna.Repository.IServices.SchoolManagement.StudentManagement.Query;
using Mdaresna.Repository.IServices.UserManagement.Query;
using Mdaresna.Repository.IServices.SchoolManagement.SchoolManagement.Query;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace Mdaresna.Controllers.SchoolManagement.ClassRoomManagement
{
    [Route("ClassRoom")]
    [Authorize]
    public class ClassRoomController : Controller
    {
        private readonly IClassRoomQueryService classRoomQueryService;
        private readonly IClassRoomCommandService classRoomCommandService;
        private readonly IStudentQueryService studentQueryService;
        private readonly INotificationFactory notificationFactory;
        private readonly IUserDeviceQueryService userDeviceQueryService;
        private readonly ISchoolAccessValidator schoolAccessValidator;

        public ClassRoomController(IClassRoomQueryService classRoomQueryService,
                                   IClassRoomCommandService classRoomCommandService,
                                   IStudentQueryService studentQueryService,
                                           INotificationFactory notificationFactory,
                                           IUserDeviceQueryService userDeviceQueryService,
                                           ISchoolAccessValidator schoolAccessValidator)
        {
            this.classRoomQueryService = classRoomQueryService;
            this.classRoomCommandService = classRoomCommandService;
            this.studentQueryService = studentQueryService;
            this.notificationFactory = notificationFactory;
            this.userDeviceQueryService = userDeviceQueryService;
            this.schoolAccessValidator = schoolAccessValidator;
        }

        private Guid CurrentUserId
        {
            get
            {
                var userIdClaim = User.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub) 
                                  ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
                return userIdClaim != null ? Guid.Parse(userIdClaim.Value) : Guid.Empty;
            }
        }

        [HttpPost("GetInitialData")]
        public async Task<IActionResult> GetInitialData([FromBody] SchoolIdDTO schoolId)
        {
            try
            {
                if (!await schoolAccessValidator.CanAccessSchoolAsync(CurrentUserId, schoolId.SchoolId))
                {
                    return Forbid();
                }
                var result = await classRoomQueryService.getInitialValue(schoolId.SchoolId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("GetClassById")]
        public async Task<IActionResult> GetClassRoomById([FromBody] ClassRoomIdDTO classRoomIdDTO)
        {
            try
            {
                if (!await schoolAccessValidator.CanAccessClassRoomAsync(CurrentUserId, classRoomIdDTO.ClassRoomId))
                {
                    return Forbid();
                }
                var room = await classRoomQueryService.GetByIdAsync(classRoomIdDTO.ClassRoomId);
                return Ok(room);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("GetSchoolClasses")]
        public async Task<IActionResult> GetSchoolClasses([FromBody] GetSchoolClassesFilteredDTO filterDto)
        {
            try
            {
                if (!await schoolAccessValidator.CanAccessSchoolAsync(CurrentUserId, filterDto.SchoolId))
                {
                    return Forbid();
                }
                var result = await classRoomQueryService.GetBySchoolIdFilteredAsync(filterDto);
                
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
                if (!await schoolAccessValidator.CanAccessSchoolAsync(CurrentUserId, supervisorClassesDTO.SchoolId))
                {
                    return Forbid();
                }
                var result = await classRoomQueryService.GetBySchoolIdAndSupervisorIdAsync(supervisorClassesDTO.SchoolId,
                                                                                     supervisorClassesDTO.SupervisorId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("GetUserClasses")]
        public async Task<IActionResult> GetUserClasses([FromBody] GetSupervisorClassesDTO supervisorClassesDTO)
        {
            try
            {
                if (!await schoolAccessValidator.CanAccessSchoolAsync(CurrentUserId, supervisorClassesDTO.SchoolId))
                {
                    return Forbid();
                }
                var result = await classRoomQueryService.GetBySchoolIdAndUserIdAsync(supervisorClassesDTO.SchoolId,
                                                                                     supervisorClassesDTO.SupervisorId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("DeactivateClass")]
        public async Task<IActionResult> DeactivateClassRoom([FromBody] ClassRoomIdDTO RoomIddto)
        {
            try
            {
                if (!await schoolAccessValidator.CanAccessClassRoomAsync(CurrentUserId, RoomIddto.ClassRoomId))
                {
                    return Forbid();
                }
                var classRoom = await classRoomQueryService.GetByIdAsync(RoomIddto.ClassRoomId);
                classRoom.Active = false;
                var Updated = classRoomCommandService.Update(classRoom);
                if (Updated)
                {
                    var notificationProvider = notificationFactory.GetProvider(NotificationProvidersEnum.Mobile);
                    var devices = await userDeviceQueryService.GetByUserIdAsync(classRoom.School.SchoolAdminId);
                    if (devices.Count() > 0)
                    {
                        var tokens = devices.Select(d => d.FcmToken).ToList();
                        await notificationProvider.SendToMultiUsersAsync(tokens, "Classroom Activation", $"Classroom {classRoom.Name} deactivated");
                    }
                    return Ok(classRoom);
                }
                else
                    return BadRequest("Error In Deactivate Process...");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("ActivateClass")]
        public async Task<IActionResult> ActivateClassRoom([FromBody] ClassRoomIdDTO RoomIddto)
        {
            try
            {
                if (!await schoolAccessValidator.CanAccessClassRoomAsync(CurrentUserId, RoomIddto.ClassRoomId))
                {
                    return Forbid();
                }
                var classRoom = await classRoomQueryService.GetByIdAsync(RoomIddto.ClassRoomId);
                classRoom.Active = true;
                var Updated = classRoomCommandService.Update(classRoom);
                if (Updated)
                {
                    var notificationProvider = notificationFactory.GetProvider(NotificationProvidersEnum.Mobile);
                    var devices = await userDeviceQueryService.GetByUserIdAsync(classRoom.School.SchoolAdminId);
                    if (devices.Count() > 0)
                    {
                        var tokens = devices.Select(d => d.FcmToken).ToList();
                        await notificationProvider.SendToMultiUsersAsync(tokens, "Classroom Activation", $"Classroom {classRoom.Name} activated");
                    }
                    return Ok(classRoom);
                }
                else
                    return BadRequest("Error In Activate Process...");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("UpdateClass")]
        public async Task<IActionResult> UpdateClassRoom([FromBody] UpdateClassRoomDTO classRoomDTO)
        {
            try
            {
                var classRoom = await classRoomQueryService.GetByIdAsync(classRoomDTO.Id);
                if (classRoom == null)
                    return BadRequest("There is no classroom to update");

                if (!await schoolAccessValidator.CanAccessSchoolAsync(CurrentUserId, classRoom.SchoolId) || 
                    !await schoolAccessValidator.CanAccessSchoolAsync(CurrentUserId, classRoomDTO.SchoolId))
                {
                    return Forbid();
                }
                classRoom.Name = classRoomDTO.Name;
                classRoom.SchoolId = classRoomDTO.SchoolId;
                classRoom.GradeId = classRoomDTO.GradeId;
                classRoom.LanguageId = classRoomDTO.LanguageId;
                classRoom.SupervisorId = classRoomDTO.SupervisorId;
                classRoom.maxOfStudents = classRoomDTO.maxOfStudents;
                classRoom.Active = classRoomDTO.Active;
                classRoom.WCSUrl = classRoomDTO.WCSUrl;
                classRoom.Gender = (ClassRoomGenderEnum)classRoomDTO.Gender;

                var updated = classRoomCommandService.Update(classRoom);
                if (updated)
                    return Ok(classRoom);
                else
                    return BadRequest("Error In Update Class");

            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("AddClass")]
        public async Task<IActionResult> CreateNewCalssRoom([FromBody] CreateClassRoomDTO classRoomDTO)
        {
            try
            {
                if (!await schoolAccessValidator.CanAccessSchoolAsync(CurrentUserId, classRoomDTO.SchoolId))
                {
                    return Forbid();
                }
                var classRoom = new ClassRoom
                {
                    Name = classRoomDTO.Name,
                    SchoolId = classRoomDTO.SchoolId,
                    GradeId = classRoomDTO.GradeId,
                    LanguageId = classRoomDTO.LanguageId,
                    SupervisorId = classRoomDTO.SupervisorId,
                    maxOfStudents = classRoomDTO.maxOfStudents,
                    Active = classRoomDTO.Active,
                    WCSUrl = classRoomDTO.WCSUrl,
                    Gender = (ClassRoomGenderEnum)classRoomDTO.Gender
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

        [HttpPost("SoftDeleteClassroom")]
        public async Task<IActionResult> SoftDeleteClassroom([FromBody] ClassRoomIdDTO dto)
        {
            try
            {
                if (!await schoolAccessValidator.CanAccessClassRoomAsync(CurrentUserId, dto.ClassRoomId))
                {
                    return Forbid();
                }
                var classroom = await classRoomQueryService.GetByIdAsync(dto.ClassRoomId);

                if (classroom == null)
                    return BadRequest("There is no classroom to delete");

                var classroomStudents = await studentQueryService.GetStudentsBySchoolIdAndClassRoomIdAsync(classroom.SchoolId, dto.ClassRoomId);

                if (classroomStudents != null && classroomStudents.Count() > 0)
                    return BadRequest("Please move students from classroom first");

                classroom.Deleted = true;

                var updated = classRoomCommandService.Update(classroom);
                return updated ? Ok("classroom deleted") : BadRequest("Error in deleting classroom");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

    }
}
