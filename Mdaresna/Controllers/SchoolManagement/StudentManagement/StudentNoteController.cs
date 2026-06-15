using Mdaresna.Doamin.Enums;
using Mdaresna.Doamin.Models.SchoolManagement.StudentManagement;
using Mdaresna.DTOs.Common;
using Mdaresna.DTOs.SchoolManagementDTO.StudentManagementDTO;
using Mdaresna.Repository.IFactories;
using Mdaresna.Repository.IServices.SchoolManagement.StudentManagement.Command;
using Mdaresna.Repository.IServices.SchoolManagement.StudentManagement.Query;
using Microsoft.AspNetCore.Mvc;

namespace Mdaresna.Controllers.SchoolManagement.StudentManagement
{
    [Route("StudentNote")]
    public class StudentNoteController : Controller
    {
        private readonly IStudentNoteQueryService studentNoteQueryService;
        private readonly IStudentNoteCommandService studentNoteCommandService;
        private readonly IStudentQueryService studentQueryService;
        private readonly INotificationFactory notificationFactory;
        private readonly IStudentTransactionsFactory studentTransactionsFactory;

        public StudentNoteController(IStudentNoteQueryService studentNoteQueryService,
                                     IStudentNoteCommandService studentNoteCommandService,
                                           IStudentQueryService studentQueryService,
                                           INotificationFactory notificationFactory,
                                           IStudentTransactionsFactory studentTransactionsFactory)
        {
            this.studentNoteQueryService = studentNoteQueryService;
            this.studentNoteCommandService = studentNoteCommandService;
            this.studentQueryService = studentQueryService;
            this.notificationFactory = notificationFactory;
            this.studentTransactionsFactory = studentTransactionsFactory;
        }

        [HttpPost("GetStudentNotesList")]
        public async Task<IActionResult> GetStudentNotesList([FromBody] GetStudentNotesDTO dTO)
        {
            try
            {
                var nots = await studentNoteQueryService.GetStudentNotesListAsync(dTO.StudentId,
                                                                            dTO.ClassRoomId,
                                                                            dTO.SupervisorId,
                                                                            dTO.CourseId,
                                                                            dTO.DateFrom,
                                                                            dTO.DateTo,
                                                                            dTO.Notes,
                                                                            dTO.pageNumber);
                return Ok(nots);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("GetStudentNote")]
        public async Task<IActionResult> GetStudentNote([FromBody] StudentNoteIdDTO dTO)
        {
            try
            {
                var not = studentNoteQueryService.GetStudentNoteViewById(dTO.NoteId);

                return Ok(not);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("CreateStudentNote")]
        public async Task<IActionResult> CreateStudentNote([FromBody] CreateStudentNoteDTO dTO)
        {
            try
            {
                var note = new StudentNote
                {
                    Date = dTO.Date,
                    Notes = dTO.Notes,
                    CourseId = dTO.CourseId,
                    SupervisorId = dTO.SupervisorId,
                    ClassRoomId = dTO.ClassRoomId,
                    StudentId = dTO.StudentId
                };

                var added = studentNoteCommandService.Create(note);

                if (!added)
                    return BadRequest("Error in adding note");

                try
                {
                    var notificationProvider = notificationFactory.GetProvider(NotificationProvidersEnum.Mobile);
                    var studentProvider = studentTransactionsFactory.GetProvider(StudentTransactionProvidersEnum.Note);
                    var studentIds = new List<Guid> { dTO.StudentId };
                    var devices = await studentProvider.GetTransactionSTudentsParentsDevicesAsync(studentIds);
                    if (devices != null && devices.Any())
                    {
                        var tokens = devices
                            .Select(d => d.FcmTocken)
                            .Where(t => !string.IsNullOrWhiteSpace(t))
                            .Distinct()
                            .ToList();

                        if (tokens.Any())
                        {
                            var student = await studentQueryService.GetByIdAsync(dTO.StudentId);
                            await notificationProvider.SendToMultiUsersAsync(tokens, "ملاحظة جديدة", $"توجد ملاحظة جديدة بخصوص الطالب {student.FirstName} {student.LastName}. يهمنا اطلاعكم عليها لنستمر معاً في دعم مسيرته التعليمية.|Type=Note|TargetId={note.Id}|StudentId={dTO.StudentId}");
                        }
                    }
                }
                catch (Exception)
                {
                    // Ignore notification failures so the API request is not aborted
                }

                return Ok(await studentNoteQueryService.GetStudentNoteViewById(note.Id));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("UpdateStudentNote")]
        public async Task<IActionResult> UpadteStudentNote([FromBody] UpdateStudentNoteDTO dTO)
        {
            try
            {
                var note = await studentNoteQueryService.GetByIdAsync(dTO.Id);

                if (note == null)
                    return BadRequest("There is no note to update");

                note.Date = dTO.Date;
                note.Notes = dTO.Notes;
                note.CourseId = dTO.CourseId;

                var updated = studentNoteCommandService.Update(note);

                if (!updated) 
                    return BadRequest("Error in updating note");

                return Ok(await studentNoteQueryService.GetStudentNoteViewById(dTO.Id));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("SoftDeleteStudentNote")]
        public async Task<IActionResult> SoftDeleteStudentNote([FromBody] StudentNoteIdDTO dTO)
        {
            try
            {
                var note = await studentNoteQueryService.GetByIdAsync(dTO.NoteId);

                if (note == null)
                    return BadRequest("There is no note to delete");

                note.Deleted = true;

                var deleted = studentNoteCommandService.Update(note);

                return deleted ? Ok("Student note Deleted") : BadRequest("Error in delete student note");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }






    }
}
