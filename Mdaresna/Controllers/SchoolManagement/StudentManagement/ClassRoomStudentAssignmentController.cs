﻿using Mdaresna.Doamin.Models.SchoolManagement.ClassRoomManagement;
using Mdaresna.Doamin.Models.SchoolManagement.StudentManagement;
using Mdaresna.DTOs.SchoolManagementDTO.StudentManagementDTO;
using Mdaresna.Infrastructure.Services.SchoolManagement.StudentManagement.Command;
using Mdaresna.Repository.IServices.SchoolManagement.ClassRoomManagement.Command;
using Mdaresna.Repository.IServices.SchoolManagement.StudentManagement.Command;
using Mdaresna.Repository.IServices.SchoolManagement.StudentManagement.Query;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Validations;

namespace Mdaresna.Controllers.SchoolManagement.StudentManagement
{
    [Route("ClassRoomStudentAssignment")]
    public class ClassRoomStudentAssignmentController : Controller
    {
        private readonly IClassRoomStudentAssignmentQueryService classRoomStudentAssignmentQueryService;
        private readonly IClassRoomStudentAssignmentCommandService classRoomStudentAssignmentCommandService;
        private readonly IClassRoomAssignmentCommandService classRoomAssignmentCommandService;

        public ClassRoomStudentAssignmentController(IClassRoomStudentAssignmentQueryService classRoomStudentAssignmentQueryService,
                                                    IClassRoomStudentAssignmentCommandService classRoomStudentAssignmentCommandService,
                                                    IClassRoomAssignmentCommandService classRoomAssignmentCommandService)
        {
            this.classRoomStudentAssignmentQueryService = classRoomStudentAssignmentQueryService;
            this.classRoomStudentAssignmentCommandService = classRoomStudentAssignmentCommandService;
            this.classRoomAssignmentCommandService = classRoomAssignmentCommandService;
        }

        [HttpPost("GetStudentAssignmentsList")]
        public async Task<IActionResult> GetStudentAssignemtns([FromBody] GetClassRoomStudentAssignmentListDTO dto)
        {
            try
            {
                var list =  await classRoomStudentAssignmentQueryService.GetStudentAssignmentsListAsync(dto.StudentId,
                                                                                                 dto.AssignementId,
                                                                                                 dto.ResultFrom,
                                                                                                 dto.ResultTo,
                                                                                                 dto.IsDelivered,
                                                                                                 dto.DeliveredDateFrom,
                                                                                                 dto.DeliveredDateTo,
                                                                                                 dto.pageNumber);
                return Ok(list);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("GetStudentAssignment")]
        public async Task<IActionResult> GetStudentAssignemtn([FromBody] GetClassRoomStudentAssignmentDTO dto)
        {
            try
            {
                var list = await classRoomStudentAssignmentQueryService.GetClassRoomStudentAssignmentViewAsync(dto.StudentId,
                                                                                                 dto.AssignementId);
                return Ok(list);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("AddStudentAssignmnet")]
        public async Task<IActionResult> AddStudentAssignment([FromBody] CreateClassRoomStudentAssignmentDTO dto)
        {
            try
            {
                //var assignment = await classRoomStudentAssignmentQueryService.GetClassRoomStudentAssignmentViewAsync(dto.StudentId, dto.AssignmentId);

                //if (assignment == null)
                //    return BadRequest("This assignment already related to the student");

                var newAssignment = new ClassRoomAssignment
                {
                    AssignmentDate = dto.AssignmentDate,
                    ClassRoomId = dto.ClassRoomId,
                    CourseId = dto.CourseId,
                    Details = dto.Details,
                    Rate = dto.Rate,
                    SupervisorId = dto.SupervisorId,
                    WeekDay = dto.WeekDay
                };

                var assAdded = classRoomAssignmentCommandService.Create(newAssignment);

                if (!assAdded)
                    return BadRequest("Error in adding student assignment");

                var studentAssignment = new ClassRoomStudentAssignment
                {
                    AssignmentId = newAssignment.Id,
                    StudentId = dto.StudentId,
                    Result = dto.Result,
                    IsDelivered = dto.IsDelivered,
                    DeliveredDate = dto.DeliveredDate,

                };

                var sAssAdded = classRoomStudentAssignmentCommandService.Create(studentAssignment);

                if(!sAssAdded)
                    return BadRequest("Error in adding student assignment");

                return Ok(await classRoomStudentAssignmentQueryService.GetClassRoomStudentAssignmentViewAsync(studentAssignment.StudentId, studentAssignment.AssignmentId));

            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("UpdateStudentAssignmnet")]
        public async Task<IActionResult> UpdateStudentAssignment([FromBody] UpdateClassRoomStudentAssignmentDTO dto)
        {
            try
            {
                var sAss = await classRoomStudentAssignmentQueryService.GetClassRoomStudentAssignmentAsync(dto.StudentId, dto.AssignmentId);

                if (sAss == null)
                    return BadRequest("There is no assignment to update");

                sAss.Result = dto.Result;
                sAss.IsDelivered = dto.IsDelivered;
                sAss.DeliveredDate = dto.DeliveredDate;

                var updated = classRoomStudentAssignmentCommandService.Update(sAss);
                if (!updated)
                    return BadRequest("Error in Updating");

                return Ok(await classRoomStudentAssignmentQueryService.GetClassRoomStudentAssignmentViewAsync(sAss.StudentId, sAss.AssignmentId));

            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }




    }
}