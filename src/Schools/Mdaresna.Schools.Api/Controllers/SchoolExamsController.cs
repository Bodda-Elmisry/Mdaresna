using System.IdentityModel.Tokens.Jwt;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using Mdaresna.Api.Contracts;
using Mdaresna.Schools.Api.Auth;
using Mdaresna.Schools.Api.Time;
using Mdaresna.Schools.Api.Exams;
using Mdaresna.Schools.Domain.Academics;
using Mdaresna.Schools.Domain.Exams;
using Mdaresna.Schools.Domain.Identity;
using Mdaresna.Schools.Domain.Students;
using Mdaresna.Schools.Infrastructure.Identity;
using Mdaresna.Schools.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Mdaresna.Schools.Api.Controllers;

[ApiController, Authorize, Route("api/schools/v1")]
public sealed partial class SchoolExamsController(ISchoolDbContextFactory dbFactory, SchoolClock clock) : ControllerBase
{
    [HttpGet("exams/options"), Authorize(Policy = SchoolPermissionPolicies.ExamsView)]
    public IActionResult Options() => Ok(ApiResponse<object>.Success(new
    {
        kinds = Enum.GetNames<ExamKind>(), authorities = Enum.GetNames<ExamAuthorityType>(),
        formats = Enum.GetNames<ExamFormat>(), administrationModes = Enum.GetNames<ExamAdministrationMode>(),
        seriesStatuses = Enum.GetNames<ExamSeriesStatus>(), resultStatuses = Enum.GetNames<ExamResultsStatus>()
    }, correlationId: HttpContext.TraceIdentifier));

    [HttpGet("class-workspace/{classSectionId:guid}/exams/subjects"), Authorize(Policy = SchoolPermissionPolicies.ExamsView)]
    public async Task<IActionResult> Subjects(Guid classSectionId, CancellationToken ct)
    {
        await using var db = await RequireDb(ct); if (db is null) return Unauthorized();
        if (!await ClassExists(db, classSectionId, ct)) return NotFound(Failure(404, "exams.class_not_found", "Class section was not found."));
        var admin = await IsAdmin(db, ct); var userId = CurrentUserId(); var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var query = db.ClassSectionSubjects.AsNoTracking().Where(x => x.ClassSectionId == classSectionId && x.IsActive && !x.IsDeleted &&
            x.GradeSubjectOffering.IsActive && !x.GradeSubjectOffering.IsDeleted);
        if (!admin) query = query.Where(x =>
            db.ClassSectionTeacherScopes.Any(scope => scope.ClassSectionId == classSectionId && scope.IsActive && !scope.IsDeleted &&
                scope.TeacherGradeSubjectScope.IsActive && !scope.TeacherGradeSubjectScope.IsDeleted && scope.TeacherGradeSubjectScope.TeacherUserId == userId &&
                scope.TeacherGradeSubjectScope.GradeSubjectOfferingId == x.GradeSubjectOfferingId) ||
            db.DepartmentLeaderships.Any(lead => lead.UserId == userId && lead.IsActive && !lead.IsDeleted && lead.StartsOn <= today &&
                (!lead.EndsOn.HasValue || lead.EndsOn >= today) && lead.Department.Subjects.Any(subject => subject.IsActive && !subject.IsDeleted &&
                    subject.SubjectId == x.GradeSubjectOffering.CurriculumGradeSubject.SubjectId)) ||
            db.SubjectCoordinatorAssignments.Any(coordinator => coordinator.CoordinatorUserId == userId && coordinator.IsActive && !coordinator.IsDeleted &&
                coordinator.StartsOn <= today && (!coordinator.EndsOn.HasValue || coordinator.EndsOn >= today) &&
                coordinator.DepartmentSubject.SubjectId == x.GradeSubjectOffering.CurriculumGradeSubject.SubjectId));
        var rows = await query.OrderBy(x => x.GradeSubjectOffering.CurriculumGradeSubject.Subject.NameAr).Select(x =>
            new ExamSubjectResponse(x.Id, x.GradeSubjectOfferingId, x.GradeSubjectOffering.CurriculumGradeSubject.Subject.NameAr,
                x.GradeSubjectOffering.CurriculumGradeSubject.Subject.NameEn)).ToArrayAsync(ct);
        return Ok(ApiResponse<IReadOnlyList<ExamSubjectResponse>>.Success(rows, correlationId: HttpContext.TraceIdentifier));
    }

    [HttpGet("class-workspace/{classSectionId:guid}/exams/period-options"), Authorize(Policy = SchoolPermissionPolicies.ExamsView)]
    public async Task<IActionResult> PeriodOptions(Guid classSectionId, CancellationToken ct)
    {
        await using var db = await RequireDb(ct); if (db is null) return Unauthorized();
        if (!await CanAccessClass(db, classSectionId, ct)) return Forbid();
        var yearId = await db.ClassSections.AsNoTracking().Where(x => x.Id == classSectionId && x.IsActive && !x.IsDeleted)
            .Select(x => (Guid?)x.GradeOffering.ProgramAcademicYearId).SingleOrDefaultAsync(ct);
        if (!yearId.HasValue) return NotFound(Failure(404, "exams.class_not_found", "Class section was not found."));
        var terms = await db.AcademicTerms.AsNoTracking().Where(x => x.ProgramAcademicYearId == yearId && x.IsActive && !x.IsDeleted)
            .OrderBy(x => x.SortOrder).Select(x => new ClassExamTermOption(x.Id, x.NameAr, x.NameEn, x.StartDate, x.EndDate, x.SortOrder))
            .ToArrayAsync(ct);
        return Ok(ApiResponse<ClassExamPeriodOptionsResponse>.Success(new(yearId.Value, terms), correlationId: HttpContext.TraceIdentifier));
    }

    [HttpGet("class-workspace/{classSectionId:guid}/exams"), Authorize(Policy = SchoolPermissionPolicies.ExamsView)]
    public async Task<IActionResult> ClassExams(Guid classSectionId, [FromQuery] ExamKind? kind, [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20, CancellationToken ct = default)
    {
        await using var db = await RequireDb(ct); if (db is null) return Unauthorized();
        if (pageNumber < 1 || pageSize is < 1 or > 100) return BadRequest(Failure(400, "exams.paging_invalid", "Invalid paging values."));
        if (!await CanAccessClass(db, classSectionId, ct)) return Forbid();
        var query = ApplyScope(db, db.ExamPapers.IgnoreQueryFilters().AsNoTracking()
            .Where(x => x.Targets.Any(target => target.ClassSectionId == classSectionId)));
        if (kind.HasValue) query = query.Where(x => x.ExamSeries.Kind == kind);
        var total = await query.CountAsync(ct);
        var rows = await ToList(query).Skip((pageNumber - 1) * pageSize).Take(pageSize).ToArrayAsync(ct);
        return Ok(PagedApiResponse<ExamListItemResponse>.Success(rows, total, pageNumber, pageSize, correlationId: HttpContext.TraceIdentifier));
    }

    [HttpGet("class-workspace/{classSectionId:guid}/exams/summary"), Authorize(Policy = SchoolPermissionPolicies.ExamsView)]
    public async Task<IActionResult> ClassExamsSummary(Guid classSectionId, CancellationToken ct)
    {
        await using var db = await RequireDb(ct); if (db is null) return Unauthorized();
        if (!await CanAccessClass(db, classSectionId, ct)) return Forbid();
        var rows = await ApplyScope(db, db.ExamPapers.IgnoreQueryFilters().AsNoTracking()
                .Where(x => x.Targets.Any(target => target.ClassSectionId == classSectionId)))
            .GroupBy(x => x.ExamSeries.Kind)
            .Select(group => new ExamKindSummaryResponse(group.Key, group.Count(),
                group.Count(x => x.ExamSeries.Status == ExamSeriesStatus.Published),
                group.Sum(x => x.Candidates.Count)))
            .ToArrayAsync(ct);
        var response = new ExamCenterSummaryResponse(rows.Sum(x => x.PaperCount), rows.Sum(x => x.PublishedCount),
            rows.Sum(x => x.CandidateCount), rows);
        return Ok(ApiResponse<ExamCenterSummaryResponse>.Success(response, correlationId: HttpContext.TraceIdentifier));
    }

    [HttpGet("exam-series"), Authorize(Policy = SchoolPermissionPolicies.ExamsView)]
    public async Task<IActionResult> Series([FromQuery] ExamKind? kind, [FromQuery] ExamSeriesStatus? status,
        [FromQuery] Guid? classSectionId, [FromQuery] DateOnly? from, [FromQuery] DateOnly? to,
        [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20, CancellationToken ct = default)
    {
        await using var db = await RequireDb(ct); if (db is null) return Unauthorized();
        if (pageNumber < 1 || pageSize is < 1 or > 100) return BadRequest(Failure(400, "exams.paging_invalid", "Invalid paging values."));
        if (from.HasValue && to.HasValue && from > to) return BadRequest(Failure(400, "exams.date_range_invalid", "The date range is invalid."));
        var query = ApplyScope(db, db.ExamPapers.IgnoreQueryFilters().AsNoTracking());
        if (kind.HasValue) query = query.Where(x => x.ExamSeries.Kind == kind);
        if (status.HasValue) query = query.Where(x => x.ExamSeries.Status == status);
        if (classSectionId.HasValue) query = query.Where(x => x.Targets.Any(target => target.ClassSectionId == classSectionId));
        if (from.HasValue) query = query.Where(x => x.Sittings.Any(s => s.ExamScheduleWindow.LocalDate >= from));
        if (to.HasValue) query = query.Where(x => x.Sittings.Any(s => s.ExamScheduleWindow.LocalDate <= to));
        var total = await query.CountAsync(ct);
        var rows = await ToList(query).Skip((pageNumber - 1) * pageSize).Take(pageSize).ToArrayAsync(ct);
        return Ok(PagedApiResponse<ExamListItemResponse>.Success(rows, total, pageNumber, pageSize, correlationId: HttpContext.TraceIdentifier));
    }

    [HttpGet("exam-series/summary"), Authorize(Policy = SchoolPermissionPolicies.ExamsView)]
    public async Task<IActionResult> SeriesSummary(CancellationToken ct)
    {
        await using var db = await RequireDb(ct); if (db is null) return Unauthorized();
        var rows = await ApplyScope(db, db.ExamPapers.IgnoreQueryFilters().AsNoTracking())
            .GroupBy(x => x.ExamSeries.Kind)
            .Select(group => new ExamKindSummaryResponse(group.Key, group.Count(),
                group.Count(x => x.ExamSeries.Status == ExamSeriesStatus.Published),
                group.Sum(x => x.Candidates.Count)))
            .ToArrayAsync(ct);
        var response = new ExamCenterSummaryResponse(rows.Sum(x => x.PaperCount), rows.Sum(x => x.PublishedCount),
            rows.Sum(x => x.CandidateCount), rows);
        return Ok(ApiResponse<ExamCenterSummaryResponse>.Success(response, correlationId: HttpContext.TraceIdentifier));
    }

    [HttpPost("class-workspace/{classSectionId:guid}/exams/quick"), Authorize(Policy = SchoolPermissionPolicies.ExamsManage)]
    public async Task<IActionResult> QuickCreate(Guid classSectionId, [FromBody] SaveQuickExamRequest request, CancellationToken ct)
    {
        await using var db = await RequireDb(ct); if (db is null) return Unauthorized();
        var school = await db.SchoolInformation.AsNoTracking().SingleOrDefaultAsync(ct);
        if (school?.TimeZoneId is null) return Conflict(Failure(409, "exams.time_zone_required", "Configure a valid school time zone first."));
        SchoolLocalNow localNow;
        try { localNow = clock.Now(school.TimeZoneId); }
        catch (Exception ex) when (ex is TimeZoneNotFoundException or InvalidTimeZoneException)
        { return Conflict(Failure(409, "exams.time_zone_required", "Configure a valid school time zone first.")); }
        var validation = await ValidateQuickRequest(db, classSectionId, request, ct); if (validation.Result is not null) return validation.Result;
        var subject = validation.Subject!; var section = validation.Section!;
        Guid? termId;
        DateOnly? assessmentMonth = null;
        if (request.Kind == ExamKind.Monthly)
        {
            var term = request.AcademicTermId.HasValue ? await db.AcademicTerms.AsNoTracking()
                .Where(x => x.Id == request.AcademicTermId && x.ProgramAcademicYearId == section.ProgramAcademicYearId && x.IsActive && !x.IsDeleted)
                .Select(x => new { x.Id, x.StartDate, x.EndDate }).SingleOrDefaultAsync(ct) : null;
            if (term is null) return BadRequest(Failure(400, "exams.term_required", "Select a term that belongs to the academic year."));
            if (!request.AssessmentMonth.HasValue || request.AssessmentMonth.Value.Day != 1 ||
                !SchoolExamRules.MonthIntersectsTerm(request.AssessmentMonth.Value, term.StartDate, term.EndDate))
                return BadRequest(Failure(400, "exams.month_outside_term", "Select a normalized month that intersects the selected term."));
            assessmentMonth = request.AssessmentMonth.Value;
            if (request.ExamDate < term.StartDate || request.ExamDate > term.EndDate ||
                request.ExamDate.Year != assessmentMonth.Value.Year || request.ExamDate.Month != assessmentMonth.Value.Month)
                return BadRequest(Failure(400, "exams.date_outside_month", "The exam date must be inside the selected month and term."));
            if (await db.ExamPaperTargets.AsNoTracking().AnyAsync(x => x.ClassSectionId == classSectionId &&
                x.ExamPaper.GradeSubjectOfferingId == subject.GradeSubjectOfferingId &&
                x.ExamPaper.ExamSeries.ProgramAcademicYearId == section.ProgramAcademicYearId &&
                x.ExamPaper.ExamSeries.AcademicTermId == term.Id && x.ExamPaper.ExamSeries.AssessmentMonth == assessmentMonth &&
                x.ExamPaper.ExamSeries.Kind == ExamKind.Monthly && x.ExamPaper.ExamSeries.Purpose == ExamSeriesPurpose.Regular &&
                x.ExamPaper.ExamSeries.Status != ExamSeriesStatus.Cancelled, ct))
                return Conflict(Failure(409, "exams.monthly_scope_overlap", "A monthly exam already covers this class and subject."));
            termId = term.Id;
        }
        else
        {
            termId = await db.AcademicTerms.AsNoTracking().Where(x => x.ProgramAcademicYearId == section.ProgramAcademicYearId &&
                x.StartDate <= request.ExamDate && x.EndDate >= request.ExamDate && x.IsActive && !x.IsDeleted)
                .Select(x => (Guid?)x.Id).FirstOrDefaultAsync(ct);
        }
        var applicablePolicy = await db.ExamPolicies.AsNoTracking().Where(x => x.IsActive &&
                (!x.EducationProgramId.HasValue || x.EducationProgramId == section.EducationProgramId) &&
                (!x.EducationStageId.HasValue || x.EducationStageId == section.EducationStageId) &&
                (!x.ExamKind.HasValue || x.ExamKind == request.Kind) && x.EffectiveFrom <= request.ExamDate &&
                (!x.EffectiveTo.HasValue || x.EffectiveTo >= request.ExamDate))
            .OrderByDescending(x => x.EducationStageId.HasValue).ThenByDescending(x => x.EducationProgramId.HasValue)
            .ThenByDescending(x => x.ExamKind.HasValue).ThenByDescending(x => x.Version).FirstOrDefaultAsync(ct);
        if (applicablePolicy is { TeacherCanCreate: false } && !await IsAdmin(db, ct)) return Forbid();
        DateTimeOffset startsAtUtc, endsAtUtc; TimeOnly endsAtLocal;
        try
        {
            var zone = SchoolClock.Resolve(localNow.TimeZoneId);
            startsAtUtc = ToUtc(request.ExamDate, request.StartsAt, zone);
            var localStart = request.ExamDate.ToDateTime(request.StartsAt); var localEnd = localStart.AddMinutes(request.DurationMinutes);
            if (localEnd.Date != localStart.Date) return BadRequest(Failure(400, "exams.window_invalid", "The exam must end on the same school day."));
            endsAtLocal = TimeOnly.FromDateTime(localEnd); endsAtUtc = ToUtc(request.ExamDate, endsAtLocal, zone);
        }
        catch (ArgumentException) { return BadRequest(Failure(400, "exams.local_time_invalid", "The selected local time is invalid in the school time zone.")); }

        var room = await db.ClassRoomAssignments.AsNoTracking().Where(x => x.ClassSectionId == classSectionId && x.IsActive && !x.IsDeleted &&
                x.EffectiveFrom <= request.ExamDate && x.EffectiveTo >= request.ExamDate &&
                (!x.StartsAt.HasValue || x.StartsAt <= request.StartsAt) && (!x.EndsAt.HasValue || x.EndsAt >= endsAtLocal))
            .OrderByDescending(x => x.IsPrimary).Select(x => new { x.RoomId, x.Room.Capacity, x.Room.NameAr }).FirstOrDefaultAsync(ct);
        var now = localNow.UtcNow; var seriesId = Guid.NewGuid(); var paperId = Guid.NewGuid(); var windowId = Guid.NewGuid();
        var sittingId = Guid.NewGuid(); var venueId = Guid.NewGuid(); var sittingVenueId = Guid.NewGuid();
        var titleAr = request.Kind == ExamKind.Monthly
            ? $"اختبار شهر {ArabicMonth(assessmentMonth!.Value.Month)} — مادة {subject.NameAr}"
            : request.TitleAr.Trim();
        var titleEn = request.Kind == ExamKind.Monthly
            ? $"{EnglishMonth(assessmentMonth!.Value.Month)} monthly exam — {subject.NameEn}"
            : string.IsNullOrWhiteSpace(request.TitleEn) ? titleAr : request.TitleEn.Trim();
        var series = new ExamSeries
        {
            Id = seriesId, ProgramAcademicYearId = section.ProgramAcademicYearId, AcademicTermId = termId,
            EducationStageId = section.EducationStageId, ScopeGradeOfferingId = section.GradeOfferingId,
            ScopeClassSectionId = classSectionId, ScopeLevel = ExamScopeLevel.ClassSection, AssessmentMonth = assessmentMonth,
            Purpose = ExamSeriesPurpose.Regular,
            Code = ExamCode(now), NameAr = titleAr, NameEn = titleEn, Kind = request.Kind,
            IssuingAuthority = request.IssuingAuthority, SchedulingAuthority = request.SchedulingAuthority,
            DefaultAdministrationMode = ExamAdministrationMode.InClass, TimeZoneIdSnapshot = localNow.TimeZoneId,
            PolicySnapshotJson = applicablePolicy is null ? null : JsonSerializer.Serialize(new ExamPolicySnapshot(applicablePolicy.Id,
                applicablePolicy.Version, applicablePolicy.DefaultAdministrationMode, applicablePolicy.TeacherCanCreate,
                applicablePolicy.TeacherCanPublishWithoutApproval, applicablePolicy.DepartmentApprovalRequired,
                applicablePolicy.SchoolApprovalRequired, applicablePolicy.ResultApprovalRequired,
                applicablePolicy.AllowScheduleWarningOverride, applicablePolicy.WorkflowDefaultsJson)),
            ExternalSourceCode = Clean(request.ExternalSourceCode, 100), ExternalAuthorityName = Clean(request.ExternalAuthorityName, 250),
            ExternalReferenceId = Clean(request.ExternalReferenceId, 200), ExternalRevision = Clean(request.ExternalRevision, 100),
            IsExternalScheduleLocked = request.IsExternalScheduleLocked, CreatedByUserId = CurrentUserId(), CreatedAtUtc = now, UpdatedAtUtc = now
        };
        series.Targets.Add(new ExamSeriesTarget { Id = Guid.NewGuid(), ExamSeries = series, GradeOfferingId = section.GradeOfferingId, ClassSectionId = classSectionId });
        var paper = new ExamPaper
        {
            Id = paperId, ExamSeries = series, GradeSubjectOfferingId = subject.GradeSubjectOfferingId, PaperCode = "P1",
            TitleAr = titleAr, TitleEn = titleEn, Format = request.Format, Instructions = Clean(request.Instructions, 6000),
            TotalScore = request.TotalScore, PassScore = request.PassScore, DurationMinutes = request.DurationMinutes,
            ContentOwnerUserId = CurrentUserId(), CreatedAtUtc = now, UpdatedAtUtc = now
        };
        paper.Targets.Add(new ExamPaperTarget { Id = Guid.NewGuid(), ExamPaper = paper, GradeOfferingId = section.GradeOfferingId,
            ClassSectionId = classSectionId, ClassSectionSubjectId = request.ClassSectionSubjectId });
        var window = new ExamScheduleWindow
        {
            Id = windowId, ExamSeries = series, LocalDate = request.ExamDate, StartsAtLocal = request.StartsAt, EndsAtLocal = endsAtLocal,
            StartsAtUtc = startsAtUtc, EndsAtUtc = endsAtUtc, TimeZoneIdSnapshot = localNow.TimeZoneId, ScheduledByUserId = CurrentUserId(),
            IsScheduleLocked = request.IsExternalScheduleLocked, CreatedAtUtc = now, UpdatedAtUtc = now
        };
        var sitting = new ExamSitting { Id = sittingId, ExamPaper = paper, ExamScheduleWindow = window,
            DurationMinutesSnapshot = request.DurationMinutes, CreatedAtUtc = now, UpdatedAtUtc = now };
        var venue = new ExamWindowVenue { Id = venueId, ExamScheduleWindow = window, RoomId = room?.RoomId,
            ClassSectionId = classSectionId, CapacitySnapshot = room?.Capacity ?? section.Capacity,
            VenueLabel = room?.NameAr, CreatedAtUtc = now, UpdatedAtUtc = now };
        var sittingVenue = new ExamSittingVenue { Id = sittingVenueId, ExamSitting = sitting, ExamWindowVenue = venue, CreatedAtUtc = now };
        await using var createTransaction = request.PublishImmediately ? await db.Database.BeginTransactionAsync(ct) : null;
        db.ExamSeries.Add(series); db.ExamPapers.Add(paper); db.ExamScheduleWindows.Add(window); db.ExamSittings.Add(sitting);
        db.ExamWindowVenues.Add(venue); db.ExamSittingVenues.Add(sittingVenue); AddAudit(db, series, "Created", now);
        await db.SaveChangesAsync(ct);
        if (request.PublishImmediately)
        {
            var publish = await PublishCore(db, seriesId, ct, ownsTransaction: false);
            if (publish is not null)
            {
                await createTransaction!.RollbackAsync(ct);
                return publish;
            }
            await createTransaction!.CommitAsync(ct);
        }
        return Ok(ApiResponse<object>.Success(new { seriesId, paperId, sittingId }, correlationId: HttpContext.TraceIdentifier));
    }

    [HttpGet("exam-series/{id:guid}"), Authorize(Policy = SchoolPermissionPolicies.ExamsView)]
    public async Task<IActionResult> Details(Guid id, CancellationToken ct)
    {
        await using var db = await RequireDb(ct); if (db is null) return Unauthorized();
        var series = await LoadSeries(db, id, ct); if (series is null) return NotFound(Failure(404, "exams.not_found", "Exam series was not found."));
        if (!await CanAccessSeries(db, series.Id, ct)) return Forbid();
        return Ok(ApiResponse<ExamSeriesDetailsResponse>.Success(ToDetails(series), correlationId: HttpContext.TraceIdentifier));
    }

    [HttpDelete("exam-series/{id:guid}"), Authorize(Policy = SchoolPermissionPolicies.ExamsManage)]
    public async Task<IActionResult> DeleteDraft(Guid id, CancellationToken ct)
    {
        await using var db = await RequireDb(ct); if (db is null) return Unauthorized();
        var series = await LoadSeries(db, id, ct); if (series is null) return NotFound(Failure(404, "exams.not_found", "Exam series was not found."));
        if (!await CanAccessSeries(db, id, ct)) return Forbid();
        if (series.Status != ExamSeriesStatus.Draft) return Conflict(Failure(409, "exams.not_draft", "Only a draft exam can be deleted."));
        var workflow = await db.ExamWorkflows.Include(x => x.Steps).SingleOrDefaultAsync(x => x.ExamSeriesId == id, ct);
        if (workflow?.Status == ExamWorkflowStatus.Active) return Conflict(Failure(409, "exams.workflow_locked", "Cancel the active workflow before removing the exam."));
        if (workflow is not null) { db.ExamWorkflowSteps.RemoveRange(workflow.Steps); db.ExamWorkflows.Remove(workflow); }
        db.ExamApprovals.RemoveRange(await db.ExamApprovals.Where(x => x.ExamSeriesId == id).ToListAsync(ct));
        db.ExamInvigilatorAssignments.RemoveRange(series.Committees.SelectMany(x => x.Invigilators));
        db.ExamSittingVenues.RemoveRange(series.ScheduleWindows.SelectMany(x => x.Sittings).SelectMany(x => x.Venues));
        db.ExamWindowVenues.RemoveRange(series.ScheduleWindows.SelectMany(x => x.Venues)); db.ExamCommittees.RemoveRange(series.Committees);
        db.ExamSittings.RemoveRange(series.Papers.SelectMany(x => x.Sittings));
        db.ExamScheduleWindows.RemoveRange(series.ScheduleWindows); db.ExamPaperTargets.RemoveRange(series.Papers.SelectMany(x => x.Targets));
        db.ExamPapers.RemoveRange(series.Papers); db.ExamSeriesTargets.RemoveRange(series.Targets); db.ExamAudits.RemoveRange(series.AuditTrail);
        db.ExamSeries.Remove(series); await db.SaveChangesAsync(ct);
        return Ok(ApiResponse<object?>.Success(null, correlationId: HttpContext.TraceIdentifier));
    }

    [HttpPost("exam-series/{id:guid}/submit-approval"), Authorize(Policy = SchoolPermissionPolicies.ExamsManage)]
    public async Task<IActionResult> SubmitApproval(Guid id, CancellationToken ct) => await ChangeSeriesStatus(id,
        ExamSeriesStatus.Draft, ExamSeriesStatus.PendingApproval, "SubmittedForApproval", null, ct);

    [HttpPost("exam-series/{id:guid}/approve"), Authorize(Policy = SchoolPermissionPolicies.ExamsApprove)]
    public async Task<IActionResult> Approve(Guid id, CancellationToken ct) => await ChangeSeriesStatus(id,
        ExamSeriesStatus.PendingApproval, ExamSeriesStatus.Approved, "Approved", null, ct);

    [HttpPost("exam-series/{id:guid}/reject"), Authorize(Policy = SchoolPermissionPolicies.ExamsApprove)]
    public async Task<IActionResult> Reject(Guid id, [FromBody] ExamReasonRequest request, CancellationToken ct) =>
        string.IsNullOrWhiteSpace(request.Reason) ? BadRequest(Failure(400, "exams.reason_required", "Enter a rejection reason.")) :
        await ChangeSeriesStatus(id, ExamSeriesStatus.PendingApproval, ExamSeriesStatus.Draft, "Rejected", request.Reason, ct);

    [HttpPost("exam-series/{id:guid}/publish"), Authorize(Policy = SchoolPermissionPolicies.ExamsPublish)]
    public async Task<IActionResult> Publish(Guid id, CancellationToken ct)
    {
        await using var db = await RequireDb(ct); if (db is null) return Unauthorized();
        if (!await CanAccessSeries(db, id, ct)) return Forbid();
        var failure = await PublishCore(db, id, ct); return failure ?? Ok(ApiResponse<object?>.Success(null, correlationId: HttpContext.TraceIdentifier));
    }

    [HttpPost("exam-series/{id:guid}/cancel"), Authorize(Policy = SchoolPermissionPolicies.ExamsCancel)]
    public async Task<IActionResult> Cancel(Guid id, [FromBody] ExamReasonRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Reason)) return BadRequest(Failure(400, "exams.reason_required", "Enter a cancellation reason."));
        await using var db = await RequireDb(ct); if (db is null) return Unauthorized();
        var series = await LoadSeries(db, id, ct); if (series is null) return NotFound(Failure(404, "exams.not_found", "Exam series was not found."));
        if (!await CanAccessSeries(db, id, ct)) return Forbid();
        if (series.Status is ExamSeriesStatus.Closed or ExamSeriesStatus.Cancelled) return Conflict(Failure(409, "exams.cancel_not_allowed", "This exam cannot be cancelled."));
        var now = DateTimeOffset.UtcNow; series.Status = ExamSeriesStatus.Cancelled; series.CancelledByUserId = CurrentUserId();
        series.CancelledAtUtc = now; series.CancellationReason = Clean(request.Reason, 1000); series.UpdatedAtUtc = now;
        foreach (var window in series.ScheduleWindows) { window.Status = ExamScheduleWindowStatus.Cancelled; window.CancellationReason = series.CancellationReason; window.UpdatedAtUtc = now; }
        foreach (var sitting in series.Papers.SelectMany(x => x.Sittings)) { sitting.ExecutionStatus = ExamSittingExecutionStatus.Cancelled; sitting.UpdatedAtUtc = now; }
        foreach (var projection in series.ScheduleWindows.Where(x => x.CalendarProjection != null).Select(x => x.CalendarProjection!))
        { projection.SchoolCalendarEvent.IsActive = false; projection.SchoolCalendarEvent.UpdatedAtUtc = now; }
        await ExamWorkflowEngine.Cancelled(db, series, CurrentUserId(), ct);
        AddAudit(db, series, "Cancelled", now, request.Reason); await db.SaveChangesAsync(ct);
        return Ok(ApiResponse<object?>.Success(null, correlationId: HttpContext.TraceIdentifier));
    }

    [HttpGet("exam-series/{id:guid}/conflicts"), Authorize(Policy = SchoolPermissionPolicies.ExamsView)]
    public async Task<IActionResult> Conflicts(Guid id, CancellationToken ct)
    {
        await using var db = await RequireDb(ct); if (db is null) return Unauthorized();
        if (!await CanAccessSeries(db, id, ct)) return Forbid();
        var conflicts = await FindConflicts(db, id, ct);
        return Ok(ApiResponse<IReadOnlyList<ExamConflictResponse>>.Success(conflicts, correlationId: HttpContext.TraceIdentifier));
    }

    [HttpPost("exam-sittings/{id:guid}/start"), Authorize(Policy = SchoolPermissionPolicies.ExamsAttendanceRecord)]
    public async Task<IActionResult> StartSitting(Guid id, CancellationToken ct) => await ChangeSittingStatus(id,
        ExamSittingExecutionStatus.NotStarted, ExamSittingExecutionStatus.InProgress, "SittingStarted", ct);

    [HttpPost("exam-sittings/{id:guid}/complete"), Authorize(Policy = SchoolPermissionPolicies.ExamsAttendanceRecord)]
    public async Task<IActionResult> CompleteSitting(Guid id, CancellationToken ct)
    {
        await using var db = await RequireDb(ct); if (db is null) return Unauthorized();
        var sitting = await LoadSitting(db, id, ct); if (sitting is null) return NotFound(Failure(404, "exams.sitting_not_found", "Exam sitting was not found."));
        if (!await CanAccessSeries(db, sitting.ExamPaper.ExamSeriesId, ct)) return Forbid();
        if (sitting.ExecutionStatus != ExamSittingExecutionStatus.InProgress) return Conflict(Failure(409, "exams.sitting_not_in_progress", "Only an in-progress sitting can be completed."));
        if (sitting.CandidateAssignments.Any(x => x.Attendance == null || !x.Attendance.FinalizedAtUtc.HasValue))
            return Conflict(Failure(409, "exams.attendance_not_finalized", "Finalize attendance before completing the sitting."));
        var now = DateTimeOffset.UtcNow; sitting.ExecutionStatus = ExamSittingExecutionStatus.Completed; sitting.CompletedByUserId = CurrentUserId(); sitting.CompletedAtUtc = now; sitting.UpdatedAtUtc = now;
        AddAudit(db, sitting.ExamPaper.ExamSeries, "SittingCompleted", now, sittingId: sitting.Id); await db.SaveChangesAsync(ct);
        return Ok(ApiResponse<object?>.Success(null, correlationId: HttpContext.TraceIdentifier));
    }

    [HttpGet("exam-sittings/{id:guid}/roster"), Authorize(Policy = SchoolPermissionPolicies.ExamsView)]
    public async Task<IActionResult> Roster(Guid id, CancellationToken ct)
    {
        await using var db = await RequireDb(ct); if (db is null) return Unauthorized();
        var sitting = await db.ExamSittings.AsNoTracking().Where(x => x.Id == id).Select(x => new { x.ExamPaper.ExamSeriesId }).SingleOrDefaultAsync(ct);
        if (sitting is null) return NotFound(Failure(404, "exams.sitting_not_found", "Exam sitting was not found."));
        if (!await CanAccessSeries(db, sitting.ExamSeriesId, ct)) return Forbid();
        var rows = await db.ExamCandidateSittingAssignments.AsNoTracking().Where(x => x.ExamSittingId == id)
            .OrderBy(x => x.ExamPaperCandidate.ExamCandidate.NameArSnapshot).Select(x => new ExamRosterItemResponse(x.Id,
                x.ExamPaperCandidate.ExamCandidate.StudentId, x.ExamPaperCandidate.ExamCandidate.StudentCodeSnapshot,
                x.ExamPaperCandidate.ExamCandidate.NameArSnapshot, x.ExamPaperCandidate.ExamCandidate.NameEnSnapshot,
                x.Attendance == null ? ExamAttendanceStatus.NotRecorded : x.Attendance.Status,
                x.Attendance != null && x.Attendance.FinalizedAtUtc.HasValue, x.Attendance == null ? null : x.Attendance.Notes)).ToArrayAsync(ct);
        return Ok(ApiResponse<IReadOnlyList<ExamRosterItemResponse>>.Success(rows, correlationId: HttpContext.TraceIdentifier));
    }

    [HttpPut("exam-sittings/{id:guid}/attendance"), Authorize(Policy = SchoolPermissionPolicies.ExamsAttendanceRecord)]
    public async Task<IActionResult> SaveAttendance(Guid id, [FromBody] SaveExamAttendanceRequest request, CancellationToken ct)
    {
        await using var db = await RequireDb(ct); if (db is null) return Unauthorized();
        var sitting = await LoadSitting(db, id, ct); if (sitting is null) return NotFound(Failure(404, "exams.sitting_not_found", "Exam sitting was not found."));
        if (!await CanAccessSeries(db, sitting.ExamPaper.ExamSeriesId, ct)) return Forbid();
        if (sitting.ExecutionStatus != ExamSittingExecutionStatus.InProgress) return Conflict(Failure(409, "exams.sitting_not_in_progress", "Start the sitting before recording attendance."));
        if (request.Items is null || request.Items.Count == 0 || request.Items.Select(x => x.AssignmentId).Distinct().Count() != request.Items.Count)
            return BadRequest(Failure(400, "exams.attendance_invalid", "Submit one valid attendance state per candidate."));
        var ids = request.Items.Select(x => x.AssignmentId).ToArray(); var assignments = sitting.CandidateAssignments.Where(x => ids.Contains(x.Id)).ToArray();
        if (assignments.Length != ids.Length) return BadRequest(Failure(400, "exams.candidate_not_found", "One or more candidates do not belong to this sitting."));
        if (assignments.Any(x => x.Attendance?.FinalizedAtUtc != null)) return Conflict(Failure(409, "exams.attendance_finalized", "Reopen finalized attendance before editing it."));
        var now = DateTimeOffset.UtcNow; var userId = CurrentUserId();
        foreach (var item in request.Items)
        {
            if (!Enum.IsDefined(item.Status) || item.Status == ExamAttendanceStatus.NotRecorded || item.Notes?.Trim().Length > 2000)
                return BadRequest(Failure(400, "exams.attendance_invalid", "Check attendance states and notes."));
            var assignment = assignments.Single(x => x.Id == item.AssignmentId); var attendance = assignment.Attendance;
            if (attendance is null)
            {
                attendance = new ExamAttendance { Id = Guid.NewGuid(), ExamCandidateSittingAssignment = assignment, CreatedAtUtc = now };
                db.ExamAttendance.Add(attendance);
            }
            attendance.Status = item.Status; attendance.ArrivedAt = item.Status == ExamAttendanceStatus.Late ? item.ArrivedAt : null;
            attendance.Notes = Clean(item.Notes, 2000); attendance.RecordedByUserId = userId; attendance.RecordedAtUtc = now; attendance.UpdatedAtUtc = now;
        }
        AddAudit(db, sitting.ExamPaper.ExamSeries, "AttendanceSaved", now, sittingId: id,
            payload: JsonSerializer.Serialize(new { Count = request.Items.Count })); await db.SaveChangesAsync(ct);
        return Ok(ApiResponse<object>.Success(new { savedCount = request.Items.Count }, correlationId: HttpContext.TraceIdentifier));
    }

    [HttpPost("exam-sittings/{id:guid}/attendance/finalize"), Authorize(Policy = SchoolPermissionPolicies.ExamsAttendanceRecord)]
    public async Task<IActionResult> FinalizeAttendance(Guid id, CancellationToken ct)
    {
        await using var db = await RequireDb(ct); if (db is null) return Unauthorized(); var sitting = await LoadSitting(db, id, ct);
        if (sitting is null) return NotFound(Failure(404, "exams.sitting_not_found", "Exam sitting was not found."));
        if (!await CanAccessSeries(db, sitting.ExamPaper.ExamSeriesId, ct)) return Forbid();
        if (sitting.ExecutionStatus != ExamSittingExecutionStatus.InProgress) return Conflict(Failure(409, "exams.sitting_not_in_progress", "Start the sitting first."));
        if (sitting.CandidateAssignments.Count == 0 || sitting.CandidateAssignments.Any(x => x.Attendance == null || x.Attendance.Status == ExamAttendanceStatus.NotRecorded))
            return Conflict(Failure(409, "exams.attendance_incomplete", "Record attendance for every candidate first."));
        var now = DateTimeOffset.UtcNow; foreach (var attendance in sitting.CandidateAssignments.Select(x => x.Attendance!))
        { attendance.FinalizedByUserId = CurrentUserId(); attendance.FinalizedAtUtc = now; attendance.UpdatedAtUtc = now; }
        AddAudit(db, sitting.ExamPaper.ExamSeries, "AttendanceFinalized", now, sittingId: id); await db.SaveChangesAsync(ct);
        return Ok(ApiResponse<object?>.Success(null, correlationId: HttpContext.TraceIdentifier));
    }

    [HttpPost("exam-sittings/{id:guid}/attendance/reopen"), Authorize(Policy = SchoolPermissionPolicies.ExamsResultsReopen)]
    public async Task<IActionResult> ReopenAttendance(Guid id, [FromBody] ExamReasonRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Reason)) return BadRequest(Failure(400, "exams.reason_required", "Enter a reopen reason."));
        await using var db = await RequireDb(ct); if (db is null) return Unauthorized(); var sitting = await LoadSitting(db, id, ct);
        if (sitting is null) return NotFound(Failure(404, "exams.sitting_not_found", "Exam sitting was not found."));
        if (!await CanAccessSeries(db, sitting.ExamPaper.ExamSeriesId, ct)) return Forbid();
        var finalized = sitting.CandidateAssignments.Where(x => x.Attendance?.FinalizedAtUtc != null).Select(x => x.Attendance!).ToArray();
        if (finalized.Length == 0) return Conflict(Failure(409, "exams.attendance_not_finalized", "Attendance is not finalized."));
        var now = DateTimeOffset.UtcNow; foreach (var attendance in finalized) { attendance.FinalizedAtUtc = null; attendance.FinalizedByUserId = null; attendance.UpdatedAtUtc = now; }
        AddAudit(db, sitting.ExamPaper.ExamSeries, "AttendanceReopened", now, request.Reason, sittingId: id); await db.SaveChangesAsync(ct);
        return Ok(ApiResponse<object?>.Success(null, correlationId: HttpContext.TraceIdentifier));
    }

    [HttpGet("exam-papers/{id:guid}/results"), Authorize(Policy = SchoolPermissionPolicies.ExamsView)]
    public async Task<IActionResult> Results(Guid id, CancellationToken ct)
    {
        await using var db = await RequireDb(ct); if (db is null) return Unauthorized();
        var paper = await db.ExamPapers.AsNoTracking().SingleOrDefaultAsync(x => x.Id == id, ct);
        if (paper is null) return NotFound(Failure(404, "exams.paper_not_found", "Exam paper was not found."));
        if (!await CanAccessSeries(db, paper.ExamSeriesId, ct)) return Forbid();
        var rows = await db.ExamPaperCandidates.AsNoTracking().Where(x => x.ExamPaperId == id)
            .OrderBy(x => x.ExamCandidate.NameArSnapshot).Select(x => new ExamResultItemResponse(x.Id,
                x.ExamCandidate.StudentId, x.ExamCandidate.StudentCodeSnapshot, x.ExamCandidate.NameArSnapshot, x.ExamCandidate.NameEnSnapshot,
                x.ResultAttempts.OrderByDescending(a => a.AttemptNumber).Select(a => (Guid?)a.Id).FirstOrDefault(),
                x.ResultAttempts.OrderByDescending(a => a.AttemptNumber).Select(a => (ExamResultDisposition?)a.Disposition).FirstOrDefault() ?? ExamResultDisposition.Pending,
                x.ResultAttempts.OrderByDescending(a => a.AttemptNumber).Select(a => a.Score).FirstOrDefault(),
                x.ResultAttempts.OrderByDescending(a => a.AttemptNumber).Select(a => a.Notes).FirstOrDefault())).ToArrayAsync(ct);
        return Ok(ApiResponse<object>.Success(new { paper.ResultsStatus, paper.ResultsRevision, paper.TotalScore, items = rows }, correlationId: HttpContext.TraceIdentifier));
    }

    [HttpPut("exam-papers/{id:guid}/results"), Authorize(Policy = SchoolPermissionPolicies.ExamsResultsEnter)]
    public async Task<IActionResult> SaveResults(Guid id, [FromBody] SaveExamResultsRequest request, CancellationToken ct)
    {
        await using var db = await RequireDb(ct); if (db is null) return Unauthorized();
        var paper = await LoadPaper(db, id, ct); if (paper is null) return NotFound(Failure(404, "exams.paper_not_found", "Exam paper was not found."));
        if (!await CanAccessSeries(db, paper.ExamSeriesId, ct)) return Forbid();
        if (!SchoolExamRules.CanEnterResults(paper.ResultsStatus)) return Conflict(Failure(409, "exams.results_not_open", "Results are not open for entry."));
        if (request.Items is null || request.Items.Count == 0 || request.Items.Select(x => x.ExamPaperCandidateId).Distinct().Count() != request.Items.Count)
            return BadRequest(Failure(400, "exams.results_invalid", "Submit one valid result per candidate."));
        var ids = request.Items.Select(x => x.ExamPaperCandidateId).ToArray(); var candidates = paper.Candidates.Where(x => ids.Contains(x.Id)).ToArray();
        if (candidates.Length != ids.Length) return BadRequest(Failure(400, "exams.candidate_not_found", "One or more candidates do not belong to this paper."));
        var now = DateTimeOffset.UtcNow; var marker = CurrentUserId();
        foreach (var item in request.Items)
        {
            if (!SchoolExamRules.HasValidResult(item.Disposition, item.Score, paper.TotalScore) || item.Notes?.Trim().Length > 2000)
                return BadRequest(Failure(400, "exams.result_invalid", "Check result states, scores and notes."));
            var candidate = candidates.Single(x => x.Id == item.ExamPaperCandidateId); var attempt = candidate.ResultAttempts.OrderByDescending(x => x.AttemptNumber).FirstOrDefault();
            if (attempt is null)
            {
                attempt = new ExamResultAttempt { Id = Guid.NewGuid(), ExamPaperCandidate = candidate, AttemptNumber = 1, IsFinal = true, CreatedAtUtc = now };
                db.ExamResultAttempts.Add(attempt);
            }
            attempt.Disposition = item.Disposition; attempt.Score = item.Disposition == ExamResultDisposition.Scored ? item.Score : null;
            attempt.Notes = Clean(item.Notes, 2000); attempt.MarkerUserId = marker; attempt.MarkedAtUtc = now; attempt.UpdatedAtUtc = now;
        }
        AddAudit(db, paper.ExamSeries, "ResultsSaved", now, paperId: paper.Id,
            payload: JsonSerializer.Serialize(new { Count = request.Items.Count }));
        await db.SaveChangesAsync(ct); return Ok(ApiResponse<object>.Success(new { savedCount = request.Items.Count }, correlationId: HttpContext.TraceIdentifier));
    }

    [HttpPost("exam-papers/{id:guid}/results/submit"), Authorize(Policy = SchoolPermissionPolicies.ExamsResultsEnter)]
    public async Task<IActionResult> SubmitResults(Guid id, CancellationToken ct) => await ChangeResultsStatus(id,
        ExamResultsStatus.EntryOpen, ExamResultsStatus.PendingApproval, "ResultsSubmitted", null, ct);

    [HttpPost("exam-papers/{id:guid}/results/approve"), Authorize(Policy = SchoolPermissionPolicies.ExamsResultsApprove)]
    public async Task<IActionResult> ApproveResults(Guid id, CancellationToken ct) => await ChangeResultsStatus(id,
        ExamResultsStatus.PendingApproval, ExamResultsStatus.Approved, "ResultsApproved", null, ct);

    [HttpPost("exam-papers/{id:guid}/results/publish"), Authorize(Policy = SchoolPermissionPolicies.ExamsResultsPublish)]
    public async Task<IActionResult> PublishResults(Guid id, CancellationToken ct) => await ChangeResultsStatus(id,
        ExamResultsStatus.Approved, ExamResultsStatus.Published, "ResultsPublished", null, ct);

    [HttpPost("exam-papers/{id:guid}/results/reopen"), Authorize(Policy = SchoolPermissionPolicies.ExamsResultsReopen)]
    public async Task<IActionResult> ReopenResults(Guid id, [FromBody] ExamReasonRequest request, CancellationToken ct) =>
        string.IsNullOrWhiteSpace(request.Reason) ? BadRequest(Failure(400, "exams.reason_required", "Enter a reopen reason.")) :
        await ChangeResultsStatus(id, ExamResultsStatus.Published, ExamResultsStatus.EntryOpen, "ResultsReopened", request.Reason, ct, incrementRevision: true);

    private async Task<IActionResult?> PublishCore(SchoolsDbContext db, Guid id, CancellationToken ct, bool ownsTransaction = true)
    {
        var series = await LoadSeries(db, id, ct); if (series is null) return NotFound(Failure(404, "exams.not_found", "Exam series was not found."));
        if (!SchoolExamRules.CanPublish(series.Status)) return Conflict(Failure(409, "exams.publish_not_allowed", "Only a draft or approved exam can be published."));
        if (!await ExamWorkflowEngine.CanPublish(db, id, CurrentUserId(), ct)) return Conflict(Failure(409, "exams.workflow_publish_blocked", "Complete all workflow steps and publish as the assigned publisher."));
        if (series.Status == ExamSeriesStatus.Draft && !string.IsNullOrWhiteSpace(series.PolicySnapshotJson))
        {
            var approvalRequired = PolicyFlag(series, "DepartmentApprovalRequired") || PolicyFlag(series, "SchoolApprovalRequired");
            var creatorCanPublish = PolicyFlag(series, "TeacherCanPublishWithoutApproval") &&
                series.CreatedByUserId == CurrentUserId();
            if (approvalRequired && !creatorCanPublish)
                return Conflict(Failure(409, "exams.approval_required", "This exam must be approved before publishing."));
        }
        var conflicts = await FindConflicts(db, id, ct); if (conflicts.Any(x => x.Severity == "Blocker"))
            return Conflict(Failure(409, "exams.schedule_conflict", string.Join("; ", conflicts.Select(x => x.Code).Distinct())));
        if (series.Papers.Count == 0 || series.ScheduleWindows.Count == 0 || series.Papers.Any(x => x.Targets.Count == 0 || x.Sittings.Count == 0))
            return Conflict(Failure(409, "exams.incomplete", "Add at least one paper, class and sitting before publishing."));
        var now = DateTimeOffset.UtcNow;
        await using var tx = ownsTransaction ? await db.Database.BeginTransactionAsync(ct) : null;
        foreach (var paper in series.Papers)
        {
            var subject = await db.GradeSubjectOfferings.AsNoTracking().Where(x => x.Id == paper.GradeSubjectOfferingId)
                .Select(x => new { x.CurriculumGradeSubject.Subject.Code, x.CurriculumGradeSubject.Subject.NameAr,
                    x.CurriculumGradeSubject.Subject.NameEn, GradeAr = x.GradeOffering.GradeLevel.NameAr, GradeEn = x.GradeOffering.GradeLevel.NameEn }).SingleAsync(ct);
            paper.SubjectCodeSnapshot = subject.Code; paper.SubjectNameArSnapshot = subject.NameAr; paper.SubjectNameEnSnapshot = subject.NameEn;
            paper.GradeNameArSnapshot = subject.GradeAr; paper.GradeNameEnSnapshot = subject.GradeEn; paper.UpdatedAtUtc = now;
            var pendingCandidates = new List<(ExamPaperCandidate Candidate, Guid ClassSectionId)>();
            foreach (var target in paper.Targets.Where(x => x.ClassSectionId.HasValue))
            {
                var enrollments = await db.StudentEnrollments.AsNoTracking().Where(x => x.ClassSectionId == target.ClassSectionId &&
                        x.Status == StudentEnrollmentStatus.Active && x.Student.IsActive)
                    .Select(x => new { Enrollment = x, x.Student }).ToArrayAsync(ct);
                if (enrollments.Length == 0) continue;
                if (paper.Sittings.All(x => x.AdministrationMode == ExamAdministrationMode.InClass))
                {
                    var classSitting = paper.Sittings.SingleOrDefault(x => x.ExamScheduleWindow.Venues.Any(v => v.ClassSectionId == target.ClassSectionId));
                    var classVenue = classSitting?.Venues.SingleOrDefault(x => x.ExamWindowVenue.ClassSectionId == target.ClassSectionId);
                    if (classSitting is null || classVenue is null) return Conflict(Failure(409, "exams.venue_missing", "Configure an in-class venue for every target."));
                }
                foreach (var row in enrollments)
                {
                    var candidate = series.Candidates.SingleOrDefault(x => x.StudentId == row.Student.Id);
                    if (candidate is null)
                    {
                        candidate = new ExamCandidate { Id = Guid.NewGuid(), ExamSeries = series, StudentId = row.Student.Id,
                            StudentEnrollmentIdSnapshot = row.Enrollment.Id, GlobalStudentIdSnapshot = row.Student.GlobalStudentId,
                            GradeOfferingIdSnapshot = row.Enrollment.GradeOfferingId, ClassSectionIdSnapshot = row.Enrollment.ClassSectionId,
                            StudentCodeSnapshot = row.Student.StudentCode, NameArSnapshot = row.Student.FullNameAr, NameEnSnapshot = row.Student.FullNameEn,
                            ExamNumber = $"{series.Code}-{series.Candidates.Count + 1:D4}",
                            CreatedAtUtc = now, UpdatedAtUtc = now };
                        db.ExamCandidates.Add(candidate);
                    }
                    var paperCandidate = new ExamPaperCandidate { Id = Guid.NewGuid(), ExamPaper = paper, ExamCandidate = candidate, CreatedAtUtc = now, UpdatedAtUtc = now };
                    db.ExamPaperCandidates.Add(paperCandidate); pendingCandidates.Add((paperCandidate, target.ClassSectionId!.Value));
                }
            }
            if (pendingCandidates.Count == 0)
                return Conflict(Failure(409, "exams.audience_empty", "The exam paper has no active candidates in its target classes."));
            var primarySitting = paper.Sittings.OrderBy(x => x.ExamScheduleWindow.StartsAtUtc).First();
            var committeeVenues = primarySitting.Venues.Where(x => x.ExamWindowVenue.ExamCommitteeId.HasValue)
                .OrderBy(x => x.ExamWindowVenue.ExamCommittee!.SortOrder).ToArray();
            if (primarySitting.AdministrationMode == ExamAdministrationMode.Committee &&
                committeeVenues.Sum(x => x.ExamWindowVenue.CapacitySnapshot) < pendingCandidates.Count)
                return Conflict(Failure(409, "exams.committee_capacity_insufficient", "Committee capacity is lower than the candidate count."));
            var seat = 0;
            foreach (var item in pendingCandidates.OrderBy(x => x.Candidate.ExamCandidate.NameArSnapshot))
            {
                ExamSitting sitting; ExamSittingVenue sittingVenue;
                if (primarySitting.AdministrationMode == ExamAdministrationMode.Committee)
                {
                    sitting = primarySitting;
                    sittingVenue = committeeVenues.First(x => seat < committeeVenues.TakeWhile(v => v != x)
                        .Sum(v => v.ExamWindowVenue.CapacitySnapshot) + x.ExamWindowVenue.CapacitySnapshot);
                }
                else
                {
                    sitting = paper.Sittings.SingleOrDefault(x => x.ExamScheduleWindow.Venues.Any(v => v.ClassSectionId == item.ClassSectionId))!;
                    sittingVenue = sitting?.Venues.SingleOrDefault(x => x.ExamWindowVenue.ClassSectionId == item.ClassSectionId)!;
                    if (sitting is null || sittingVenue is null) return Conflict(Failure(409, "exams.venue_missing", "Configure an in-class venue for every target."));
                }
                seat++;
                var assignment = new ExamCandidateSittingAssignment { Id = Guid.NewGuid(), ExamPaperId = paper.Id,
                    ExamPaperCandidate = item.Candidate, ExamSitting = sitting, ExamSittingVenue = sittingVenue,
                    SeatNumber = primarySitting.AdministrationMode == ExamAdministrationMode.Committee ? seat : null,
                    DeskOrSeatLabel = primarySitting.AdministrationMode == ExamAdministrationMode.Committee ? seat.ToString() : null,
                    AssignedByUserId = CurrentUserId(), AssignedAtUtc = now };
                db.ExamCandidateSittingAssignments.Add(assignment);
                db.ExamAttendance.Add(new ExamAttendance { Id = Guid.NewGuid(), ExamCandidateSittingAssignment = assignment, CreatedAtUtc = now, UpdatedAtUtc = now });
                db.ExamResultAttempts.Add(new ExamResultAttempt { Id = Guid.NewGuid(), ExamPaperCandidate = item.Candidate,
                    ExamCandidateSittingAssignment = assignment, AttemptNumber = 1, IsFinal = true, CreatedAtUtc = now, UpdatedAtUtc = now });
            }
        }
        foreach (var window in series.ScheduleWindows)
        {
            window.Status = ExamScheduleWindowStatus.Scheduled; window.UpdatedAtUtc = now;
            var existingCode = $"EXAM-{series.Code}-{window.Id.ToString("N")[..6]}"; if (existingCode.Length > 50) existingCode = existingCode[..50];
            var calendar = new SchoolCalendarEvent { Id = Guid.NewGuid(), EducationProgramId = series.ProgramAcademicYear.EducationProgramId,
                ProgramAcademicYearId = series.ProgramAcademicYearId, Code = existingCode, NameAr = series.NameAr, NameEn = series.NameEn,
                EventType = SchoolCalendarEventType.Exam, StartDate = window.LocalDate, EndDate = window.LocalDate, IsActive = true,
                CreatedAtUtc = now, UpdatedAtUtc = now };
            db.SchoolCalendarEvents.Add(calendar); db.ExamCalendarProjections.Add(new ExamCalendarProjection { Id = Guid.NewGuid(),
                ExamScheduleWindow = window, SchoolCalendarEvent = calendar, LastProjectedAtUtc = now });
        }
        series.Status = ExamSeriesStatus.Published; series.PublishedByUserId = CurrentUserId(); series.PublishedAtUtc = now; series.UpdatedAtUtc = now;
        await ExamWorkflowEngine.Published(db, series, CurrentUserId(), ct);
        AddAudit(db, series, "Published", now, payload: JsonSerializer.Serialize(new { CandidateCount = series.Candidates.Count }));
        try
        {
            await db.SaveChangesAsync(ct);
            if (tx is not null) await tx.CommitAsync(ct);
        }
        catch (DbUpdateException)
        {
            if (tx is not null) await tx.RollbackAsync(ct);
            return Conflict(Failure(409, "exams.concurrency_conflict", "The exam changed while it was being published."));
        }
        return null;
    }

    private async Task<IReadOnlyList<ExamConflictResponse>> FindConflicts(SchoolsDbContext db, Guid seriesId, CancellationToken ct)
    {
        var series = await LoadSeries(db, seriesId, ct); if (series is null) return [];
        var result = new List<ExamConflictResponse>();
        foreach (var window in series.ScheduleWindows)
        {
            var closed = await db.SchoolCalendarEvents.AsNoTracking().AnyAsync(x => x.IsActive && !x.IsDeleted && x.IsSchoolClosed &&
                x.StartDate <= window.LocalDate && x.EndDate >= window.LocalDate &&
                (!x.ProgramAcademicYearId.HasValue || x.ProgramAcademicYearId == series.ProgramAcademicYearId), ct);
            if (closed) result.Add(new("exams.school_closed", "Blocker", window.Id, "The school is closed on this date."));
            var classIds = window.Venues.Where(x => x.ClassSectionId.HasValue).Select(x => x.ClassSectionId!.Value).Distinct().ToArray(); var roomIds = window.Venues.Where(x => x.RoomId.HasValue).Select(x => x.RoomId!.Value).Distinct().ToArray();
            if (await db.ExamWindowVenues.AsNoTracking().AnyAsync(v => v.ExamScheduleWindow.ExamSeriesId != seriesId &&
                    v.ExamScheduleWindow.ExamSeries.Status == ExamSeriesStatus.Published && v.ExamScheduleWindow.Status == ExamScheduleWindowStatus.Scheduled &&
                    v.ExamScheduleWindow.StartsAtUtc < window.EndsAtUtc && window.StartsAtUtc < v.ExamScheduleWindow.EndsAtUtc &&
                    v.ClassSectionId.HasValue && classIds.Contains(v.ClassSectionId.Value), ct))
                result.Add(new("exams.class_conflict", "Blocker", window.Id, "A class already has another exam at this time."));
            if (roomIds.Length != 0 && await db.ExamWindowVenues.AsNoTracking().AnyAsync(v => v.ExamScheduleWindow.ExamSeriesId != seriesId && v.RoomId.HasValue &&
                    roomIds.Contains(v.RoomId.Value) && v.ExamScheduleWindow.ExamSeries.Status == ExamSeriesStatus.Published &&
                    v.ExamScheduleWindow.Status == ExamScheduleWindowStatus.Scheduled && v.ExamScheduleWindow.StartsAtUtc < window.EndsAtUtc &&
                    window.StartsAtUtc < v.ExamScheduleWindow.EndsAtUtc, ct))
                result.Add(new("exams.room_conflict", "Blocker", window.Id, "A room is already booked for another exam."));
            var studentIds = await db.StudentEnrollments.AsNoTracking().Where(x => classIds.Contains(x.ClassSectionId) &&
                x.Status == StudentEnrollmentStatus.Active && x.Student.IsActive).Select(x => x.StudentId).Distinct().ToArrayAsync(ct);
            if (studentIds.Length != 0 && await db.ExamCandidates.AsNoTracking().AnyAsync(c => c.ExamSeriesId != seriesId && studentIds.Contains(c.StudentId) &&
                    c.ExamSeries.Status == ExamSeriesStatus.Published && c.ExamSeries.ScheduleWindows.Any(other =>
                        other.Status == ExamScheduleWindowStatus.Scheduled && other.StartsAtUtc < window.EndsAtUtc && window.StartsAtUtc < other.EndsAtUtc), ct))
                result.Add(new("exams.student_conflict", "Blocker", window.Id, "One or more students already have another exam at this time."));
        }
        return result;
    }

    private async Task<IActionResult> ChangeSeriesStatus(Guid id, ExamSeriesStatus expected, ExamSeriesStatus next,
        string action, string? reason, CancellationToken ct)
    {
        await using var db = await RequireDb(ct); if (db is null) return Unauthorized(); var series = await LoadSeries(db, id, ct);
        if (series is null) return NotFound(Failure(404, "exams.not_found", "Exam series was not found."));
        if (!await CanAccessSeries(db, id, ct)) return Forbid(); if (series.Status != expected) return Conflict(Failure(409, "exams.transition_invalid", "The exam is not in the required state."));
        var workflow = await db.ExamWorkflows.Include(x => x.Steps).Include(x => x.Series).SingleOrDefaultAsync(x => x.ExamSeriesId == id, ct);
        if (workflow is null) return Conflict(Failure(409, "exams.workflow_required", "Configure workflow responsibilities first."));
        if (next == ExamSeriesStatus.PendingApproval) {
            if (workflow.Status is not (ExamWorkflowStatus.Configured or ExamWorkflowStatus.Returned)) return WorkflowConflict();
            workflow.Revision++; workflow.Status = ExamWorkflowStatus.Active; workflow.UpdatedAtUtc = DateTimeOffset.UtcNow;
            ExamWorkflowEngine.Advance(db, workflow, CurrentUserId()); AddAudit(db, series, "WorkflowStarted", workflow.UpdatedAtUtc);
            return await SaveWorkflow(db, ct);
        }
        var step = workflow.Steps.FirstOrDefault(x => x.Status == ExamWorkflowStepStatus.Active && x.AssigneeUserId == CurrentUserId() && (x.Stage == ExamWorkflowStage.DepartmentReview || x.Stage == ExamWorkflowStage.SchoolApproval));
        if (step is null) return Forbid();
        return await WorkflowAction(id, step.Id, next == ExamSeriesStatus.Approved ? "complete" : "return", new WorkflowActionRequest(workflow.Revision, reason, null), ct);
    }

    private async Task<IActionResult> ChangeSittingStatus(Guid id, ExamSittingExecutionStatus expected,
        ExamSittingExecutionStatus next, string action, CancellationToken ct)
    {
        await using var db = await RequireDb(ct); if (db is null) return Unauthorized(); var sitting = await LoadSitting(db, id, ct);
        if (sitting is null) return NotFound(Failure(404, "exams.sitting_not_found", "Exam sitting was not found."));
        if (!await CanAccessSeries(db, sitting.ExamPaper.ExamSeriesId, ct)) return Forbid();
        if (sitting.ExamPaper.ExamSeries.Status != ExamSeriesStatus.Published || sitting.ExamScheduleWindow.Status != ExamScheduleWindowStatus.Scheduled || sitting.ExecutionStatus != expected)
            return Conflict(Failure(409, "exams.sitting_transition_invalid", "The sitting is not in the required state."));
        var now = DateTimeOffset.UtcNow; sitting.ExecutionStatus = next; sitting.UpdatedAtUtc = now;
        if (next == ExamSittingExecutionStatus.InProgress) { sitting.StartedByUserId = CurrentUserId(); sitting.StartedAtUtc = now; sitting.ExamPaper.ResultsStatus = ExamResultsStatus.EntryOpen; }
        AddAudit(db, sitting.ExamPaper.ExamSeries, action, now, sittingId: id); await db.SaveChangesAsync(ct);
        return Ok(ApiResponse<object?>.Success(null, correlationId: HttpContext.TraceIdentifier));
    }

    private async Task<IActionResult> ChangeResultsStatus(Guid id, ExamResultsStatus expected, ExamResultsStatus next,
        string action, string? reason, CancellationToken ct, bool incrementRevision = false)
    {
        await using var db = await RequireDb(ct); if (db is null) return Unauthorized(); var paper = await LoadPaper(db, id, ct);
        if (paper is null) return NotFound(Failure(404, "exams.paper_not_found", "Exam paper was not found."));
        if (!await CanAccessSeries(db, paper.ExamSeriesId, ct)) return Forbid();
        if (paper.ResultsStatus != expected) return Conflict(Failure(409, "exams.results_transition_invalid", "Results are not in the required state."));
        if (next == ExamResultsStatus.PendingApproval && paper.Candidates.Any(x => x.ResultAttempts.All(a => a.Disposition == ExamResultDisposition.Pending)))
            return Conflict(Failure(409, "exams.results_incomplete", "Enter a final state or score for every candidate first."));
        var effectiveNext = next == ExamResultsStatus.PendingApproval &&
            !PolicyFlag(paper.ExamSeries, "ResultApprovalRequired") ? ExamResultsStatus.Approved : next;
        var now = DateTimeOffset.UtcNow; paper.ResultsStatus = effectiveNext; paper.UpdatedAtUtc = now; if (incrementRevision) paper.ResultsRevision++;
        foreach (var attempt in paper.Candidates.SelectMany(x => x.ResultAttempts).Where(x => x.IsFinal))
        {
            if (effectiveNext == ExamResultsStatus.Approved) { attempt.ApprovedByUserId = CurrentUserId(); attempt.ApprovedAtUtc = now; }
            if (effectiveNext == ExamResultsStatus.Published) attempt.PublishedAtUtc = now;
        }
        var effectiveAction = effectiveNext == ExamResultsStatus.Approved && next == ExamResultsStatus.PendingApproval
            ? "ResultsSubmittedAndApproved" : action;
        AddAudit(db, paper.ExamSeries, effectiveAction, now, reason, paper.Id);
        if (effectiveNext == ExamResultsStatus.Published)
        {
            var hasUnpublishedPaper = await db.ExamPapers.AsNoTracking().AnyAsync(x => x.ExamSeriesId == paper.ExamSeriesId &&
                x.Id != paper.Id && x.ResultsStatus != ExamResultsStatus.Published, ct);
            var hasOpenSitting = await db.ExamSittings.AsNoTracking().AnyAsync(x => x.ExamPaper.ExamSeriesId == paper.ExamSeriesId &&
                x.ExecutionStatus != ExamSittingExecutionStatus.Completed && x.ExecutionStatus != ExamSittingExecutionStatus.Cancelled, ct);
            if (!hasUnpublishedPaper && !hasOpenSitting)
            {
                paper.ExamSeries.Status = ExamSeriesStatus.Closed;
                paper.ExamSeries.ClosedByUserId = CurrentUserId();
                paper.ExamSeries.ClosedAtUtc = now;
                paper.ExamSeries.UpdatedAtUtc = now;
                AddAudit(db, paper.ExamSeries, "SeriesClosed", now,
                    payload: JsonSerializer.Serialize(new { Reason = "AllSittingsCompletedAndResultsPublished" }));
            }
        }
        else if (incrementRevision && paper.ExamSeries.Status == ExamSeriesStatus.Closed)
        {
            paper.ExamSeries.Status = ExamSeriesStatus.Published;
            paper.ExamSeries.ClosedByUserId = null;
            paper.ExamSeries.ClosedAtUtc = null;
            paper.ExamSeries.UpdatedAtUtc = now;
            AddAudit(db, paper.ExamSeries, "SeriesReopened", now, reason, paper.Id);
        }
        await db.SaveChangesAsync(ct);
        return Ok(ApiResponse<object?>.Success(null, correlationId: HttpContext.TraceIdentifier));
    }

    private static bool PolicyFlag(ExamSeries series, string propertyName)
    {
        if (string.IsNullOrWhiteSpace(series.PolicySnapshotJson)) return false;
        try
        {
            using var policy = JsonDocument.Parse(series.PolicySnapshotJson);
            return policy.RootElement.TryGetProperty(propertyName, out var value) &&
                value.ValueKind is JsonValueKind.True or JsonValueKind.False && value.GetBoolean();
        }
        catch (JsonException)
        {
            return false;
        }
    }

    private async Task<(IActionResult? Result, QuickExamSubject? Subject, QuickExamSection? Section)> ValidateQuickRequest(
        SchoolsDbContext db, Guid classSectionId, SaveQuickExamRequest request, CancellationToken ct)
    {
        if (request.Kind is ExamKind.TermFinal or ExamKind.YearFinal)
            return (BadRequest(Failure(400, "exams.central_creation_required",
                "Term-final and year-final exams must be created from the exam center.")), null, null);
        if (!Enum.IsDefined(request.Kind) || !Enum.IsDefined(request.Format) || !Enum.IsDefined(request.IssuingAuthority) ||
            !Enum.IsDefined(request.SchedulingAuthority) || (request.Kind != ExamKind.Monthly &&
            (string.IsNullOrWhiteSpace(request.TitleAr) || request.TitleAr.Trim().Length > 250)) ||
            request.TitleEn?.Trim().Length > 250 || request.Instructions?.Trim().Length > 6000 || request.DurationMinutes is < 5 or > 600 ||
            !SchoolExamRules.HasValidScoreDefinition(request.TotalScore, request.PassScore))
            return (BadRequest(Failure(400, "exams.invalid", "Enter valid exam data.")), null, null);
        if (request.IssuingAuthority == ExamAuthorityType.EducationalAuthority &&
            (string.IsNullOrWhiteSpace(request.ExternalSourceCode) || string.IsNullOrWhiteSpace(request.ExternalReferenceId)))
            return (BadRequest(Failure(400, "exams.external_reference_required", "Enter the educational authority source and reference.")), null, null);
        var section = await db.ClassSections.AsNoTracking().Where(x => x.Id == classSectionId && x.IsActive && !x.IsDeleted)
            .Select(x => new QuickExamSection(x.GradeOfferingId, x.GradeOffering.ProgramAcademicYearId,
                x.GradeOffering.ProgramAcademicYear.EducationProgramId, x.GradeOffering.GradeLevel.EducationStageId, x.Capacity,
                x.GradeOffering.ProgramAcademicYear.StartDate, x.GradeOffering.ProgramAcademicYear.EndDate)).SingleOrDefaultAsync(ct);
        if (section is null) return (NotFound(Failure(404, "exams.class_not_found", "Class section was not found.")), null, null);
        if (request.ExamDate < section.StartDate || request.ExamDate > section.EndDate)
            return (BadRequest(Failure(400, "exams.date_outside_year", "The exam date must be inside the academic year.")), null, null);
        var subject = await db.ClassSectionSubjects.AsNoTracking().Where(x => x.Id == request.ClassSectionSubjectId && x.ClassSectionId == classSectionId &&
                x.IsActive && !x.IsDeleted && x.GradeSubjectOffering.IsActive && !x.GradeSubjectOffering.IsDeleted)
            .Select(x => new QuickExamSubject(x.Id, x.GradeSubjectOfferingId, x.GradeSubjectOffering.CurriculumGradeSubject.SubjectId,
                x.GradeSubjectOffering.CurriculumGradeSubject.Subject.NameAr,
                x.GradeSubjectOffering.CurriculumGradeSubject.Subject.NameEn)).SingleOrDefaultAsync(ct);
        if (subject is null) return (BadRequest(Failure(400, "exams.subject_invalid", "Select an active subject from this class.")), null, null);
        if (!await CanAccessSubject(db, classSectionId, subject.Id, subject.SubjectId, ct)) return (Forbid(), null, null);
        return (null, subject, section);
    }

    private IQueryable<ExamPaper> ApplyScope(SchoolsDbContext db, IQueryable<ExamPaper> query)
    {
        if (User.HasClaim(SchoolClaimTypes.Role, SchoolIdentitySeed.SchoolAdminRoleCode)) return query;
        var userId = CurrentUserId(); var today = DateOnly.FromDateTime(DateTime.UtcNow);
        return query.Where(paper => paper.Targets.Any(target => target.ClassSectionId.HasValue &&
                db.ClassSectionTeacherScopes.Any(scope => scope.ClassSectionId == target.ClassSectionId && scope.IsActive && !scope.IsDeleted &&
                    scope.TeacherGradeSubjectScope.IsActive && !scope.TeacherGradeSubjectScope.IsDeleted && scope.TeacherGradeSubjectScope.TeacherUserId == userId &&
                    scope.TeacherGradeSubjectScope.GradeSubjectOfferingId == paper.GradeSubjectOfferingId)) ||
            db.DepartmentLeaderships.Any(lead => lead.UserId == userId && lead.IsActive && !lead.IsDeleted && lead.StartsOn <= today &&
                (!lead.EndsOn.HasValue || lead.EndsOn >= today) && lead.Department.Subjects.Any(subject => subject.IsActive && !subject.IsDeleted &&
                    subject.SubjectId == paper.GradeSubjectOffering.CurriculumGradeSubject.SubjectId)) ||
            db.SubjectCoordinatorAssignments.Any(coordinator => coordinator.CoordinatorUserId == userId && coordinator.IsActive && !coordinator.IsDeleted &&
                coordinator.StartsOn <= today && (!coordinator.EndsOn.HasValue || coordinator.EndsOn >= today) &&
                coordinator.DepartmentSubject.SubjectId == paper.GradeSubjectOffering.CurriculumGradeSubject.SubjectId));
    }

    private static IQueryable<ExamListItemResponse> ToList(IQueryable<ExamPaper> query) => query
        .OrderByDescending(x => x.Sittings.Select(s => s.ExamScheduleWindow.LocalDate).FirstOrDefault()).ThenByDescending(x => x.CreatedAtUtc).ThenByDescending(x => x.Id)
        .Select(x => new ExamListItemResponse(x.ExamSeriesId, x.Id, x.Sittings.Select(s => (Guid?)s.Id).FirstOrDefault(),
            x.TitleAr, x.TitleEn, x.SubjectNameArSnapshot ?? x.GradeSubjectOffering.CurriculumGradeSubject.Subject.NameAr,
            x.SubjectNameEnSnapshot ?? x.GradeSubjectOffering.CurriculumGradeSubject.Subject.NameEn, x.ExamSeries.Kind,
            x.ExamSeries.Status, x.ResultsStatus, x.Sittings.Select(s => (DateOnly?)s.ExamScheduleWindow.LocalDate).FirstOrDefault(),
            x.Sittings.Select(s => (TimeOnly?)s.ExamScheduleWindow.StartsAtLocal).FirstOrDefault(), x.DurationMinutes, x.TotalScore,
            x.Candidates.Count, x.Candidates.Count(c => c.SittingAssignments.Any(a => a.Attendance != null && a.Attendance.Status != ExamAttendanceStatus.NotRecorded)),
            x.Candidates.Count(c => c.ResultAttempts.Any(a => a.Disposition != ExamResultDisposition.Pending)),
            x.Sittings.Select(s => (ExamSittingExecutionStatus?)s.ExecutionStatus).FirstOrDefault()));

    private static ExamSeriesDetailsResponse ToDetails(ExamSeries x) => new(x.Id, x.Code, x.NameAr, x.NameEn, x.Kind,
        x.IssuingAuthority, x.SchedulingAuthority, x.Status, x.TimeZoneIdSnapshot, x.ExternalAuthorityName, x.ExternalReferenceId,
        x.IsExternalScheduleLocked, x.Papers.Select(p => new ExamPaperDetailsResponse(p.Id, p.TitleAr, p.TitleEn,
            p.SubjectNameArSnapshot ?? p.GradeSubjectOffering.CurriculumGradeSubject.Subject.NameAr,
            p.SubjectNameEnSnapshot ?? p.GradeSubjectOffering.CurriculumGradeSubject.Subject.NameEn, p.Format, p.TotalScore,
            p.PassScore, p.DurationMinutes, p.ResultsStatus, p.ResultsRevision, p.Sittings.Select(s => new ExamSittingDetailsResponse(s.Id,
                s.ExamScheduleWindow.LocalDate, s.ExamScheduleWindow.StartsAtLocal, s.ExamScheduleWindow.EndsAtLocal,
                s.ExecutionStatus, s.AdministrationMode, s.CandidateAssignments.Count)).ToArray())).ToArray(),
        x.CreatedAtUtc, x.PublishedAtUtc, x.CancelledAtUtc, x.CancellationReason);

    private async Task<ExamSeries?> LoadSeries(SchoolsDbContext db, Guid id, CancellationToken ct) => await db.ExamSeries.IgnoreQueryFilters()
        .Include(x => x.ProgramAcademicYear).Include(x => x.Targets).Include(x => x.AuditTrail)
        .Include(x => x.Candidates).Include(x => x.Papers).ThenInclude(x => x.GradeSubjectOffering).ThenInclude(x => x.CurriculumGradeSubject).ThenInclude(x => x.Subject)
        .Include(x => x.Papers).ThenInclude(x => x.Targets).Include(x => x.Papers).ThenInclude(x => x.Sittings).ThenInclude(x => x.Venues)
        .Include(x => x.ScheduleWindows).ThenInclude(x => x.Venues).Include(x => x.ScheduleWindows).ThenInclude(x => x.Sittings)
        .Include(x => x.Committees).ThenInclude(x => x.Venues)
        .Include(x => x.Committees).ThenInclude(x => x.Invigilators).ThenInclude(x => x.User).ThenInclude(x => x.Person)
        .Include(x => x.ScheduleWindows).ThenInclude(x => x.CalendarProjection!).ThenInclude(x => x.SchoolCalendarEvent)
        .SingleOrDefaultAsync(x => x.Id == id, ct);

    private static Task<ExamSitting?> LoadSitting(SchoolsDbContext db, Guid id, CancellationToken ct) => db.ExamSittings
        .Include(x => x.ExamScheduleWindow).Include(x => x.ExamPaper).ThenInclude(x => x.ExamSeries)
        .Include(x => x.CandidateAssignments).ThenInclude(x => x.Attendance)
        .SingleOrDefaultAsync(x => x.Id == id, ct);

    private static Task<ExamPaper?> LoadPaper(SchoolsDbContext db, Guid id, CancellationToken ct) => db.ExamPapers
        .Include(x => x.ExamSeries).Include(x => x.Candidates).ThenInclude(x => x.ResultAttempts)
        .SingleOrDefaultAsync(x => x.Id == id, ct);

    private async Task<bool> CanAccessSeries(SchoolsDbContext db, Guid seriesId, CancellationToken ct)
    {
        if (await IsAdmin(db, ct)) return true; return await ApplyScope(db, db.ExamPapers.AsNoTracking()).AnyAsync(x => x.ExamSeriesId == seriesId, ct);
    }

    private async Task<bool> CanAccessClass(SchoolsDbContext db, Guid classSectionId, CancellationToken ct)
    {
        if (await IsAdmin(db, ct)) return true; var userId = CurrentUserId(); var today = DateOnly.FromDateTime(DateTime.UtcNow);
        return await db.ClassSectionTeacherScopes.AsNoTracking().AnyAsync(x => x.ClassSectionId == classSectionId && x.IsActive && !x.IsDeleted &&
            x.TeacherGradeSubjectScope.IsActive && !x.TeacherGradeSubjectScope.IsDeleted && x.TeacherGradeSubjectScope.TeacherUserId == userId, ct) ||
            await db.DepartmentLeaderships.AsNoTracking().AnyAsync(x => x.UserId == userId && x.IsActive && !x.IsDeleted && x.StartsOn <= today &&
                (!x.EndsOn.HasValue || x.EndsOn >= today), ct);
    }

    private async Task<bool> CanAccessSubject(SchoolsDbContext db, Guid classSectionId, Guid classSectionSubjectId, Guid subjectId, CancellationToken ct)
    {
        if (await IsAdmin(db, ct)) return true; var userId = CurrentUserId(); var today = DateOnly.FromDateTime(DateTime.UtcNow);
        return await db.ClassSectionTeacherScopes.AsNoTracking().AnyAsync(x => x.ClassSectionId == classSectionId && x.IsActive && !x.IsDeleted &&
            x.TeacherGradeSubjectScope.IsActive && !x.TeacherGradeSubjectScope.IsDeleted && x.TeacherGradeSubjectScope.TeacherUserId == userId &&
            x.TeacherGradeSubjectScope.GradeSubjectOfferingId == db.ClassSectionSubjects.Where(s => s.Id == classSectionSubjectId).Select(s => s.GradeSubjectOfferingId).First(), ct) ||
            await db.DepartmentLeaderships.AsNoTracking().AnyAsync(x => x.UserId == userId && x.IsActive && !x.IsDeleted && x.StartsOn <= today &&
                (!x.EndsOn.HasValue || x.EndsOn >= today) && x.Department.Subjects.Any(s => s.IsActive && !s.IsDeleted && s.SubjectId == subjectId), ct) ||
            await db.SubjectCoordinatorAssignments.AsNoTracking().AnyAsync(x => x.CoordinatorUserId == userId && x.IsActive && !x.IsDeleted &&
                x.StartsOn <= today && (!x.EndsOn.HasValue || x.EndsOn >= today) && x.DepartmentSubject.SubjectId == subjectId, ct);
    }

    private static Task<bool> ClassExists(SchoolsDbContext db, Guid id, CancellationToken ct) => db.ClassSections.AsNoTracking().AnyAsync(x => x.Id == id && x.IsActive && !x.IsDeleted, ct);
    private Task<bool> IsAdmin(SchoolsDbContext db, CancellationToken ct) => db.LocalUserRoles.AsNoTracking().AnyAsync(x => x.UserId == CurrentUserId() && x.RoleId == SchoolIdentitySeed.SchoolAdminRoleId && x.Role.IsActive, ct);
    private async Task<SchoolsDbContext?> RequireDb(CancellationToken ct) => await dbFactory.CreateAsync(User.FindFirst(SchoolClaimTypes.SchoolCode)?.Value ?? string.Empty, ct);
    private Guid CurrentUserId() => Guid.TryParse(User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value, out var id) ? id : Guid.Empty;
    private ApiResponse<object?> Failure(int status, string code, string message) => ApiResponse<object?>.Failure(status, code, message, correlationId: HttpContext.TraceIdentifier);
    private void AddAudit(SchoolsDbContext db, ExamSeries series, string action, DateTimeOffset now, string? reason = null,
        Guid? paperId = null, Guid? sittingId = null, string? payload = null) => db.ExamAudits.Add(new ExamAudit { Id = Guid.NewGuid(),
            ExamSeries = series, ExamPaperId = paperId, ExamSittingId = sittingId, Action = action, ActorUserId = CurrentUserId(),
            Reason = Clean(reason, 1000), PayloadJson = payload, CreatedAtUtc = now });
    private static DateTimeOffset ToUtc(DateOnly date, TimeOnly time, TimeZoneInfo zone)
    {
        var local = DateTime.SpecifyKind(date.ToDateTime(time), DateTimeKind.Unspecified);
        if (zone.IsInvalidTime(local)) throw new ArgumentException("Invalid local time.");
        return new DateTimeOffset(TimeZoneInfo.ConvertTimeToUtc(local, zone), TimeSpan.Zero);
    }
    private static string ExamCode(DateTimeOffset now) => $"EX-{now:yyyyMMddHHmmss}-{Guid.NewGuid().ToString("N")[..6]}";
    private static string ArabicMonth(int month) => new[] { "", "يناير", "فبراير", "مارس", "أبريل", "مايو", "يونيو",
        "يوليو", "أغسطس", "سبتمبر", "أكتوبر", "نوفمبر", "ديسمبر" }[month];
    private static string EnglishMonth(int month) => new DateTime(2000, month, 1).ToString("MMMM", CultureInfo.InvariantCulture);
    private static string? Clean(string? value, int max) => string.IsNullOrWhiteSpace(value) ? null : value.Trim()[..Math.Min(value.Trim().Length, max)];
}

public sealed record SaveQuickExamRequest(Guid ClassSectionSubjectId, ExamKind Kind, string TitleAr, string? TitleEn,
    ExamFormat Format, string? Instructions, decimal TotalScore, decimal? PassScore, DateOnly ExamDate,
    [property: JsonConverter(typeof(FlexibleTimeOnlyJsonConverter))] TimeOnly StartsAt,
    int DurationMinutes, ExamAuthorityType IssuingAuthority, ExamAuthorityType SchedulingAuthority, bool PublishImmediately,
    string? ExternalSourceCode, string? ExternalAuthorityName, string? ExternalReferenceId, string? ExternalRevision,
    bool IsExternalScheduleLocked, Guid? AcademicTermId, DateOnly? AssessmentMonth);
public sealed record ExamSubjectResponse(Guid Id, Guid GradeSubjectOfferingId, string NameAr, string NameEn);
public sealed record ClassExamPeriodOptionsResponse(Guid ProgramAcademicYearId, IReadOnlyList<ClassExamTermOption> Terms);
public sealed record ClassExamTermOption(Guid Id, string NameAr, string NameEn, DateOnly StartDate, DateOnly EndDate, int SortOrder);
public sealed record ExamListItemResponse(Guid SeriesId, Guid PaperId, Guid? SittingId, string TitleAr, string TitleEn,
    string SubjectNameAr, string SubjectNameEn, ExamKind Kind, ExamSeriesStatus SeriesStatus, ExamResultsStatus ResultsStatus,
    DateOnly? ExamDate, TimeOnly? StartsAt, int DurationMinutes, decimal TotalScore, int CandidateCount,
    int AttendanceRecordedCount, int ResultsRecordedCount, ExamSittingExecutionStatus? SittingStatus);
public sealed record ExamCenterSummaryResponse(int TotalPapers, int PublishedPapers, int CandidateCount,
    IReadOnlyList<ExamKindSummaryResponse> Kinds);
public sealed record ExamKindSummaryResponse(ExamKind Kind, int PaperCount, int PublishedCount, int CandidateCount);
public sealed record ExamSeriesDetailsResponse(Guid Id, string Code, string NameAr, string NameEn, ExamKind Kind,
    ExamAuthorityType IssuingAuthority, ExamAuthorityType SchedulingAuthority, ExamSeriesStatus Status, string TimeZoneId,
    string? ExternalAuthorityName, string? ExternalReferenceId, bool IsExternalScheduleLocked, IReadOnlyList<ExamPaperDetailsResponse> Papers,
    DateTimeOffset CreatedAtUtc, DateTimeOffset? PublishedAtUtc, DateTimeOffset? CancelledAtUtc, string? CancellationReason);
public sealed record ExamPaperDetailsResponse(Guid Id, string TitleAr, string TitleEn, string SubjectNameAr, string SubjectNameEn,
    ExamFormat Format, decimal TotalScore, decimal? PassScore, int DurationMinutes, ExamResultsStatus ResultsStatus,
    int ResultsRevision, IReadOnlyList<ExamSittingDetailsResponse> Sittings);
public sealed record ExamSittingDetailsResponse(Guid Id, DateOnly Date, TimeOnly StartsAt, TimeOnly EndsAt,
    ExamSittingExecutionStatus Status, ExamAdministrationMode AdministrationMode, int CandidateCount);
public sealed record ExamRosterItemResponse(Guid AssignmentId, Guid StudentId, string StudentCode, string FullNameAr,
    string FullNameEn, ExamAttendanceStatus Status, bool IsFinalized, string? Notes);
public sealed record SaveExamAttendanceRequest(IReadOnlyList<SaveExamAttendanceItem> Items);
public sealed record SaveExamAttendanceItem(Guid AssignmentId, ExamAttendanceStatus Status, TimeOnly? ArrivedAt, string? Notes);
public sealed record ExamResultItemResponse(Guid ExamPaperCandidateId, Guid StudentId, string StudentCode, string FullNameAr,
    string FullNameEn, Guid? AttemptId, ExamResultDisposition Disposition, decimal? Score, string? Notes);
public sealed record SaveExamResultsRequest(IReadOnlyList<SaveExamResultItem> Items);
public sealed record SaveExamResultItem(Guid ExamPaperCandidateId, ExamResultDisposition Disposition, decimal? Score, string? Notes);
public sealed record ExamReasonRequest(string Reason);
public sealed record ExamConflictResponse(string Code, string Severity, Guid WindowId, string Message);
internal sealed record QuickExamSubject(Guid Id, Guid GradeSubjectOfferingId, Guid SubjectId, string NameAr, string NameEn);
internal sealed record QuickExamSection(Guid GradeOfferingId, Guid ProgramAcademicYearId, Guid EducationProgramId,
    Guid EducationStageId, int Capacity, DateOnly StartDate, DateOnly EndDate);
