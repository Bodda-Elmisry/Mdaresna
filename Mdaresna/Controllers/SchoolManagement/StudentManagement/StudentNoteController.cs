using Mdaresna.Doamin.Models.SchoolManagement.StudentManagement;
using Mdaresna.DTOs.Common;
using Mdaresna.DTOs.SchoolManagementDTO.StudentManagementDTO;
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

        public StudentNoteController(IStudentNoteQueryService studentNoteQueryService,
                                     IStudentNoteCommandService studentNoteCommandService)
        {
            this.studentNoteQueryService = studentNoteQueryService;
            this.studentNoteCommandService = studentNoteCommandService;
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
