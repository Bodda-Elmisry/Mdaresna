using Mdaresna.Doamin.Enums;
using Mdaresna.Doamin.Models.SchoolManagement.ClassRoomManagement;
using Mdaresna.Doamin.Models.SchoolManagement.StudentManagement;
using Mdaresna.DTOs.Common;
using Mdaresna.DTOs.SchoolManagementDTO.StudentManagementDTO;
using Mdaresna.Repository.IFactories;
using Mdaresna.Repository.IServices.SchoolManagement.StudentManagement.Command;
using Mdaresna.Repository.IServices.SchoolManagement.StudentManagement.Query;
using Mdaresna.Repository.IServices.UserManagement.Query;
using Microsoft.AspNetCore.Mvc;

namespace Mdaresna.Controllers.SchoolManagement.StudentManagement
{
    [Route("StudentParent")]
    public class StudentParentController : Controller
    {
        private readonly IStudentParentQueryService studentParentQueryService;
        private readonly IStudentParentCommandService studentParentCommandService;
        private readonly INotificationFactory notificationFactory;
        private readonly IUserDeviceQueryService userDeviceQueryService;

        public StudentParentController(IStudentParentQueryService studentParentQueryService,
                                       IStudentParentCommandService studentParentCommandService,
                                           INotificationFactory notificationFactory,
                                           IUserDeviceQueryService userDeviceQueryService)
        {
            this.studentParentQueryService = studentParentQueryService;
            this.studentParentCommandService = studentParentCommandService;
            this.notificationFactory = notificationFactory;
            this.userDeviceQueryService = userDeviceQueryService;
        }

        [HttpPost("GetParentStudents")]
        public async Task<IActionResult> GetParentStudents([FromBody] ParentIdRelationIdDTO dTO)
        {
            try
            {
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
                    var addedRow = await studentParentQueryService.GetstudentParentAsync(entity.ParentId, entity.StudentId);
                    var notificationProvider = notificationFactory.GetProvider(NotificationProvidersEnum.Mobile);
                    var devices = await userDeviceQueryService.GetByUserIdAsync(entity.ParentId);
                    if (devices.Count() > 0)
                    {
                        var tokens = devices.Select(d => d.FcmToken).ToList();
                        await notificationProvider.SendToMultiUsersAsync(tokens, "Relation", $"Chield {addedRow.StudentName} added to you");
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
                var parentStudent = await studentParentQueryService.GetstudentParentByIdAsync(entity.ParentId, entity.StudentId);

                if (parentStudent == null)
                    return BadRequest("There is no relation to update");

                parentStudent.ParentId = entity.ParentId;
                parentStudent.StudentId = entity.StudentId;
                parentStudent.RelationId = entity.RelationId;


                var added = studentParentCommandService.Create(parentStudent);

                if (added)
                    return Ok(await studentParentQueryService.GetstudentParentAsync(entity.ParentId, entity.StudentId));

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
                var parentStudent = await studentParentQueryService.GetstudentParentByIdAsync(entity.ParentId, entity.StudentId);

                if (parentStudent == null)
                    return BadRequest("There is no relation to delete");

                var deleted = await studentParentCommandService.DeleteAsync(parentStudent);

                if (deleted)
                    return Ok("Relation deleted");

                return BadRequest("Error in delete");

            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }








    }
}
