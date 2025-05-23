﻿using Mdaresna.Doamin.DTOs.StudentManagement;
using Mdaresna.Doamin.Models.SchoolManagement.StudentManagement;
using Mdaresna.DTOs.SchoolManagementDTO.StudentManagementDTO;
using Mdaresna.Infrastructure.Services.SchoolManagement.StudentManagement.Command;
using Mdaresna.Infrastructure.Services.SchoolManagement.StudentManagement.Query;
using Mdaresna.Repository.IServices.SchoolManagement.StudentManagement.Command;
using Mdaresna.Repository.IServices.SchoolManagement.StudentManagement.Query;
using Microsoft.AspNetCore.Mvc;

namespace Mdaresna.Controllers.SchoolManagement.StudentManagement
{
    [Route("Attendance")]
    public class StudentAttendanceController : Controller
    {
        private readonly IStudentAttendanceCommandService studentAttendanceCommandService;
        private readonly IStudentAttendanceQueryService studentAttendanceQueryService;

        public StudentAttendanceController(IStudentAttendanceCommandService studentAttendanceCommandService,
                                           IStudentAttendanceQueryService studentAttendanceQueryService)
        {
            this.studentAttendanceCommandService = studentAttendanceCommandService;
            this.studentAttendanceQueryService = studentAttendanceQueryService;
        }

        [HttpPost("GetStudentsAttendences")]
        public async Task<IActionResult> GetStudentsAttendencesList([FromBody] GetStudentsAttendencesDTO studentsAttendencesDTO)
        {
            try
            {
                if(studentsAttendencesDTO.StudentId == null && studentsAttendencesDTO.ClassRoomId == null)
                {
                    return BadRequest("Can't get data without student or class room");
                }

                var data = await studentAttendanceQueryService.GetStudentsAttendancesAsync(studentsAttendencesDTO.StudentId, studentsAttendencesDTO.ClassRoomId, studentsAttendencesDTO.PageNumber);

                return Ok(data);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("SaveAttendance")]
        public async Task<IActionResult> AddClassRoomAttendence([FromBody] AddClassRoomAttendanceDTO attendanceDTO)
        {
            try
            {
                var attendenceCompleated = await studentAttendanceCommandService.AddClassRoomAttendance(attendanceDTO);
                if (attendenceCompleated)
                    return Ok("Attendence Compleated");

                return BadRequest("Errro in attendence");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("SoftDeleteStudentAttendence")]
        public async Task<IActionResult> SoftDeleteStudentAttendence([FromBody] StudentAttendanceIdDTO dto)
        {
            try
            {
                var studentAttendence = await studentAttendanceQueryService.GetByIdAsync(dto.StudentAttendanceId);

                if (studentAttendence == null)
                    return BadRequest("There is no attendence to delete");

                studentAttendence.Deleted = true;
                var deleted = studentAttendanceCommandService.Update(studentAttendence);

                return deleted ? Ok("Attendance Deleted") : BadRequest("Error in deleting attendance");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


    }
}
