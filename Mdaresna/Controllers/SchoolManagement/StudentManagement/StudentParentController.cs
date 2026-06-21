using Mdaresna.Doamin.Enums;
using Mdaresna.Doamin.Models.SchoolManagement.StudentManagement;
using Mdaresna.DTOs.Common;
using Mdaresna.DTOs.SchoolManagementDTO.StudentManagementDTO;
using Mdaresna.Repository.IFactories;
using Mdaresna.Repository.IServices.SchoolManagement.StudentManagement.Command;
using Mdaresna.Repository.IServices.SchoolManagement.StudentManagement.Query;
using Mdaresna.Repository.IServices.UserManagement.Query;
using Mdaresna.Repository.IServices.SchoolManagement.SchoolManagement.Query;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Mdaresna.Controllers.SchoolManagement.StudentManagement
{
    [Route("StudentParent")]
    [Authorize]
    public class StudentParentController : Controller
    {
        private readonly IStudentParentQueryService studentParentQueryService;
        private readonly IStudentParentCommandService studentParentCommandService;
        private readonly INotificationFactory notificationFactory;
        private readonly IUserDeviceQueryService userDeviceQueryService;
        private readonly IStudentQueryService studentQueryService;
        private readonly ISchoolAccessValidator schoolAccessValidator;

        public StudentParentController(IStudentParentQueryService studentParentQueryService,
                                       IStudentParentCommandService studentParentCommandService,
                                           INotificationFactory notificationFactory,
                                           IUserDeviceQueryService userDeviceQueryService,
                                           IStudentQueryService studentQueryService,
                                           ISchoolAccessValidator schoolAccessValidator)
        {
            this.studentParentQueryService = studentParentQueryService;
            this.studentParentCommandService = studentParentCommandService;
            this.notificationFactory = notificationFactory;
            this.userDeviceQueryService = userDeviceQueryService;
            this.studentQueryService = studentQueryService;
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

        [HttpPost("GetParentStudents")]
        public async Task<IActionResult> GetParentStudents([FromBody] ParentIdRelationIdDTO dTO)
        {
            try
            {
                if (dTO.ParentId != CurrentUserId)
                {
                    return Forbid();
                }
                var students = await studentParentQueryService.GetParentStudentsAsync(dTO.ParentId, dTO.RelationId);

                return Ok(students);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("GetstudentParents")]
        public async Task<IActionResult> GetstudentParents([FromBody] StudentIdRelationIdDTO dTO)
        {
            try
            {
                if (!await schoolAccessValidator.CanAccessStudentAsync(CurrentUserId, dTO.StudentId))
                {
                    return Forbid();
                }
                var parents = await studentParentQueryService.GetstudentParentsAsync(dTO.StudentId, dTO.RelationId);

                return Ok(parents);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("GetStudentParent")]
        public async Task<IActionResult> GetStudentParent([FromBody] ParentIdStudentIdDTO dTO)
        {
            try
            {
                if (dTO.ParentId != CurrentUserId && !await schoolAccessValidator.CanAccessStudentAsync(CurrentUserId, dTO.StudentId))
                {
                    return Forbid();
                }
                var parentStudent = await studentParentQueryService.GetstudentParentAsync(dTO.ParentId, dTO.StudentId);

                return Ok(parentStudent);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("AddStudentParent")]
        public async Task<IActionResult> AddStudentParent([FromBody] CreateStudentParentDTO entity)
        {
            try
            {
                if (!await schoolAccessValidator.CanAccessStudentAsync(CurrentUserId, entity.StudentId))
                {
                    return Forbid();
                }
                var parentStudent = await studentParentQueryService.GetstudentParentAsync(entity.ParentId, entity.StudentId);

                if (parentStudent != null)
                    return Ok(parentStudent);

                var parentStudentN = new StudentParent
                {
                    ParentId = entity.ParentId,
                    StudentId = entity.StudentId,
                    RelationId = entity.RelationId
                };

                var added = studentParentCommandService.Create(parentStudentN);

                if (added)
                {
                    var student = await studentQueryService.GetByIdAsync(entity.StudentId);
                    if (student != null)
                    {
                        schoolAccessValidator.RemoveSchoolAccessCache(entity.ParentId, student.SchoolId);
                    }

                    var addedRow = await studentParentQueryService.GetstudentParentAsync(entity.ParentId, entity.StudentId);
                    var notificationProvider = notificationFactory.GetProvider(NotificationProvidersEnum.Mobile);
                    var devices = await userDeviceQueryService.GetByUserIdAsync(entity.ParentId);
                    if (devices.Count() > 0)
                    {
                        var tokens = devices.Select(d => d.FcmToken).ToList();
                        await notificationProvider.SendToMultiUsersAsync(tokens, "Relation", $"سعدنا انضمامك؛ تم ربط ملف الطالب {addedRow.StudentName} بحسابك بنجاح. نتمنى لكم رحلة متابعة ممتعة ومثمرة.");
                    }

                    return Ok(addedRow);
                }

                return BadRequest("Error in creating relation");

            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("UpdateStudentParent")]
        public async Task<IActionResult> UpdateStudentParent([FromBody] CreateStudentParentDTO entity)
        {
            try
            {
                if (!await schoolAccessValidator.CanAccessStudentAsync(CurrentUserId, entity.StudentId))
                {
                    return Forbid();
                }
                var parentStudent = await studentParentQueryService.GetstudentParentByIdAsync(entity.ParentId, entity.StudentId);

                if (parentStudent == null)
                    return BadRequest("There is no relation to update");

                var oldParentId = parentStudent.ParentId;

                parentStudent.ParentId = entity.ParentId;
                parentStudent.StudentId = entity.StudentId;
                parentStudent.RelationId = entity.RelationId;


                var added = studentParentCommandService.Create(parentStudent);

                if (added)
                {
                    var student = await studentQueryService.GetByIdAsync(entity.StudentId);
                    if (student != null)
                    {
                        if (oldParentId != entity.ParentId)
                        {
                            schoolAccessValidator.RemoveSchoolAccessCache(oldParentId, student.SchoolId);
                        }
                        schoolAccessValidator.RemoveSchoolAccessCache(entity.ParentId, student.SchoolId);
                    }
                    return Ok(await studentParentQueryService.GetstudentParentAsync(entity.ParentId, entity.StudentId));
                }

                return BadRequest("Error in creating relation");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("DeleteStudentParent")]
        public async Task<IActionResult> DeleteStudentParent([FromBody] StudentParent entity)
        {
            try
            {
                if (!await schoolAccessValidator.CanAccessStudentAsync(CurrentUserId, entity.StudentId))
                {
                    return Forbid();
                }
                var parentStudent = await studentParentQueryService.GetstudentParentByIdAsync(entity.ParentId, entity.StudentId);

                if (parentStudent == null)
                    return BadRequest("There is no relation to delete");

                var deleted = await studentParentCommandService.DeleteAsync(parentStudent);

                if (deleted)
                {
                    var student = await studentQueryService.GetByIdAsync(entity.StudentId);
                    if (student != null)
                    {
                        schoolAccessValidator.RemoveSchoolAccessCache(entity.ParentId, student.SchoolId);
                    }
                    return Ok("Relation deleted");
                }

                return BadRequest("Error in delete");

            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }








    }
}
