using Mdaresna.Doamin.DTOs.Common;
using Mdaresna.Doamin.DTOs.SettingsManagement;
using Mdaresna.Doamin.Models.SettingsManagement;
using Mdaresna.DTOs.SettingsManagementDTO;
using Mdaresna.Repository.IServices.SettingsManagement.Query;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Mdaresna.Controllers.SettingsManagement;

[Route("ReportQueue")]
[Authorize]
public class ReportQueueController : Controller
{
    private readonly IReportQueueQueryService reportQueueQueryService;

    public ReportQueueController(IReportQueueQueryService reportQueueQueryService)
    {
        this.reportQueueQueryService = reportQueueQueryService;
    }

    [HttpPost("GetBySchool")]
    public async Task<IActionResult> GetBySchool([FromBody] GetReportQueuesDTO dto)
    {
        try
        {
            if (dto == null || dto.SchoolId == Guid.Empty)
            {
                return BadRequest("School id is required.");
            }

            var queues = await reportQueueQueryService.GetBySchoolAsync(
                dto.SchoolId,
                dto.MonthId,
                dto.GradeId,
                dto.ClassroomId,
                dto.Status,
                dto.ReportType);
            return Ok(queues.Select(MapToResult));
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("GetBySchoolPaged")]
    public async Task<IActionResult> GetBySchoolPaged([FromBody] GetReportQueuesPagedDTO dto)
    {
        try
        {
            if (dto == null || dto.SchoolId == Guid.Empty)
            {
                return BadRequest("School id is required.");
            }

            var queues = await reportQueueQueryService.GetBySchoolPagedAsync(
                dto.SchoolId,
                dto.MonthId,
                dto.GradeId,
                dto.ClassroomId,
                dto.Status,
                dto.ReportType,
                dto.PageNumber,
                dto.PageSize);

            return Ok(new PagedResultDTO<ReportQueueResultDTO>
            {
                Items = queues.Items.Select(MapToResult),
                TotalCount = queues.TotalCount,
                PageNumber = queues.PageNumber,
                PageSize = queues.PageSize,
                TotalPages = queues.TotalPages
            });
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("GetStudentReportsByReportId")]
    public async Task<IActionResult> GetStudentReportsByReportId(
        [FromBody] GetStudentReportsByReportIdDTO dto,
        CancellationToken cancellationToken)
    {
        try
        {
            var reportQueueId = dto?.GetReportQueueId() ?? Guid.Empty;
            if (reportQueueId == Guid.Empty)
            {
                return BadRequest("Report id is required.");
            }

            var reports = await reportQueueQueryService.GetStudentReportsByReportIdAsync(
                reportQueueId,
                dto?.GradeId,
                dto?.ClassRoomId,
                cancellationToken);

            return Ok(reports);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("GetStudentReportsMonths")]
    public async Task<IActionResult> GetStudentReportsMonths(
        [FromBody] GetStudentReportsMonthsDTO dto,
        CancellationToken cancellationToken)
    {
        try
        {
            if (dto == null || dto.SchoolId == Guid.Empty || dto.StudentId == Guid.Empty)
            {
                return BadRequest("School id and Student id are required.");
            }

            var queues = await reportQueueQueryService.GetStudentReportsQueuesAsync(
                dto.SchoolId,
                dto.StudentId,
                cancellationToken);

            return Ok(queues.Select(MapToResult));
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    private static ReportQueueResultDTO MapToResult(ReportQueue queue)
    {
        return new ReportQueueResultDTO
        {
            Id = queue.Id,
            SchoolId = queue.SchoolId,
            SchoolName = queue.School?.Name ?? string.Empty,
            GradeId = queue.GradeId,
            GradeName = queue.Grade?.Name,
            ClassroomId = queue.ClassroomId,
            ClassroomName = queue.Classroom?.Name,
            FromDate = queue.FromDate,
            ToDate = queue.ToDate,
            MonthId = queue.MonthId,
            MonthName = queue.Month?.Name,
            WeekName = queue.WeekName,
            ReportType = queue.ReportType,
            Status = queue.Status,
            CreatedById = queue.CreatedById,
            CreatedByName = GetUserFullName(
                queue.CreatedBy?.FirstName,
                queue.CreatedBy?.LastName),
            CreatedAt = queue.CreatedAt,
            StartedAt = queue.StartedAt,
            CompletedAt = queue.CompletedAt,
            ReviewdById = queue.ReviewdById,
            ReviewdByName = queue.ReviewdBy == null
                ? null
                : GetUserFullName(queue.ReviewdBy.FirstName, queue.ReviewdBy.LastName),
            PublishedAt = queue.PublishedAt,
            Errors = queue.Errors,
            AffectedRows = queue.AffectedRows,
            RetryCount = queue.RetryCount,
            Notes = queue.Notes
        };
    }

    private static string GetUserFullName(string? firstName, string? lastName)
    {
        return $"{firstName} {lastName}".Trim();
    }
}
