using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Mdaresna.Api.Contracts;
using Mdaresna.Schools.Api.Auth;
using Mdaresna.Schools.Api.Exams;
using Mdaresna.Schools.Api.Time;
using Mdaresna.Schools.Domain.Academics;
using Mdaresna.Schools.Domain.Exams;
using Mdaresna.Schools.Domain.Identity;
using Mdaresna.Schools.Infrastructure.Identity;
using Mdaresna.Schools.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Mdaresna.Schools.Api.Controllers;

[ApiController, Authorize, Route("api/schools/v1/exam-management")]
public sealed class SchoolExamManagementController(ISchoolDbContextFactory dbFactory) : ControllerBase
{
    [HttpGet("options"), Authorize(Policy = SchoolPermissionPolicies.ExamsView)]
    public async Task<IActionResult> Options(CancellationToken ct)
    {
        await using var db = await RequireDb(ct); if (db is null) return Unauthorized();
        var programs = await db.EducationPrograms.AsNoTracking().Where(x => x.IsActive && !x.IsDeleted)
            .OrderBy(x => x.NameAr).Select(x => new Option(x.Id, x.NameAr, x.NameEn, x.Code)).ToArrayAsync(ct);
        var years = await db.ProgramAcademicYears.AsNoTracking().Where(x => x.IsActive && !x.IsDeleted)
            .OrderByDescending(x => x.StartDate).Select(x => new YearOption(x.Id, x.EducationProgramId,
                x.NameAr, x.NameEn, x.Code, x.StartDate, x.EndDate)).ToArrayAsync(ct);
        var rawTerms = await db.AcademicTerms.AsNoTracking().Where(x => x.IsActive && !x.IsDeleted)
            .OrderBy(x => x.SortOrder).Select(x => new { x.Id, x.ProgramAcademicYearId, x.NameAr, x.NameEn,
                x.StartDate, x.EndDate, x.SortOrder }).ToArrayAsync(ct);
        var lastTermByYear = rawTerms.GroupBy(x => x.ProgramAcademicYearId).ToDictionary(x => x.Key, x => x.Max(t => t.SortOrder));
        var terms = rawTerms.Select(x => new TermOption(x.Id, x.ProgramAcademicYearId, x.NameAr, x.NameEn,
            x.StartDate, x.EndDate, x.SortOrder, x.SortOrder == lastTermByYear[x.ProgramAcademicYearId])).ToArray();
        var stages = await db.EducationStages.AsNoTracking().Where(x => x.IsActive && !x.IsDeleted)
            .OrderBy(x => x.SortOrder).Select(x => new StageOption(x.Id, x.EducationProgramId, x.NameAr, x.NameEn)).ToArrayAsync(ct);
        var grades = await db.GradeOfferings.AsNoTracking().Where(x => x.IsActive && !x.IsDeleted)
            .OrderBy(x => x.GradeLevel.SortOrder).Select(x => new GradeOption(x.Id, x.ProgramAcademicYearId,
                x.GradeLevel.EducationStageId, x.NameAr, x.NameEn)).ToArrayAsync(ct);
        var classes = await db.ClassSections.AsNoTracking().Where(x => x.IsActive && !x.IsDeleted)
            .OrderBy(x => x.NameAr).Select(x => new ClassOption(x.Id, x.GradeOfferingId, x.NameAr, x.NameEn)).ToArrayAsync(ct);
        var gradeSubjects = await db.GradeSubjectOfferings.AsNoTracking().Where(x => x.IsActive && !x.IsDeleted &&
                x.GradeOffering.IsActive && !x.GradeOffering.IsDeleted)
            .OrderBy(x => x.GradeOffering.GradeLevel.SortOrder).ThenBy(x => x.CurriculumGradeSubject.Subject.NameAr)
            .Select(x => new GradeSubjectOption(x.Id, x.GradeOfferingId, x.GradeOffering.ProgramAcademicYearId,
                x.GradeOffering.GradeLevel.EducationStageId, x.GradeOffering.NameAr, x.GradeOffering.NameEn,
                x.CurriculumGradeSubject.Subject.NameAr, x.CurriculumGradeSubject.Subject.NameEn)).ToArrayAsync(ct);
        var subjects = await db.ClassSectionSubjects.AsNoTracking().Where(x => x.IsActive && !x.IsDeleted && x.ClassSection.IsActive &&
                !x.ClassSection.IsDeleted && x.GradeSubjectOffering.IsActive && !x.GradeSubjectOffering.IsDeleted)
            .OrderBy(x => x.ClassSection.GradeOffering.GradeLevel.SortOrder).ThenBy(x => x.ClassSection.NameAr)
            .Select(x => new SubjectOption(x.Id, x.GradeSubjectOfferingId, x.ClassSectionId,
                x.ClassSection.GradeOffering.ProgramAcademicYearId, x.ClassSection.GradeOfferingId,
                x.ClassSection.NameAr, x.ClassSection.NameEn, x.ClassSection.GradeOffering.GradeLevel.NameAr,
                x.ClassSection.GradeOffering.GradeLevel.NameEn, x.GradeSubjectOffering.CurriculumGradeSubject.Subject.NameAr,
                x.GradeSubjectOffering.CurriculumGradeSubject.Subject.NameEn)).ToArrayAsync(ct);
        var rooms = await db.SchoolRooms.AsNoTracking().Where(x => x.IsActive && !x.IsDeleted && x.IsSchedulable)
            .OrderBy(x => x.NameAr).Select(x => new RoomOption(x.Id, x.NameAr, x.NameEn, x.Capacity)).ToArrayAsync(ct);
        var users = await db.LocalUsers.AsNoTracking().Where(x => x.Status == LocalUserStatus.Active)
            .OrderBy(x => x.Person.DisplayName).Select(x => new UserOption(x.Id, x.Person.DisplayName, x.Kind)).ToArrayAsync(ct);
        return Ok(ApiResponse<object>.Success(new { programs, years, terms, stages, grades, classes, gradeSubjects, subjects, rooms, users,
            kinds = Enum.GetNames<ExamKind>(), formats = Enum.GetNames<ExamFormat>(), authorities = Enum.GetNames<ExamAuthorityType>(),
            administrationModes = Enum.GetNames<ExamAdministrationMode>(), invigilatorRoles = Enum.GetNames<ExamInvigilatorRole>() },
            correlationId: HttpContext.TraceIdentifier));
    }

    [HttpGet("policies"), Authorize(Policy = SchoolPermissionPolicies.ExamsView)]
    public async Task<IActionResult> Policies(CancellationToken ct)
    {
        await using var db = await RequireDb(ct); if (db is null) return Unauthorized();
        var rows = await db.ExamPolicies.AsNoTracking().OrderByDescending(x => x.EffectiveFrom).ThenByDescending(x => x.Version)
            .Select(x => new ExamPolicyResponse(x.Id, x.EducationProgramId, x.EducationStageId, x.ExamKind,
                x.DefaultAdministrationMode, x.TeacherCanCreate, x.TeacherCanPublishWithoutApproval,
                x.DepartmentApprovalRequired, x.SchoolApprovalRequired, x.ResultApprovalRequired,
                x.AllowScheduleWarningOverride, x.EffectiveFrom, x.EffectiveTo, x.Version, x.IsActive)).ToArrayAsync(ct);
        return Ok(ApiResponse<IReadOnlyList<ExamPolicyResponse>>.Success(rows, correlationId: HttpContext.TraceIdentifier));
    }

    [HttpGet("policies/{id:guid}/workflow-defaults"), Authorize(Policy = SchoolPermissionPolicies.ExamsView)]
    public async Task<IActionResult> WorkflowDefaults(Guid id, CancellationToken ct) {
        await using var db = await RequireDb(ct); if (db is null) return Unauthorized();
        var policy = await db.ExamPolicies.AsNoTracking().SingleOrDefaultAsync(x => x.Id == id, ct);
        return policy is null ? NotFound() : Ok(ApiResponse<object>.Success(JsonSerializer.Deserialize<WorkflowDefault[]>(policy.WorkflowDefaultsJson ?? "[]") ?? []));
    }
    [HttpPut("policies/{id:guid}/workflow-defaults"), Authorize(Policy = SchoolPermissionPolicies.ExamsPolicyManage)]
    public async Task<IActionResult> SaveWorkflowDefaults(Guid id, IReadOnlyList<WorkflowDefault> defaults, CancellationToken ct) {
        await using var db = await RequireDb(ct); if (db is null) return Unauthorized();
        var policy = await db.ExamPolicies.SingleOrDefaultAsync(x => x.Id == id, ct); if (policy is null) return NotFound();
        if (defaults.Count > 5 || defaults.Any(x => x is null || !Enum.IsDefined(x.Stage)) || defaults.Select(x => x.Stage).Distinct().Count() != defaults.Count)
            return BadRequest(Failure(400, "exams.workflow_invalid", "Invalid workflow defaults."));
        foreach (var d in defaults) if (!await ExamWorkflowEngine.Permission(db, d.AssigneeUserId, ExamWorkflowRules.Permission(d.Stage), ct) || !await ExamWorkflowEngine.Permission(db, d.AssigneeUserId, "school.exams.view", ct))
            return BadRequest(Failure(400, "exams.workflow_assignee_invalid", "The assignee is not eligible."));
        policy.WorkflowDefaultsJson = JsonSerializer.Serialize(defaults); policy.UpdatedAtUtc = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(ct); return Ok(ApiResponse<object?>.Success(null));
    }
    [HttpPost("policies"), Authorize(Policy = SchoolPermissionPolicies.ExamsPolicyManage)]
    public async Task<IActionResult> SavePolicy([FromBody] SaveExamPolicyRequest request, CancellationToken ct)
    {
        if (request.EffectiveFrom == default || request.EffectiveTo.HasValue && request.EffectiveTo < request.EffectiveFrom ||
            !Enum.IsDefined(request.DefaultAdministrationMode) || request.ExamKind.HasValue && !Enum.IsDefined(request.ExamKind.Value) ||
            request.EducationStageId.HasValue && !request.EducationProgramId.HasValue)
            return BadRequest(Failure(400, "exams.policy_invalid", "Enter a valid exam policy."));
        await using var db = await RequireDb(ct); if (db is null) return Unauthorized();
        if (request.EducationProgramId.HasValue && !await db.EducationPrograms.AsNoTracking().AnyAsync(x =>
            x.Id == request.EducationProgramId.Value && x.IsActive && !x.IsDeleted, ct))
            return BadRequest(Failure(400, "exams.policy_program_invalid", "Select an active education program."));
        if (request.EducationStageId.HasValue && !await db.EducationStages.AsNoTracking().AnyAsync(x =>
            x.Id == request.EducationStageId.Value && x.EducationProgramId == request.EducationProgramId && x.IsActive && !x.IsDeleted, ct))
            return BadRequest(Failure(400, "exams.policy_stage_invalid", "Select an active stage from the selected education program."));
        var latestVersion = await db.ExamPolicies.Where(x => x.EducationProgramId == request.EducationProgramId &&
            x.EducationStageId == request.EducationStageId && x.ExamKind == request.ExamKind).MaxAsync(x => (int?)x.Version, ct) ?? 0;
        var now = DateTimeOffset.UtcNow;
        var superseded = await db.ExamPolicies.Where(x => x.IsActive && x.EducationProgramId == request.EducationProgramId &&
            x.EducationStageId == request.EducationStageId && x.ExamKind == request.ExamKind).ToArrayAsync(ct);
        foreach (var existing in superseded) { existing.IsActive = false; existing.UpdatedAtUtc = now; }
        var policy = new ExamPolicy { Id = Guid.NewGuid(), EducationProgramId = request.EducationProgramId,
            EducationStageId = request.EducationStageId, ExamKind = request.ExamKind,
            DefaultAdministrationMode = request.DefaultAdministrationMode, TeacherCanCreate = request.TeacherCanCreate,
            TeacherCanPublishWithoutApproval = request.TeacherCanPublishWithoutApproval,
            DepartmentApprovalRequired = request.DepartmentApprovalRequired, SchoolApprovalRequired = request.SchoolApprovalRequired,
            ResultApprovalRequired = request.ResultApprovalRequired, AllowScheduleWarningOverride = request.AllowScheduleWarningOverride,
            WorkflowDefaultsJson = superseded.OrderByDescending(x => x.Version).FirstOrDefault()?.WorkflowDefaultsJson,
            EffectiveFrom = request.EffectiveFrom, EffectiveTo = request.EffectiveTo, Version = latestVersion + 1,
            IsActive = true, CreatedAtUtc = now, UpdatedAtUtc = now };
        db.ExamPolicies.Add(policy); await db.SaveChangesAsync(ct);
        return Ok(ApiResponse<object>.Success(new { policy.Id, policy.Version }, correlationId: HttpContext.TraceIdentifier));
    }

    [HttpPut("policies/{id:guid}/status"), Authorize(Policy = SchoolPermissionPolicies.ExamsPolicyManage)]
    public async Task<IActionResult> SetPolicyStatus(Guid id, [FromBody] ChangeExamPolicyStatusRequest request, CancellationToken ct)
    {
        await using var db = await RequireDb(ct); if (db is null) return Unauthorized();
        var policy = await db.ExamPolicies.SingleOrDefaultAsync(x => x.Id == id, ct);
        if (policy is null) return NotFound(Failure(404, "exams.policy_not_found", "Exam policy was not found."));
        var now = DateTimeOffset.UtcNow;
        if (request.IsActive)
        {
            var competing = await db.ExamPolicies.Where(x => x.Id != id && x.IsActive &&
                x.EducationProgramId == policy.EducationProgramId && x.EducationStageId == policy.EducationStageId &&
                x.ExamKind == policy.ExamKind).ToArrayAsync(ct);
            foreach (var existing in competing) { existing.IsActive = false; existing.UpdatedAtUtc = now; }
        }
        policy.IsActive = request.IsActive; policy.UpdatedAtUtc = now;
        await db.SaveChangesAsync(ct);
        return Ok(ApiResponse<object?>.Success(null, correlationId: HttpContext.TraceIdentifier));
    }

    [HttpPost("series"), Authorize(Policy = SchoolPermissionPolicies.ExamsManage)]
    public async Task<IActionResult> CreateSeries([FromBody] CreateCentralExamRequest request, CancellationToken ct)
    {
        if (request.ProgramAcademicYearId == Guid.Empty || request.Papers.Count == 0 || !Enum.IsDefined(request.Kind) ||
            !Enum.IsDefined(request.AdministrationMode) || !Enum.IsDefined(request.IssuingAuthority) ||
            !Enum.IsDefined(request.SchedulingAuthority) || !request.ScopeLevel.HasValue || !Enum.IsDefined(request.ScopeLevel.Value) ||
            request.Papers.Any(x => x.GradeSubjectOfferingId == Guid.Empty || x.DurationMinutes is < 5 or > 600 ||
                !SchoolExamRules.HasValidScoreDefinition(x.TotalScore, x.PassScore)))
            return BadRequest(Failure(400, "exams.invalid", "Enter valid exam series and paper data."));
        if (request.Kind == ExamKind.Other && (string.IsNullOrWhiteSpace(request.CustomNameAr) ||
            request.Papers.Any(x => string.IsNullOrWhiteSpace(x.CustomTitleAr))))
            return BadRequest(Failure(400, "exams.custom_name_required", "Enter a name for a custom exam and each of its papers."));
        if (request.IssuingAuthority == ExamAuthorityType.EducationalAuthority &&
            (string.IsNullOrWhiteSpace(request.ExternalSourceCode) || string.IsNullOrWhiteSpace(request.ExternalReferenceId)))
            return BadRequest(Failure(400, "exams.external_reference_required", "Enter the educational authority source and reference."));

        await using var db = await RequireDb(ct); if (db is null) return Unauthorized();
        var school = await db.SchoolInformation.AsNoTracking().SingleOrDefaultAsync(ct);
        if (school?.TimeZoneId is null) return Conflict(Failure(409, "exams.time_zone_required", "Configure a valid school time zone first."));
        TimeZoneInfo zone; try { zone = SchoolClock.Resolve(school.TimeZoneId); } catch { return Conflict(Failure(409, "exams.time_zone_required", "Configure a valid school time zone first.")); }
        var year = await db.ProgramAcademicYears.AsNoTracking().Where(x => x.Id == request.ProgramAcademicYearId && x.IsActive && !x.IsDeleted)
            .Select(x => new { x.Id, x.EducationProgramId, x.StartDate, x.EndDate, x.NameAr, x.NameEn }).SingleOrDefaultAsync(ct);
        if (year is null) return BadRequest(Failure(400, "exams.year_invalid", "Select an active academic year."));
        if (request.Papers.Any(x => x.ExamDate < year.StartDate || x.ExamDate > year.EndDate))
            return BadRequest(Failure(400, "exams.date_outside_year", "Every exam date must be inside the academic year."));

        var terms = await db.AcademicTerms.AsNoTracking().Where(x => x.ProgramAcademicYearId == year.Id && x.IsActive && !x.IsDeleted)
            .OrderBy(x => x.SortOrder).Select(x => new CentralTerm(x.Id, x.NameAr, x.NameEn, x.StartDate, x.EndDate, x.SortOrder)).ToArrayAsync(ct);
        var term = request.AcademicTermId.HasValue ? terms.SingleOrDefault(x => x.Id == request.AcademicTermId.Value) : null;
        if (request.Kind is ExamKind.Monthly or ExamKind.MidTerm or ExamKind.TermFinal or ExamKind.YearFinal && term is null)
            return BadRequest(Failure(400, "exams.term_required", "Select a term that belongs to the academic year."));

        var effectiveKind = request.Kind;
        if (request.Kind is ExamKind.TermFinal or ExamKind.YearFinal)
        {
            if (request.ScopeLevel != ExamScopeLevel.Grade)
                return BadRequest(Failure(400, "exams.final_scope_invalid", "Term and year finals must target a grade."));
            effectiveKind = SchoolExamRules.ResolveFinalKind(term!.SortOrder, terms.Max(x => x.SortOrder));
        }
        if (request.Kind == ExamKind.Monthly && (!request.AssessmentMonth.HasValue ||
            !SchoolExamRules.MonthIntersectsTerm(request.AssessmentMonth.Value, term!.StartDate, term.EndDate)))
            return BadRequest(Failure(400, "exams.month_outside_term", "Select a normalized month that intersects the selected term."));
        if (request.Kind != ExamKind.Monthly && request.AssessmentMonth.HasValue)
            return BadRequest(Failure(400, "exams.month_not_allowed", "The assessment month is available only for monthly exams."));

        var stage = request.EducationStageId.HasValue ? await db.EducationStages.AsNoTracking()
            .Where(x => x.Id == request.EducationStageId && x.EducationProgramId == year.EducationProgramId && x.IsActive && !x.IsDeleted)
            .Select(x => new CentralStage(x.Id, x.NameAr, x.NameEn)).SingleOrDefaultAsync(ct) : null;
        var grade = request.GradeOfferingId.HasValue ? await db.GradeOfferings.AsNoTracking()
            .Where(x => x.Id == request.GradeOfferingId && x.ProgramAcademicYearId == year.Id && x.IsActive && !x.IsDeleted)
            .Select(x => new CentralGrade(x.Id, x.GradeLevel.EducationStageId, x.NameAr, x.NameEn)).SingleOrDefaultAsync(ct) : null;
        var section = request.ClassSectionId.HasValue ? await db.ClassSections.AsNoTracking()
            .Where(x => x.Id == request.ClassSectionId && x.IsActive && !x.IsDeleted)
            .Select(x => new CentralSection(x.Id, x.GradeOfferingId, x.NameAr, x.NameEn, x.Capacity)).SingleOrDefaultAsync(ct) : null;
        var scopeInvalid = request.ScopeLevel switch
        {
            ExamScopeLevel.Stage => stage is null || request.GradeOfferingId.HasValue || request.ClassSectionId.HasValue,
            ExamScopeLevel.Grade => stage is null || grade is null || grade.StageId != stage.Id || request.ClassSectionId.HasValue,
            ExamScopeLevel.ClassSection => stage is null || grade is null || section is null || grade.StageId != stage.Id || section.GradeOfferingId != grade.Id,
            _ => true
        };
        if (scopeInvalid) return BadRequest(Failure(400, "exams.scope_invalid", "Select a valid education stage, grade or class scope."));

        var subjectIds = request.Papers.Select(x => x.GradeSubjectOfferingId).Distinct().ToArray();
        var subjects = await db.GradeSubjectOfferings.AsNoTracking().Where(x => subjectIds.Contains(x.Id) && x.IsActive && !x.IsDeleted &&
                x.GradeOffering.IsActive && !x.GradeOffering.IsDeleted)
            .Select(x => new CentralGradeSubject(x.Id, x.GradeOfferingId, x.GradeOffering.GradeLevel.EducationStageId,
                x.CurriculumGradeSubject.Subject.NameAr, x.CurriculumGradeSubject.Subject.NameEn))
            .ToDictionaryAsync(x => x.Id, ct);
        if (subjects.Count != subjectIds.Length || subjects.Values.Any(x => request.ScopeLevel == ExamScopeLevel.Stage
                ? x.StageId != stage!.Id : x.GradeOfferingId != grade!.Id))
            return BadRequest(Failure(400, "exams.subject_invalid", "One or more grade subjects are outside the selected scope."));
        if (request.Papers.GroupBy(x => new { x.GradeSubjectOfferingId, x.ExamDate }).Any(x => x.Count() > 1))
            return Conflict(Failure(409, "exams.paper_duplicate", "The same class subject cannot be scheduled twice on one day."));

        var relevantGradeIds = subjects.Values.Select(x => x.GradeOfferingId).Distinct().ToArray();
        var classRows = await db.ClassSections.AsNoTracking().Where(x => relevantGradeIds.Contains(x.GradeOfferingId) && x.IsActive && !x.IsDeleted &&
                (!request.ClassSectionId.HasValue || x.Id == request.ClassSectionId.Value))
            .Select(x => new CentralSection(x.Id, x.GradeOfferingId, x.NameAr, x.NameEn, x.Capacity)).ToArrayAsync(ct);
        if (relevantGradeIds.Any(id => classRows.All(x => x.GradeOfferingId != id)))
            return Conflict(Failure(409, "exams.scope_empty", "Every selected grade subject must have at least one active class."));

        if (effectiveKind is ExamKind.TermFinal or ExamKind.YearFinal && await db.ExamSeries.AsNoTracking().AnyAsync(x =>
            x.ProgramAcademicYearId == year.Id && x.AcademicTermId == term!.Id && x.ScopeGradeOfferingId == grade!.Id &&
            x.Purpose == ExamSeriesPurpose.Regular && x.Status != ExamSeriesStatus.Cancelled &&
            (x.Kind == ExamKind.TermFinal || x.Kind == ExamKind.YearFinal), ct))
            return Conflict(Failure(409, "exams.final_slot_exists", "A primary final exam already exists for this grade and term."));

        if (effectiveKind == ExamKind.Monthly)
        {
            var existing = await db.ExamPaperTargets.AsNoTracking().Where(x => x.ClassSectionId.HasValue &&
                    x.ExamPaper.ExamSeries.ProgramAcademicYearId == year.Id && x.ExamPaper.ExamSeries.AcademicTermId == term!.Id &&
                    x.ExamPaper.ExamSeries.AssessmentMonth == request.AssessmentMonth && x.ExamPaper.ExamSeries.Kind == ExamKind.Monthly &&
                    x.ExamPaper.ExamSeries.Purpose == ExamSeriesPurpose.Regular && x.ExamPaper.ExamSeries.Status != ExamSeriesStatus.Cancelled)
                .Select(x => new { ClassSectionId = x.ClassSectionId!.Value, x.ExamPaper.GradeSubjectOfferingId }).ToArrayAsync(ct);
            if (request.Papers.Any(p => classRows.Where(c => c.GradeOfferingId == subjects[p.GradeSubjectOfferingId].GradeOfferingId)
                .Any(c => existing.Any(e => e.ClassSectionId == c.Id && e.GradeSubjectOfferingId == p.GradeSubjectOfferingId))))
                return Conflict(Failure(409, "exams.monthly_scope_overlap", "A monthly exam already covers one of the selected classes and subjects."));
        }

        var now = DateTimeOffset.UtcNow; var actor = CurrentUserId();
        var applicablePolicy = await ResolvePolicy(db, year.EducationProgramId, stage!.Id, effectiveKind,
            request.Papers.Min(x => x.ExamDate), ct);
        if (applicablePolicy is { TeacherCanCreate: false } && !User.HasClaim(SchoolClaimTypes.Role, SchoolIdentitySeed.SchoolAdminRoleCode))
            return Forbid();
        if (!string.IsNullOrWhiteSpace(request.ExternalReferenceId) && await db.ExamSeries.IgnoreQueryFilters().AnyAsync(x =>
            x.ExternalSourceCode == request.ExternalSourceCode && x.ExternalReferenceId == request.ExternalReferenceId &&
            x.ExternalRevision == request.ExternalRevision && !x.IsDeleted, ct))
            return Conflict(Failure(409, "exams.external_duplicate", "This external exam revision was already imported."));
        var names = BuildSeriesNames(effectiveKind, request.AssessmentMonth, term, stage!, grade, section, request.CustomNameAr, request.CustomNameEn);
        var series = new ExamSeries { Id = Guid.NewGuid(), ProgramAcademicYearId = request.ProgramAcademicYearId,
            AcademicTermId = term?.Id, EducationStageId = stage!.Id, ScopeGradeOfferingId = grade?.Id,
            ScopeClassSectionId = section?.Id, ScopeLevel = request.ScopeLevel, AssessmentMonth = request.AssessmentMonth,
            Purpose = ExamSeriesPurpose.Regular, Code = ExamCode(now), NameAr = names.Ar, NameEn = names.En, Kind = effectiveKind,
            IssuingAuthority = request.IssuingAuthority, SchedulingAuthority = request.SchedulingAuthority,
            DefaultAdministrationMode = request.AdministrationMode, TimeZoneIdSnapshot = school.TimeZoneId,
            PolicySnapshotJson = applicablePolicy is null ? null : JsonSerializer.Serialize(new ExamPolicySnapshot(applicablePolicy.Id,
                applicablePolicy.Version, applicablePolicy.DefaultAdministrationMode, applicablePolicy.TeacherCanCreate,
                applicablePolicy.TeacherCanPublishWithoutApproval, applicablePolicy.DepartmentApprovalRequired,
                applicablePolicy.SchoolApprovalRequired, applicablePolicy.ResultApprovalRequired,
                applicablePolicy.AllowScheduleWarningOverride, applicablePolicy.WorkflowDefaultsJson)),
            ExternalSourceCode = Clean(request.ExternalSourceCode, 100), ExternalAuthorityName = Clean(request.ExternalAuthorityName, 250),
            ExternalReferenceId = Clean(request.ExternalReferenceId, 200), ExternalRevision = Clean(request.ExternalRevision, 100),
            IsExternalScheduleLocked = request.IsExternalScheduleLocked, CreatedByUserId = actor, CreatedAtUtc = now, UpdatedAtUtc = now };

        var targetKeys = new HashSet<(Guid GradeId, Guid ClassId)>(); var paperNo = 0;
        foreach (var input in request.Papers)
        {
            var subject = subjects[input.GradeSubjectOfferingId];
            var targetClasses = classRows.Where(x => x.GradeOfferingId == subject.GradeOfferingId).ToArray();
            foreach (var targetClass in targetClasses)
                if (targetKeys.Add((targetClass.GradeOfferingId, targetClass.Id))) series.Targets.Add(new ExamSeriesTarget
                    { Id = Guid.NewGuid(), GradeOfferingId = targetClass.GradeOfferingId, ClassSectionId = targetClass.Id });
            var endLocal = input.ExamDate.ToDateTime(input.StartsAt).AddMinutes(input.DurationMinutes);
            if (DateOnly.FromDateTime(endLocal) != input.ExamDate) return BadRequest(Failure(400, "exams.window_invalid", "An exam cannot cross the school day."));
            var endTime = TimeOnly.FromDateTime(endLocal); DateTimeOffset startsUtc, endsUtc;
            try { startsUtc = ToUtc(input.ExamDate, input.StartsAt, zone); endsUtc = ToUtc(input.ExamDate, endTime, zone); }
            catch { return BadRequest(Failure(400, "exams.local_time_invalid", "The selected local time is invalid.")); }
            var titles = BuildPaperTitles(effectiveKind, request.AssessmentMonth, term, subject.NameAr, subject.NameEn,
                input.CustomTitleAr, input.CustomTitleEn);
            var paper = new ExamPaper { Id = Guid.NewGuid(), PaperCode = $"P{++paperNo}", GradeSubjectOfferingId = subject.Id,
                TitleAr = titles.Ar, TitleEn = titles.En, Format = input.Format,
                Instructions = Clean(input.Instructions, 6000), TotalScore = input.TotalScore, PassScore = input.PassScore,
                DurationMinutes = input.DurationMinutes, SortOrder = paperNo, ContentOwnerUserId = actor, CreatedAtUtc = now, UpdatedAtUtc = now };
            var classSubjectIds = await db.ClassSectionSubjects.AsNoTracking().Where(x => x.GradeSubjectOfferingId == subject.Id &&
                    targetClasses.Select(c => c.Id).Contains(x.ClassSectionId) && x.IsActive && !x.IsDeleted)
                .ToDictionaryAsync(x => x.ClassSectionId, x => (Guid?)x.Id, ct);
            foreach (var targetClass in targetClasses) paper.Targets.Add(new ExamPaperTarget { Id = Guid.NewGuid(),
                GradeOfferingId = targetClass.GradeOfferingId, ClassSectionId = targetClass.Id,
                ClassSectionSubjectId = classSubjectIds.GetValueOrDefault(targetClass.Id) });
            var window = new ExamScheduleWindow { Id = Guid.NewGuid(), LocalDate = input.ExamDate, StartsAtLocal = input.StartsAt,
                EndsAtLocal = endTime, StartsAtUtc = startsUtc, EndsAtUtc = endsUtc, TimeZoneIdSnapshot = school.TimeZoneId,
                IsScheduleLocked = request.IsExternalScheduleLocked, ScheduledByUserId = actor, CreatedAtUtc = now, UpdatedAtUtc = now };
            var sitting = new ExamSitting { Id = Guid.NewGuid(), ExamPaper = paper, ExamScheduleWindow = window,
                AdministrationMode = request.AdministrationMode, DurationMinutesSnapshot = input.DurationMinutes,
                CreatedAtUtc = now, UpdatedAtUtc = now };
            paper.Sittings.Add(sitting);
            series.Papers.Add(paper); series.ScheduleWindows.Add(window);
            if (request.AdministrationMode == ExamAdministrationMode.InClass)
            {
                foreach (var targetClass in targetClasses)
                {
                    var room = await db.ClassRoomAssignments.AsNoTracking().Where(x => x.ClassSectionId == targetClass.Id && x.IsActive &&
                        !x.IsDeleted && x.EffectiveFrom <= input.ExamDate && x.EffectiveTo >= input.ExamDate).OrderByDescending(x => x.IsPrimary)
                        .Select(x => new { x.RoomId, x.Room.NameAr, x.Room.Capacity }).FirstOrDefaultAsync(ct);
                    var venue = new ExamWindowVenue { Id = Guid.NewGuid(), RoomId = room?.RoomId, ClassSectionId = targetClass.Id,
                        CapacitySnapshot = room?.Capacity ?? targetClass.Capacity, VenueLabel = room?.NameAr, CreatedAtUtc = now, UpdatedAtUtc = now };
                    window.Venues.Add(venue); sitting.Venues.Add(new ExamSittingVenue { Id = Guid.NewGuid(), ExamWindowVenue = venue, CreatedAtUtc = now });
                }
            }
        }
        db.ExamSeries.Add(series); AddAudit(db, series, "CentralSeriesCreated", now);
        await db.SaveChangesAsync(ct);
        return Ok(ApiResponse<object>.Success(new { series.Id, series.Code }, correlationId: HttpContext.TraceIdentifier));
    }

    [HttpPost("windows/{windowId:guid}/committees"), Authorize(Policy = SchoolPermissionPolicies.ExamsCommitteesManage)]
    public async Task<IActionResult> CreateCommittee(Guid windowId, [FromBody] SaveExamCommitteeRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Code) || string.IsNullOrWhiteSpace(request.NameAr) || request.Capacity <= 0)
            return BadRequest(Failure(400, "exams.committee_invalid", "Enter valid committee data."));
        await using var db = await RequireDb(ct); if (db is null) return Unauthorized();
        var window = await db.ExamScheduleWindows.Include(x => x.ExamSeries).Include(x => x.Sittings).ThenInclude(x => x.Venues)
            .SingleOrDefaultAsync(x => x.Id == windowId, ct);
        if (window is null) return NotFound(Failure(404, "exams.window_not_found", "Exam window was not found."));
        if (window.ExamSeries.Status != ExamSeriesStatus.Draft) return Conflict(Failure(409, "exams.not_draft", "Committees can be changed only before publishing."));
        var room = await db.SchoolRooms.AsNoTracking().SingleOrDefaultAsync(x => x.Id == request.RoomId && x.IsActive && !x.IsDeleted && x.IsSchedulable, ct);
        if (room is null || request.Capacity > room.Capacity) return BadRequest(Failure(400, "exams.room_capacity_invalid", "Select a suitable room and capacity."));
        if (await db.ExamWindowVenues.AsNoTracking().AnyAsync(x => x.RoomId == request.RoomId && x.ExamScheduleWindowId != window.Id &&
            x.ExamScheduleWindow.StartsAtUtc < window.EndsAtUtc && window.StartsAtUtc < x.ExamScheduleWindow.EndsAtUtc &&
            x.ExamScheduleWindow.Status != ExamScheduleWindowStatus.Cancelled, ct) ||
            await db.ExamWindowVenues.AsNoTracking().AnyAsync(x => x.RoomId == request.RoomId && x.ExamScheduleWindowId == window.Id, ct))
            return Conflict(Failure(409, "exams.room_conflict", "The room is already reserved during this exam window."));
        if (await db.ExamCommittees.AnyAsync(x => x.ExamScheduleWindowId == windowId && x.Code == request.Code.Trim(), ct))
            return Conflict(Failure(409, "exams.committee_code_exists", "Committee code already exists in this window."));
        var now = DateTimeOffset.UtcNow;
        var committee = new ExamCommittee { Id = Guid.NewGuid(), ExamSeriesId = window.ExamSeriesId, ExamScheduleWindowId = window.Id,
            Code = request.Code.Trim(), NameAr = request.NameAr.Trim(), NameEn = Clean(request.NameEn, 250) ?? request.NameAr.Trim(),
            Capacity = request.Capacity, SortOrder = request.SortOrder, CreatedAtUtc = now, UpdatedAtUtc = now };
        var venue = new ExamWindowVenue { Id = Guid.NewGuid(), ExamScheduleWindowId = window.Id, RoomId = room.Id,
            ExamCommittee = committee, CapacitySnapshot = request.Capacity, VenueLabel = room.NameAr, CreatedAtUtc = now, UpdatedAtUtc = now };
        db.ExamCommittees.Add(committee); db.ExamWindowVenues.Add(venue);
        foreach (var sitting in window.Sittings) db.ExamSittingVenues.Add(new ExamSittingVenue { Id = Guid.NewGuid(),
            ExamSittingId = sitting.Id, ExamWindowVenue = venue, CreatedAtUtc = now });
        AddAudit(db, window.ExamSeries, "CommitteeCreated", now, payload: JsonSerializer.Serialize(new { committee.Code, room.Id }));
        await db.SaveChangesAsync(ct);
        return Ok(ApiResponse<object>.Success(new { committee.Id }, correlationId: HttpContext.TraceIdentifier));
    }

    [HttpPost("committees/{committeeId:guid}/invigilators"), Authorize(Policy = SchoolPermissionPolicies.ExamsInvigilatorsManage)]
    public async Task<IActionResult> AssignInvigilator(Guid committeeId, [FromBody] SaveExamInvigilatorRequest request, CancellationToken ct)
    {
        await using var db = await RequireDb(ct); if (db is null) return Unauthorized();
        var committee = await db.ExamCommittees.Include(x => x.ExamSeries).Include(x => x.ExamScheduleWindow)
            .SingleOrDefaultAsync(x => x.Id == committeeId && x.IsActive, ct);
        if (committee is null) return NotFound(Failure(404, "exams.committee_not_found", "Committee was not found."));
        if (!await db.LocalUsers.AnyAsync(x => x.Id == request.UserId && x.Status == LocalUserStatus.Active, ct))
            return BadRequest(Failure(400, "exams.user_invalid", "Select an active employee or teacher."));
        var w = committee.ExamScheduleWindow;
        var absent = await db.StaffAbsences.AsNoTracking().AnyAsync(x => x.UserId == request.UserId && x.IsActive && !x.IsDeleted &&
            x.StartsOn <= w.LocalDate && x.EndsOn >= w.LocalDate && (!x.StartsAt.HasValue || x.StartsAt < w.EndsAtLocal) &&
            (!x.EndsAt.HasValue || x.EndsAt > w.StartsAtLocal), ct);
        if (absent) return Conflict(Failure(409, "exams.invigilator_absent", "The selected user is absent during this exam."));
        var conflict = await db.ExamInvigilatorAssignments.AsNoTracking().AnyAsync(x => x.UserId == request.UserId &&
            x.Status != ExamInvigilatorStatus.Cancelled && x.Status != ExamInvigilatorStatus.Replaced &&
            x.ExamCommittee.ExamScheduleWindow.StartsAtUtc < w.EndsAtUtc && w.StartsAtUtc < x.ExamCommittee.ExamScheduleWindow.EndsAtUtc, ct);
        if (conflict) return Conflict(Failure(409, "exams.invigilator_conflict", "The selected user already has an overlapping committee."));
        var now = DateTimeOffset.UtcNow;
        var assignment = new ExamInvigilatorAssignment { Id = Guid.NewGuid(), ExamCommitteeId = committee.Id, UserId = request.UserId,
            Role = request.Role, Notes = Clean(request.Notes, 1000), AssignedByUserId = CurrentUserId(), AssignedAtUtc = now, UpdatedAtUtc = now };
        db.ExamInvigilatorAssignments.Add(assignment); Notify(db, request.UserId, "ExamInvigilationAssigned", "تم إسناد مراقبة اختبار",
            "Exam invigilation assigned", $"تم إسنادك إلى لجنة {committee.NameAr} يوم {w.LocalDate:yyyy-MM-dd}",
            $"You were assigned to {committee.NameEn} on {w.LocalDate:yyyy-MM-dd}", "ExamCommittee", committee.Id, now);
        AddAudit(db, committee.ExamSeries, "InvigilatorAssigned", now, payload: JsonSerializer.Serialize(new { committee.Id, request.UserId, request.Role }));
        await db.SaveChangesAsync(ct);
        return Ok(ApiResponse<object>.Success(new { assignment.Id }, correlationId: HttpContext.TraceIdentifier));
    }

    [HttpPost("invigilators/{assignmentId:guid}/replace"), Authorize(Policy = SchoolPermissionPolicies.ExamsInvigilatorsManage)]
    public async Task<IActionResult> ReplaceInvigilator(Guid assignmentId, [FromBody] ReplaceExamInvigilatorRequest request, CancellationToken ct)
    {
        await using var db = await RequireDb(ct); if (db is null) return Unauthorized();
        var existing = await db.ExamInvigilatorAssignments.Include(x => x.ExamCommittee).ThenInclude(x => x.ExamScheduleWindow)
            .Include(x => x.ExamCommittee).ThenInclude(x => x.ExamSeries).SingleOrDefaultAsync(x => x.Id == assignmentId, ct);
        if (existing is null) return NotFound(Failure(404, "exams.invigilator_not_found", "Invigilator assignment was not found."));
        var result = await AssignInvigilatorInternal(db, existing, request.UserId, request.Reason, ct);
        if (result is not null) return result;
        await db.SaveChangesAsync(ct);
        return Ok(ApiResponse<object?>.Success(null, correlationId: HttpContext.TraceIdentifier));
    }

    [HttpGet("my-schedule"), Authorize(Policy = SchoolPermissionPolicies.ExamsView)]
    public async Task<IActionResult> MySchedule([FromQuery] DateOnly? from, [FromQuery] DateOnly? to, CancellationToken ct)
    {
        await using var db = await RequireDb(ct); if (db is null) return Unauthorized();
        var today = DateOnly.FromDateTime(DateTime.UtcNow); var start = from ?? today.AddDays(-7); var end = to ?? today.AddDays(30); var userId = CurrentUserId();
        var rows = await db.ExamInvigilatorAssignments.AsNoTracking().Where(x => x.UserId == userId &&
                x.Status != ExamInvigilatorStatus.Cancelled && x.Status != ExamInvigilatorStatus.Replaced &&
                x.ExamCommittee.ExamScheduleWindow.LocalDate >= start && x.ExamCommittee.ExamScheduleWindow.LocalDate <= end)
            .OrderBy(x => x.ExamCommittee.ExamScheduleWindow.StartsAtUtc)
            .Select(x => new MyExamDutyResponse(x.Id, x.ExamCommittee.ExamSeriesId, x.ExamCommittee.ExamSeries.NameAr,
                x.ExamCommittee.ExamSeries.NameEn, x.ExamCommittee.Id, x.ExamCommittee.NameAr, x.ExamCommittee.NameEn,
                x.ExamCommittee.ExamScheduleWindow.LocalDate, x.ExamCommittee.ExamScheduleWindow.StartsAtLocal,
                x.ExamCommittee.ExamScheduleWindow.EndsAtLocal, x.Role, x.Status,
                x.ExamCommittee.Venues.Select(v => v.VenueLabel).FirstOrDefault())).ToArrayAsync(ct);
        return Ok(ApiResponse<IReadOnlyList<MyExamDutyResponse>>.Success(rows, correlationId: HttpContext.TraceIdentifier));
    }

    [HttpGet("series/{seriesId:guid}/operations"), Authorize(Policy = SchoolPermissionPolicies.ExamsView)]
    public async Task<IActionResult> Operations(Guid seriesId, CancellationToken ct)
    {
        await using var db = await RequireDb(ct); if (db is null) return Unauthorized();
        var series = await db.ExamSeries.AsNoTracking().Where(x => x.Id == seriesId).Select(x => new
        {
            x.Id, x.Code, x.NameAr, x.NameEn, x.Status, x.DefaultAdministrationMode,
            windows = x.ScheduleWindows.OrderBy(w => w.StartsAtUtc).Select(w => new
            {
                w.Id, w.LocalDate, w.StartsAtLocal, w.EndsAtLocal, w.Status, w.IsScheduleLocked,
                committees = w.Committees.OrderBy(c => c.SortOrder).Select(c => new
                {
                    c.Id, c.Code, c.NameAr, c.NameEn, c.Capacity,
                    roomId = c.Venues.Select(v => v.RoomId).FirstOrDefault(), venue = c.Venues.Select(v => v.VenueLabel).FirstOrDefault(),
                    occupied = c.Venues.SelectMany(v => v.Sittings).SelectMany(v => v.CandidateAssignments).Count(),
                    invigilators = c.Invigilators.Where(i => i.Status != ExamInvigilatorStatus.Cancelled && i.Status != ExamInvigilatorStatus.Replaced)
                        .Select(i => new { i.Id, i.UserId, name = i.User.Person.DisplayName, i.Role, i.Status })
                })
            }),
            papers = x.Papers.OrderBy(p => p.SortOrder).Select(p => new { p.Id, p.TitleAr, p.TitleEn, p.ResultsStatus,
                candidateCount = p.Candidates.Count, appealCount = p.Candidates.SelectMany(c => c.Appeals).Count() })
        }).SingleOrDefaultAsync(ct);
        return series is null ? NotFound(Failure(404, "exams.not_found", "Exam series was not found.")) :
            Ok(ApiResponse<object>.Success(series, correlationId: HttpContext.TraceIdentifier));
    }

    [HttpPost("windows/{windowId:guid}/reschedule"), Authorize(Policy = SchoolPermissionPolicies.ExamsSchedule)]
    public async Task<IActionResult> Reschedule(Guid windowId, [FromBody] RescheduleExamWindowRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Reason) || request.Date == default || request.EndsAt <= request.StartsAt)
            return BadRequest(Failure(400, "exams.window_invalid", "Enter a valid new schedule and reason."));
        await using var db = await RequireDb(ct); if (db is null) return Unauthorized();
        var window = await db.ExamScheduleWindows.Include(x => x.ExamSeries).Include(x => x.CalendarProjection!).ThenInclude(x => x.SchoolCalendarEvent)
            .SingleOrDefaultAsync(x => x.Id == windowId, ct);
        if (window is null) return NotFound(Failure(404, "exams.window_not_found", "Exam window was not found."));
        if (window.IsScheduleLocked) return Conflict(Failure(409, "exams.external_schedule_locked", "This external schedule is locked."));
        if (window.ExamSeries.Status is ExamSeriesStatus.Closed or ExamSeriesStatus.Cancelled) return Conflict(Failure(409, "exams.workflow_locked", "The exam is closed or cancelled."));
        var paperIds = await db.ExamPapers.Where(x => x.ExamSeriesId == window.ExamSeriesId).Select(x => x.Id).ToArrayAsync(ct);
        if (!await ExamWorkflowEngine.Eligible(db, CurrentUserId(), ExamWorkflowStage.Schedule, paperIds, ct)) return Forbid();
        var zone = SchoolClock.Resolve(window.TimeZoneIdSnapshot); var now = DateTimeOffset.UtcNow;
        var oldDate = $"{window.LocalDate:yyyy-MM-dd} {window.StartsAtLocal:HH:mm}";
        window.LocalDate = request.Date; window.StartsAtLocal = request.StartsAt; window.EndsAtLocal = request.EndsAt;
        window.StartsAtUtc = ToUtc(request.Date, request.StartsAt, zone); window.EndsAtUtc = ToUtc(request.Date, request.EndsAt, zone);
        window.Status = window.ExamSeries.Status == ExamSeriesStatus.Published ? ExamScheduleWindowStatus.Scheduled : ExamScheduleWindowStatus.Draft;
        window.PostponementReason = request.Reason.Trim(); window.UpdatedAtUtc = now;
        if (window.CalendarProjection is not null) { window.CalendarProjection.SchoolCalendarEvent.StartDate = request.Date;
            window.CalendarProjection.SchoolCalendarEvent.EndDate = request.Date; window.CalendarProjection.SchoolCalendarEvent.UpdatedAtUtc = now;
            window.CalendarProjection.ProjectionVersion++; window.CalendarProjection.LastProjectedAtUtc = now; }
        AddAudit(db, window.ExamSeries, "WindowRescheduled", now, request.Reason, payload: JsonSerializer.Serialize(request));
        await ExamWorkflowEngine.Rescheduled(db, window.ExamSeries, CurrentUserId(), oldDate, $"{request.Date:yyyy-MM-dd} {request.StartsAt:HH:mm}", request.Reason.Trim(), ct);
        await db.SaveChangesAsync(ct); return Ok(ApiResponse<object?>.Success(null, correlationId: HttpContext.TraceIdentifier));
    }

    [HttpPost("papers/{paperId:guid}/makeup"), Authorize(Policy = SchoolPermissionPolicies.ExamsSchedule)]
    public async Task<IActionResult> CreateMakeup(Guid paperId, [FromBody] CreateMakeupSittingRequest request, CancellationToken ct)
    {
        if (request.ExamPaperCandidateIds.Count == 0 || request.Date == default || request.DurationMinutes is < 5 or > 600)
            return BadRequest(Failure(400, "exams.makeup_invalid", "Select candidates and a valid makeup schedule."));
        await using var db = await RequireDb(ct); if (db is null) return Unauthorized();
        var paper = await db.ExamPapers.Include(x => x.ExamSeries).Include(x => x.Candidates).ThenInclude(x => x.SittingAssignments)
            .SingleOrDefaultAsync(x => x.Id == paperId, ct);
        if (paper is null) return NotFound(Failure(404, "exams.paper_not_found", "Exam paper was not found."));
        var selected = paper.Candidates.Where(x => request.ExamPaperCandidateIds.Contains(x.Id)).ToArray();
        if (selected.Length != request.ExamPaperCandidateIds.Distinct().Count()) return BadRequest(Failure(400, "exams.candidate_invalid", "One or more candidates are invalid."));
        var room = await db.SchoolRooms.AsNoTracking().SingleOrDefaultAsync(x => x.Id == request.RoomId && x.IsActive && !x.IsDeleted, ct);
        if (room is null || room.Capacity < selected.Length) return BadRequest(Failure(400, "exams.room_capacity_invalid", "Select a room with enough capacity."));
        var zone = SchoolClock.Resolve(paper.ExamSeries.TimeZoneIdSnapshot); var localEnd = request.Date.ToDateTime(request.StartsAt).AddMinutes(request.DurationMinutes);
        var end = TimeOnly.FromDateTime(localEnd); var now = DateTimeOffset.UtcNow;
        var window = new ExamScheduleWindow { Id = Guid.NewGuid(), ExamSeriesId = paper.ExamSeriesId, LocalDate = request.Date,
            StartsAtLocal = request.StartsAt, EndsAtLocal = end, StartsAtUtc = ToUtc(request.Date, request.StartsAt, zone), EndsAtUtc = ToUtc(request.Date, end, zone),
            TimeZoneIdSnapshot = paper.ExamSeries.TimeZoneIdSnapshot, Status = ExamScheduleWindowStatus.Scheduled, ScheduledByUserId = CurrentUserId(), CreatedAtUtc = now, UpdatedAtUtc = now };
        var sitting = new ExamSitting { Id = Guid.NewGuid(), ExamPaperId = paper.Id, ExamScheduleWindow = window,
            ParentSittingId = selected.SelectMany(x => x.SittingAssignments).Select(x => (Guid?)x.ExamSittingId).FirstOrDefault(), Purpose = ExamSittingPurpose.MakeUp,
            AdministrationMode = ExamAdministrationMode.InClass, DurationMinutesSnapshot = request.DurationMinutes, CreatedAtUtc = now, UpdatedAtUtc = now };
        var venue = new ExamWindowVenue { Id = Guid.NewGuid(), ExamScheduleWindow = window, RoomId = room.Id, CapacitySnapshot = room.Capacity,
            VenueLabel = room.NameAr, CreatedAtUtc = now, UpdatedAtUtc = now };
        var sittingVenue = new ExamSittingVenue { Id = Guid.NewGuid(), ExamSitting = sitting, ExamWindowVenue = venue, CreatedAtUtc = now };
        db.ExamScheduleWindows.Add(window); db.ExamSittings.Add(sitting); db.ExamWindowVenues.Add(venue); db.ExamSittingVenues.Add(sittingVenue);
        foreach (var candidate in selected)
        {
            var assignment = new ExamCandidateSittingAssignment { Id = Guid.NewGuid(), ExamPaperId = paper.Id, ExamPaperCandidateId = candidate.Id,
                ExamSitting = sitting, ExamSittingVenue = sittingVenue, AssignedByUserId = CurrentUserId(), AssignedAtUtc = now };
            db.ExamCandidateSittingAssignments.Add(assignment); db.ExamAttendance.Add(new ExamAttendance { Id = Guid.NewGuid(), ExamCandidateSittingAssignment = assignment, CreatedAtUtc = now, UpdatedAtUtc = now });
            db.ExamResultAttempts.Add(new ExamResultAttempt { Id = Guid.NewGuid(), ExamPaperCandidateId = candidate.Id, ExamCandidateSittingAssignment = assignment,
                AttemptNumber = candidate.SittingAssignments.Count + 1, IsFinal = true, CreatedAtUtc = now, UpdatedAtUtc = now });
        }
        AddAudit(db, paper.ExamSeries, "MakeupSittingCreated", now, request.Reason, paper.Id, sitting.Id);
        await db.SaveChangesAsync(ct); return Ok(ApiResponse<object>.Success(new { windowId = window.Id, sittingId = sitting.Id }, correlationId: HttpContext.TraceIdentifier));
    }

    [HttpGet("series/{seriesId:guid}/analytics"), Authorize(Policy = SchoolPermissionPolicies.ExamsReports)]
    public async Task<IActionResult> Analytics(Guid seriesId, CancellationToken ct)
    {
        await using var db = await RequireDb(ct); if (db is null) return Unauthorized();
        var papers = await db.ExamPapers.AsNoTracking().Where(x => x.ExamSeriesId == seriesId).Select(x => new
        {
            x.Id, x.TitleAr, x.TitleEn, x.TotalScore, x.PassScore, candidates = x.Candidates.Count,
            present = x.Candidates.Count(c => c.SittingAssignments.Any(a => a.Attendance!.Status == ExamAttendanceStatus.Present || a.Attendance.Status == ExamAttendanceStatus.Late)),
            scored = x.Candidates.Count(c => c.ResultAttempts.Any(a => a.IsFinal && a.Disposition == ExamResultDisposition.Scored)),
            passed = x.Candidates.Count(c => c.ResultAttempts.Any(a => a.IsFinal && a.Disposition == ExamResultDisposition.Scored && (!x.PassScore.HasValue || a.Score >= x.PassScore))),
            average = x.Candidates.SelectMany(c => c.ResultAttempts).Where(a => a.IsFinal && a.Score.HasValue).Average(a => (decimal?)a.Score)
        }).ToArrayAsync(ct);
        return Ok(ApiResponse<object>.Success(new { papers }, correlationId: HttpContext.TraceIdentifier));
    }

    [HttpPost("paper-candidates/{paperCandidateId:guid}/appeals"), Authorize(Policy = SchoolPermissionPolicies.ExamsResultsReopen)]
    public async Task<IActionResult> CreateAppeal(Guid paperCandidateId, [FromBody] ExamReasonRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Reason)) return BadRequest(Failure(400, "exams.reason_required", "Enter an appeal reason."));
        await using var db = await RequireDb(ct); if (db is null) return Unauthorized();
        var candidate = await db.ExamPaperCandidates.Include(x => x.ExamPaper).ThenInclude(x => x.ExamSeries)
            .Include(x => x.ResultAttempts).SingleOrDefaultAsync(x => x.Id == paperCandidateId, ct);
        if (candidate is null) return NotFound(Failure(404, "exams.candidate_not_found", "Exam candidate was not found."));
        if (candidate.ExamPaper.ResultsStatus != ExamResultsStatus.Published) return Conflict(Failure(409, "exams.results_not_published", "Only published results can be appealed."));
        if (await db.ExamResultAppeals.AnyAsync(x => x.ExamPaperCandidateId == paperCandidateId &&
            (x.Status == ExamAppealStatus.Submitted || x.Status == ExamAppealStatus.UnderReview), ct))
            return Conflict(Failure(409, "exams.appeal_open", "An open appeal already exists."));
        var now = DateTimeOffset.UtcNow; var score = candidate.ResultAttempts.Where(x => x.IsFinal).OrderByDescending(x => x.AttemptNumber).Select(x => x.Score).FirstOrDefault();
        var appeal = new ExamResultAppeal { Id = Guid.NewGuid(), ExamPaperCandidateId = paperCandidateId, Reason = request.Reason.Trim(),
            PreviousScore = score, RequestedByUserId = CurrentUserId(), RequestedAtUtc = now, UpdatedAtUtc = now };
        db.ExamResultAppeals.Add(appeal); AddAudit(db, candidate.ExamPaper.ExamSeries, "ResultAppealSubmitted", now, request.Reason, candidate.ExamPaperId);
        await db.SaveChangesAsync(ct); return Ok(ApiResponse<object>.Success(new { appeal.Id }, correlationId: HttpContext.TraceIdentifier));
    }

    [HttpPost("appeals/{appealId:guid}/resolve"), Authorize(Policy = SchoolPermissionPolicies.ExamsResultsApprove)]
    public async Task<IActionResult> ResolveAppeal(Guid appealId, [FromBody] ResolveExamAppealRequest request, CancellationToken ct)
    {
        if (request.Status is not (ExamAppealStatus.Accepted or ExamAppealStatus.Rejected) || string.IsNullOrWhiteSpace(request.DecisionNotes))
            return BadRequest(Failure(400, "exams.appeal_decision_invalid", "Enter a valid appeal decision."));
        await using var db = await RequireDb(ct); if (db is null) return Unauthorized();
        var appeal = await db.ExamResultAppeals.Include(x => x.ExamPaperCandidate).ThenInclude(x => x.ExamPaper).ThenInclude(x => x.ExamSeries)
            .Include(x => x.ExamPaperCandidate).ThenInclude(x => x.ResultAttempts).SingleOrDefaultAsync(x => x.Id == appealId, ct);
        if (appeal is null) return NotFound(Failure(404, "exams.appeal_not_found", "Appeal was not found."));
        if (appeal.Status is ExamAppealStatus.Accepted or ExamAppealStatus.Rejected or ExamAppealStatus.Cancelled)
            return Conflict(Failure(409, "exams.appeal_closed", "Appeal is already closed."));
        var paper = appeal.ExamPaperCandidate.ExamPaper;
        if (request.Status == ExamAppealStatus.Accepted && (!request.RevisedScore.HasValue || request.RevisedScore < 0 || request.RevisedScore > paper.TotalScore))
            return BadRequest(Failure(400, "exams.score_invalid", "Enter a revised score within the paper total."));
        var now = DateTimeOffset.UtcNow; appeal.Status = request.Status; appeal.DecisionNotes = request.DecisionNotes.Trim();
        appeal.RevisedScore = request.Status == ExamAppealStatus.Accepted ? request.RevisedScore : null; appeal.ReviewedByUserId = CurrentUserId();
        appeal.ReviewedAtUtc = now; appeal.UpdatedAtUtc = now;
        if (request.Status == ExamAppealStatus.Accepted)
        {
            foreach (var attempt in appeal.ExamPaperCandidate.ResultAttempts.Where(x => x.IsFinal)) xSetFinal(attempt, false, now);
            var previous = appeal.ExamPaperCandidate.ResultAttempts.OrderByDescending(x => x.AttemptNumber).FirstOrDefault();
            db.ExamResultAttempts.Add(new ExamResultAttempt { Id = Guid.NewGuid(), ExamPaperCandidateId = appeal.ExamPaperCandidateId,
                ExamCandidateSittingAssignmentId = previous?.ExamCandidateSittingAssignmentId,
                AttemptNumber = (previous?.AttemptNumber ?? 0) + 1, Disposition = ExamResultDisposition.Scored,
                Score = request.RevisedScore, IsFinal = true, MarkerUserId = CurrentUserId(), ApprovedByUserId = CurrentUserId(),
                MarkedAtUtc = now, ApprovedAtUtc = now, PublishedAtUtc = now, Notes = $"Appeal {appeal.Id:D}", CreatedAtUtc = now, UpdatedAtUtc = now });
            paper.ResultsRevision++; paper.UpdatedAtUtc = now;
        }
        AddAudit(db, paper.ExamSeries, "ResultAppealResolved", now, request.DecisionNotes, paper.Id,
            payload: JsonSerializer.Serialize(new { appeal.Id, request.Status, request.RevisedScore }));
        await db.SaveChangesAsync(ct); return Ok(ApiResponse<object?>.Success(null, correlationId: HttpContext.TraceIdentifier));
    }

    [HttpGet("series/{seriesId:guid}/report.csv"), Authorize(Policy = SchoolPermissionPolicies.ExamsReports)]
    public async Task<IActionResult> ExportReport(Guid seriesId, CancellationToken ct)
    {
        await using var db = await RequireDb(ct); if (db is null) return Unauthorized();
        var series = await db.ExamSeries.AsNoTracking().SingleOrDefaultAsync(x => x.Id == seriesId, ct);
        if (series is null) return NotFound(Failure(404, "exams.not_found", "Exam series was not found."));
        var rows = await db.ExamPaperCandidates.AsNoTracking().Where(x => x.ExamPaper.ExamSeriesId == seriesId)
            .OrderBy(x => x.ExamPaper.SortOrder).ThenBy(x => x.ExamCandidate.NameArSnapshot)
            .Select(x => new { Paper = x.ExamPaper.TitleAr, x.ExamCandidate.ExamNumber, x.ExamCandidate.StudentCodeSnapshot,
                x.ExamCandidate.NameArSnapshot, Committee = x.SittingAssignments.Select(a => a.ExamSittingVenue.ExamWindowVenue.ExamCommittee!.NameAr).FirstOrDefault(),
                Seat = x.SittingAssignments.Select(a => a.SeatNumber).FirstOrDefault(),
                Attendance = x.SittingAssignments.Select(a => a.Attendance!.Status).FirstOrDefault(),
                Score = x.ResultAttempts.Where(a => a.IsFinal).Select(a => a.Score).FirstOrDefault(),
                Disposition = x.ResultAttempts.Where(a => a.IsFinal).Select(a => a.Disposition).FirstOrDefault() }).ToArrayAsync(ct);
        var csv = new StringBuilder("\uFEFFPaper,ExamNumber,StudentCode,StudentName,Committee,Seat,Attendance,Score,Disposition\r\n");
        foreach (var r in rows) csv.AppendLine(string.Join(',', Csv(r.Paper), Csv(r.ExamNumber), Csv(r.StudentCodeSnapshot), Csv(r.NameArSnapshot),
            Csv(r.Committee), r.Seat?.ToString(CultureInfo.InvariantCulture), r.Attendance, r.Score?.ToString(CultureInfo.InvariantCulture), r.Disposition));
        return File(Encoding.UTF8.GetBytes(csv.ToString()), "text/csv; charset=utf-8", $"exam-{series.Code}.csv");
    }

    private async Task<IActionResult?> AssignInvigilatorInternal(SchoolsDbContext db, ExamInvigilatorAssignment existing, Guid replacementUserId, string reason, CancellationToken ct)
    {
        var w = existing.ExamCommittee.ExamScheduleWindow;
        if (await db.StaffAbsences.AnyAsync(x => x.UserId == replacementUserId && x.IsActive && !x.IsDeleted && x.StartsOn <= w.LocalDate && x.EndsOn >= w.LocalDate, ct))
            return Conflict(Failure(409, "exams.invigilator_absent", "The replacement is absent on the exam date."));
        if (await db.ExamInvigilatorAssignments.AnyAsync(x => x.UserId == replacementUserId && x.Status != ExamInvigilatorStatus.Cancelled &&
            x.Status != ExamInvigilatorStatus.Replaced && x.ExamCommittee.ExamScheduleWindow.StartsAtUtc < w.EndsAtUtc &&
            w.StartsAtUtc < x.ExamCommittee.ExamScheduleWindow.EndsAtUtc, ct))
            return Conflict(Failure(409, "exams.invigilator_conflict", "The replacement already has an overlapping committee."));
        var now = DateTimeOffset.UtcNow; existing.Status = ExamInvigilatorStatus.Replaced; existing.Notes = Clean(reason, 1000); existing.UpdatedAtUtc = now;
        db.ExamInvigilatorAssignments.Add(new ExamInvigilatorAssignment { Id = Guid.NewGuid(), ExamCommitteeId = existing.ExamCommitteeId,
            UserId = replacementUserId, Role = existing.Role, ReplacesAssignmentId = existing.Id, AssignedByUserId = CurrentUserId(),
            AssignedAtUtc = now, UpdatedAtUtc = now, Notes = Clean(reason, 1000) });
        Notify(db, replacementUserId, "ExamInvigilationReplacement", "تكليف بديل لمراقبة اختبار", "Exam invigilation replacement",
            $"تم تكليفك بديلًا في لجنة {existing.ExamCommittee.NameAr}", $"You were assigned as replacement in {existing.ExamCommittee.NameEn}",
            "ExamCommittee", existing.ExamCommitteeId, now);
        AddAudit(db, existing.ExamCommittee.ExamSeries, "InvigilatorReplaced", now, reason,
            payload: JsonSerializer.Serialize(new { existing.Id, replacementUserId })); return null;
    }

    private static LocalizedExamName BuildSeriesNames(ExamKind kind, DateOnly? month, CentralTerm? term,
        CentralStage stage, CentralGrade? grade, CentralSection? section, string? customAr, string? customEn)
    {
        var scopeAr = section?.NameAr ?? grade?.NameAr ?? stage.NameAr;
        var scopeEn = section?.NameEn ?? grade?.NameEn ?? stage.NameEn;
        return kind switch
        {
            ExamKind.Monthly => new($"اختبارات شهر {ArabicMonth(month!.Value.Month)} — {scopeAr}",
                $"{EnglishMonth(month.Value.Month)} monthly exams — {scopeEn}"),
            ExamKind.TermFinal => new($"اختبارات نهاية {term!.NameAr} — {scopeAr}", $"End of {term.NameEn} exams — {scopeEn}"),
            ExamKind.YearFinal => new($"اختبارات نهاية العام — {scopeAr}", $"End-of-year exams — {scopeEn}"),
            ExamKind.MidTerm => new($"اختبارات منتصف {term!.NameAr} — {scopeAr}", $"Mid-{term.NameEn} exams — {scopeEn}"),
            ExamKind.Weekly => new($"اختبارات أسبوعية — {scopeAr}", $"Weekly exams — {scopeEn}"),
            _ => new(customAr!.Trim(), Clean(customEn, 250) ?? customAr.Trim())
        };
    }

    private static LocalizedExamName BuildPaperTitles(ExamKind kind, DateOnly? month, CentralTerm? term,
        string subjectAr, string subjectEn, string? customAr, string? customEn) => kind switch
    {
        ExamKind.Monthly => new($"اختبار شهر {ArabicMonth(month!.Value.Month)} — مادة {subjectAr}",
            $"{EnglishMonth(month.Value.Month)} monthly exam — {subjectEn}"),
        ExamKind.TermFinal => new($"اختبار نهاية {term!.NameAr} — مادة {subjectAr}", $"End of {term.NameEn} exam — {subjectEn}"),
        ExamKind.YearFinal => new($"اختبار نهاية العام — مادة {subjectAr}", $"End-of-year exam — {subjectEn}"),
        ExamKind.MidTerm => new($"اختبار منتصف {term!.NameAr} — مادة {subjectAr}", $"Mid-{term.NameEn} exam — {subjectEn}"),
        ExamKind.Weekly => new($"اختبار أسبوعي — مادة {subjectAr}", $"Weekly exam — {subjectEn}"),
        _ => new(customAr!.Trim(), Clean(customEn, 250) ?? customAr.Trim())
    };

    private static string ArabicMonth(int month) => new[] { "", "يناير", "فبراير", "مارس", "أبريل", "مايو", "يونيو",
        "يوليو", "أغسطس", "سبتمبر", "أكتوبر", "نوفمبر", "ديسمبر" }[month];
    private static string EnglishMonth(int month) => new DateTime(2000, month, 1).ToString("MMMM", CultureInfo.InvariantCulture);

    private static async Task<ExamPolicy?> ResolvePolicy(SchoolsDbContext db, Guid educationProgramId, Guid educationStageId,
        ExamKind kind, DateOnly date, CancellationToken ct) =>
        await db.ExamPolicies.AsNoTracking().Where(x => x.IsActive && (!x.EducationProgramId.HasValue || x.EducationProgramId == educationProgramId) &&
            (!x.EducationStageId.HasValue || x.EducationStageId == educationStageId) &&
            (!x.ExamKind.HasValue || x.ExamKind == kind) && x.EffectiveFrom <= date && (!x.EffectiveTo.HasValue || x.EffectiveTo >= date))
            .OrderByDescending(x => x.EducationStageId.HasValue).ThenByDescending(x => x.EducationProgramId.HasValue)
            .ThenByDescending(x => x.ExamKind.HasValue).ThenByDescending(x => x.Version).FirstOrDefaultAsync(ct);
    private static void xSetFinal(ExamResultAttempt attempt, bool value, DateTimeOffset now) { attempt.IsFinal = value; attempt.UpdatedAtUtc = now; }
    private async Task<SchoolsDbContext?> RequireDb(CancellationToken ct) => await dbFactory.CreateAsync(User.FindFirst(SchoolClaimTypes.SchoolCode)?.Value ?? string.Empty, ct);
    private Guid CurrentUserId() => Guid.TryParse(User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value, out var id) ? id : Guid.Empty;
    private ApiResponse<object?> Failure(int status, string code, string message) => ApiResponse<object?>.Failure(status, code, message, correlationId: HttpContext.TraceIdentifier);
    private void AddAudit(SchoolsDbContext db, ExamSeries series, string action, DateTimeOffset now, string? reason = null, Guid? paperId = null,
        Guid? sittingId = null, string? payload = null) => db.ExamAudits.Add(new ExamAudit { Id = Guid.NewGuid(), ExamSeries = series,
            ExamPaperId = paperId, ExamSittingId = sittingId, Action = action, ActorUserId = CurrentUserId(), Reason = Clean(reason, 1000),
            PayloadJson = payload, CreatedAtUtc = now });
    private static void Notify(SchoolsDbContext db, Guid userId, string type, string titleAr, string titleEn, string bodyAr, string bodyEn,
        string entityType, Guid entityId, DateTimeOffset now) => db.SchoolUserNotifications.Add(new SchoolUserNotification { Id = Guid.NewGuid(),
            RecipientUserId = userId, Type = type, TitleAr = titleAr, TitleEn = titleEn, BodyAr = bodyAr, BodyEn = bodyEn,
            RelatedEntityType = entityType, RelatedEntityId = entityId.ToString("D"), CreatedAtUtc = now, UpdatedAtUtc = now });
    private static DateTimeOffset ToUtc(DateOnly date, TimeOnly time, TimeZoneInfo zone)
    { var local = DateTime.SpecifyKind(date.ToDateTime(time), DateTimeKind.Unspecified); if (zone.IsInvalidTime(local)) throw new ArgumentException(); return new DateTimeOffset(TimeZoneInfo.ConvertTimeToUtc(local, zone), TimeSpan.Zero); }
    private static string ExamCode(DateTimeOffset now) => $"EX-{now:yyyyMMddHHmmss}-{Guid.NewGuid().ToString("N")[..6]}";
    private static string? Clean(string? value, int max) => string.IsNullOrWhiteSpace(value) ? null : value.Trim()[..Math.Min(value.Trim().Length, max)];
    private static string Csv(object? value) { var text = value?.ToString() ?? string.Empty; return $"\"{text.Replace("\"", "\"\"")}\""; }
}

public sealed record Option(Guid Id, string NameAr, string NameEn, string Code);
public sealed record YearOption(Guid Id, Guid EducationProgramId, string NameAr, string NameEn, string Code, DateOnly StartDate, DateOnly EndDate);
public sealed record TermOption(Guid Id, Guid ProgramAcademicYearId, string NameAr, string NameEn, DateOnly StartDate, DateOnly EndDate,
    int SortOrder, bool IsLast);
public sealed record StageOption(Guid Id, Guid EducationProgramId, string NameAr, string NameEn);
public sealed record GradeOption(Guid Id, Guid ProgramAcademicYearId, Guid EducationStageId, string NameAr, string NameEn);
public sealed record ClassOption(Guid Id, Guid GradeOfferingId, string NameAr, string NameEn);
public sealed record GradeSubjectOption(Guid Id, Guid GradeOfferingId, Guid ProgramAcademicYearId, Guid EducationStageId,
    string GradeNameAr, string GradeNameEn, string SubjectNameAr, string SubjectNameEn);
public sealed record SubjectOption(Guid Id, Guid GradeSubjectOfferingId, Guid ClassSectionId, Guid ProgramAcademicYearId,
    Guid GradeOfferingId, string ClassNameAr, string ClassNameEn, string GradeNameAr, string GradeNameEn, string SubjectNameAr, string SubjectNameEn);
public sealed record RoomOption(Guid Id, string NameAr, string NameEn, int Capacity);
public sealed record UserOption(Guid Id, string DisplayName, SchoolUserKind Kind);
public sealed record SaveExamPolicyRequest(Guid? EducationProgramId, Guid? EducationStageId, ExamKind? ExamKind,
    ExamAdministrationMode DefaultAdministrationMode, bool TeacherCanCreate, bool TeacherCanPublishWithoutApproval,
    bool DepartmentApprovalRequired, bool SchoolApprovalRequired, bool ResultApprovalRequired, bool AllowScheduleWarningOverride,
    DateOnly EffectiveFrom, DateOnly? EffectiveTo);
public sealed record ChangeExamPolicyStatusRequest(bool IsActive);
public sealed record ExamPolicyResponse(Guid Id, Guid? EducationProgramId, Guid? EducationStageId, ExamKind? ExamKind,
    ExamAdministrationMode DefaultAdministrationMode, bool TeacherCanCreate, bool TeacherCanPublishWithoutApproval,
    bool DepartmentApprovalRequired, bool SchoolApprovalRequired, bool ResultApprovalRequired, bool AllowScheduleWarningOverride,
    DateOnly EffectiveFrom, DateOnly? EffectiveTo, int Version, bool IsActive);
public sealed class CreateCentralExamRequest
{
    public Guid ProgramAcademicYearId { get; init; }
    public Guid? AcademicTermId { get; init; }
    public ExamKind Kind { get; init; }
    public ExamScopeLevel? ScopeLevel { get; init; }
    public Guid? EducationStageId { get; init; }
    public Guid? GradeOfferingId { get; init; }
    public Guid? ClassSectionId { get; init; }
    public DateOnly? AssessmentMonth { get; init; }
    public string? CustomNameAr { get; init; }
    public string? CustomNameEn { get; init; }
    public ExamAuthorityType IssuingAuthority { get; init; }
    public ExamAuthorityType SchedulingAuthority { get; init; }
    public ExamAdministrationMode AdministrationMode { get; init; }
    public string? ExternalSourceCode { get; init; }
    public string? ExternalAuthorityName { get; init; }
    public string? ExternalReferenceId { get; init; }
    public string? ExternalRevision { get; init; }
    public bool IsExternalScheduleLocked { get; init; }
    public IReadOnlyList<CreateCentralExamPaperRequest> Papers { get; init; } = [];
}
public sealed class CreateCentralExamPaperRequest
{
    public Guid GradeSubjectOfferingId { get; init; }
    public string? CustomTitleAr { get; init; }
    public string? CustomTitleEn { get; init; }
    public ExamFormat Format { get; init; } = ExamFormat.Written;
    public string? Instructions { get; init; }
    public decimal TotalScore { get; init; }
    public decimal? PassScore { get; init; }
    public DateOnly ExamDate { get; init; }
    [JsonConverter(typeof(FlexibleTimeOnlyJsonConverter))]
    public TimeOnly StartsAt { get; init; }
    public int DurationMinutes { get; init; }
}
public sealed record SaveExamCommitteeRequest(string Code, string NameAr, string? NameEn, Guid RoomId, int Capacity, int SortOrder);
public sealed record SaveExamInvigilatorRequest(Guid UserId, ExamInvigilatorRole Role, string? Notes);
public sealed record ReplaceExamInvigilatorRequest(Guid UserId, string Reason);
public sealed record ResolveExamAppealRequest(ExamAppealStatus Status, decimal? RevisedScore, string DecisionNotes);
public sealed record RescheduleExamWindowRequest(DateOnly Date,
    [property: JsonConverter(typeof(FlexibleTimeOnlyJsonConverter))] TimeOnly StartsAt,
    [property: JsonConverter(typeof(FlexibleTimeOnlyJsonConverter))] TimeOnly EndsAt, string Reason);
public sealed record CreateMakeupSittingRequest(IReadOnlyList<Guid> ExamPaperCandidateIds, DateOnly Date,
    [property: JsonConverter(typeof(FlexibleTimeOnlyJsonConverter))] TimeOnly StartsAt,
    int DurationMinutes, Guid RoomId, string? Reason);
public sealed record MyExamDutyResponse(Guid AssignmentId, Guid SeriesId, string SeriesNameAr, string SeriesNameEn,
    Guid CommitteeId, string CommitteeNameAr, string CommitteeNameEn, DateOnly Date, TimeOnly StartsAt, TimeOnly EndsAt,
    ExamInvigilatorRole Role, ExamInvigilatorStatus Status, string? Venue);
internal sealed record CentralTerm(Guid Id, string NameAr, string NameEn, DateOnly StartDate, DateOnly EndDate, int SortOrder);
internal sealed record CentralStage(Guid Id, string NameAr, string NameEn);
internal sealed record CentralGrade(Guid Id, Guid StageId, string NameAr, string NameEn);
internal sealed record CentralSection(Guid Id, Guid GradeOfferingId, string NameAr, string NameEn, int Capacity);
internal sealed record CentralGradeSubject(Guid Id, Guid GradeOfferingId, Guid StageId, string NameAr, string NameEn);
internal sealed record LocalizedExamName(string Ar, string En);
internal sealed record ExamPolicySnapshot(Guid Id, int Version, ExamAdministrationMode DefaultAdministrationMode,
    bool TeacherCanCreate, bool TeacherCanPublishWithoutApproval, bool DepartmentApprovalRequired,
    bool SchoolApprovalRequired, bool ResultApprovalRequired, bool AllowScheduleWarningOverride, string? WorkflowDefaultsJson = null);
