using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using Mdaresna.Api.Contracts;
using Mdaresna.Schools.Api.Auth;
using Mdaresna.Schools.Api.Documents;
using Mdaresna.Schools.Api.Time;
using Mdaresna.Schools.Domain.Academics;
using Mdaresna.Schools.Domain.Documents;
using Mdaresna.Schools.Domain.Identity;
using Mdaresna.Schools.Infrastructure.Identity;
using Mdaresna.Schools.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Mdaresna.Schools.Api.Controllers;

[ApiController, Route("api/schools/v1/teaching-plans")]
public sealed class SchoolTeachingPlansController(
    ISchoolDbContextFactory dbFactory,
    ISchoolDocumentStorage storage,
    IConfiguration configuration,
    SchoolClock clock) : ControllerBase
{
    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
        { ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".ppt", ".pptx", ".jpg", ".jpeg", ".png", ".webp" };
    private static readonly string[] ArabicMonths = ["يناير", "فبراير", "مارس", "أبريل", "مايو", "يونيو", "يوليو", "أغسطس", "سبتمبر", "أكتوبر", "نوفمبر", "ديسمبر"];

    [Authorize(Policy = SchoolPermissionPolicies.AcademicsView), HttpGet("options")]
    public async Task<IActionResult> Options(CancellationToken ct, Guid? programAcademicYearId = null, Guid? academicTermId = null, Guid? academicYearDefinitionId = null)
    {
        await using var db = await RequireDb(ct); if (db is null) return Unauthorized();
        var timeZoneId = await db.SchoolInformation.AsNoTracking().Select(x => x.TimeZoneId).SingleOrDefaultAsync(ct) ?? "UTC";
        var schoolLocalDate = clock.Now(timeZoneId).Date;
        var scope = AccessibleSections(db, ct);
        if (programAcademicYearId.HasValue) scope = scope.Where(x => x.GradeOffering.ProgramAcademicYearId == programAcademicYearId);
        if (academicYearDefinitionId.HasValue) scope = scope.Where(x => x.GradeOffering.ProgramAcademicYear.AcademicYearDefinitionId == academicYearDefinitionId);
        if (academicTermId.HasValue) scope = scope.Where(x => x.GradeOffering.ProgramAcademicYear.Terms.Any(t => t.Id == academicTermId && t.IsActive));
        var sections = await scope
            .OrderByDescending(x => x.GradeOffering.ProgramAcademicYear.StartDate).ThenBy(x => x.NameAr)
            .Select(x => new TeachingPlanClassOption(x.Id, x.NameAr, x.NameEn, x.GradeOfferingId,
                x.GradeOffering.ProgramAcademicYearId, x.GradeOffering.ProgramAcademicYear.AcademicYearDefinition.NameAr, x.GradeOffering.ProgramAcademicYear.AcademicYearDefinition.NameEn,
                x.GradeOffering.ProgramAcademicYear.StartDate, x.GradeOffering.ProgramAcademicYear.EndDate,
                x.GradeOffering.GradeLevel.NameAr, x.GradeOffering.GradeLevel.NameEn, schoolLocalDate,
                x.GradeOffering.SubjectOfferings.Where(s => s.IsActive && s.Status != GradeSubjectOfferingStatus.Closed)
                    .OrderBy(s => s.CurriculumGradeSubject.SortOrder)
                    .Select(s => new TeachingPlanSubjectOption(s.Id, s.CurriculumGradeSubject.Subject.NameAr, s.CurriculumGradeSubject.Subject.NameEn)).ToArray(),
                x.GradeOffering.ProgramAcademicYear.Terms.Where(t => t.IsActive).OrderBy(t => t.SortOrder)
                    .Select(t => new TeachingPlanTermOption(t.Id, t.NameAr, t.NameEn, t.StartDate, t.EndDate) { Periods = t.Periods.Where(p => p.IsActive && p.StartDate >= t.StartDate && p.EndDate <= t.EndDate).OrderBy(p => p.SortOrder).ThenBy(p => p.StartDate).Select(p => new TeachingPlanPeriodOption(p.Id,p.NameAr,p.NameEn,p.StartDate,p.EndDate)).ToArray() }).ToArray()))
            .ToArrayAsync(ct);
        return Ok(Success(sections));
    }

    [Authorize(Policy = SchoolPermissionPolicies.AcademicsView), HttpGet]
    public async Task<IActionResult> List([FromQuery] Guid? classSectionId, [FromQuery] Guid? gradeOfferingId, [FromQuery] TeachingPlanType? type, CancellationToken ct, int? pageNumber = null, int pageSize = 20, bool annualOnly = false, Guid? programAcademicYearId = null, Guid? academicTermId = null, Guid? academicYearDefinitionId = null)
    {
        await using var db = await RequireDb(ct); if (db is null) return Unauthorized();
        var accessibleIds = AccessibleSections(db, ct).Select(x => x.Id);
        var query = db.TeachingPlans.AsNoTracking().Where(x =>
            (!x.Targets.Any() && db.ClassSections.Any(s => accessibleIds.Contains(s.Id) && s.GradeOfferingId == x.GradeOfferingId)) ||
            x.Targets.Any(t => accessibleIds.Contains(t.ClassSectionId)));
        if (classSectionId.HasValue)
        {
            if (!await accessibleIds.AnyAsync(id => id == classSectionId.Value, ct))
                return NotFound(Failure(404, "plans.class_not_found", "Class section was not found."));
            query = query.Where(x => x.Targets.Any(t => t.ClassSectionId == classSectionId) ||
                (!x.Targets.Any() && (x.Type == TeachingPlanType.Yearly || x.Type == TeachingPlanType.Term) &&
                 db.ClassSections.Any(s => s.Id == classSectionId && s.GradeOfferingId == x.GradeOfferingId)));
        }
        if (gradeOfferingId.HasValue) query = query.Where(x => x.GradeOfferingId == gradeOfferingId);
        if (type.HasValue) query = query.Where(x => x.Type == type);
        if (programAcademicYearId.HasValue) query = query.Where(x => x.ProgramAcademicYearId == programAcademicYearId);
        if (academicYearDefinitionId.HasValue) query = query.Where(x => x.ProgramAcademicYear.AcademicYearDefinitionId == academicYearDefinitionId);
        if (academicTermId.HasValue) query = query.Where(x => x.AcademicTermId == academicTermId);
        if (!SchoolListPaging.Valid(pageNumber,pageSize)) return BadRequest();
        if(annualOnly)query=query.Where(x=>x.Type==TeachingPlanType.Yearly||x.Type==TeachingPlanType.Term);
        var window=await SchoolListPaging.Window(query.OrderByDescending(x=>x.FromDate).ThenBy(x=>x.TitleAr).ThenBy(x=>x.Id),pageNumber,pageSize,ct);
        var rows = await window.Query
            .Select(x => new TeachingPlanSummary(x.Id, x.Type, x.Status, x.TitleAr, x.TitleEn, x.FromDate, x.ToDate,
                x.GradeSubjectOffering.CurriculumGradeSubject.Subject.NameAr,
                x.GradeSubjectOffering.CurriculumGradeSubject.Subject.NameEn,
                x.Targets.Select(t => new TeachingPlanTargetResponse(t.ClassSectionId, t.ClassSection.NameAr, t.ClassSection.NameEn)).ToArray(),
                x.Documents.Count, x.Revision)).ToArrayAsync(ct);
        return Ok(Success(pageNumber.HasValue ? window.Result(rows) : rows));
    }

    [Authorize(Policy = SchoolPermissionPolicies.AcademicsView), HttpGet("{id:guid}")]
    public async Task<IActionResult> Details(Guid id, CancellationToken ct)
    {
        await using var db = await RequireDb(ct); if (db is null) return Unauthorized();
        if (!await CanAccessPlan(db, id, ct)) return NotFound(Failure(404, "plans.not_found", "Teaching plan was not found."));
        var row = await db.TeachingPlans.AsNoTracking().Where(x => x.Id == id).Select(x => new TeachingPlanDetails(
            x.Id, x.Type, x.Status, x.TitleAr, x.TitleEn, x.Details, x.SourceAuthority, x.SourceReference,
            x.ProgramAcademicYearId, x.GradeOfferingId, x.GradeSubjectOfferingId, x.AcademicTermId, x.ParentPlanId,
            x.FromDate, x.ToDate, x.Revision,
            x.Targets.OrderBy(t => t.ClassSection.NameAr).Select(t => new TeachingPlanTargetResponse(t.ClassSectionId, t.ClassSection.NameAr, t.ClassSection.NameEn)).ToArray(),
            x.Items.OrderBy(i => i.SortOrder).Select(i => new TeachingPlanItemResponse(i.Id, i.SortOrder, i.Title, i.Details, i.FromDate, i.ToDate)).ToArray(),
            x.Documents.OrderBy(d => d.SortOrder).Select(d => new TeachingPlanDocumentResponse(d.DocumentId, d.Purpose, d.Document.Title,
                d.Document.Versions.OrderByDescending(v => v.VersionNumber).Select(v => v.OriginalFileName).First(),
                d.Document.Versions.OrderByDescending(v => v.VersionNumber).Select(v => v.SizeBytes).First())).ToArray()
        )).SingleAsync(ct);
        var returnReason = await db.TeachingPlanAudits.AsNoTracking().Where(a => a.TeachingPlanId == id && a.Action == "Returned")
            .OrderByDescending(a => a.CreatedAtUtc).Select(a => a.Details).FirstOrDefaultAsync(ct);
        var days = await db.Set<TeachingPlanDayDetail>().AsNoTracking().Where(x => x.TeachingPlanId == id).OrderBy(x => x.Date)
            .Select(x => new SaveTeachingPlanDayRequest(x.Date, x.Details)).ToArrayAsync(ct);
        return Ok(Success(row with { ReturnReason = returnReason, DayDetails = days }));
    }

    [Authorize(Policy = SchoolPermissionPolicies.AcademicsManage), HttpPost]
    public async Task<IActionResult> Create([FromBody] SaveTeachingPlanRequest request, CancellationToken ct)
    {
        await using var db = await RequireDb(ct); if (db is null) return Unauthorized();
        request = await ResolveCalendarPeriod(db, request, ct);
        var validation = await Validate(db, request, null, ct); if (validation.Error is not null) return validation.Error;
        var now = DateTimeOffset.UtcNow; var userId = CurrentUserId();
        var plan = new TeachingPlan { Id = Guid.NewGuid(), Type = request.Type, ProgramAcademicYearId = validation.ProgramYearId,
            GradeOfferingId = validation.GradeOfferingId, GradeSubjectOfferingId = request.GradeSubjectOfferingId,
            AcademicTermId = request.AcademicTermId, ParentPlanId = request.ParentPlanId, Details = request.Details.Trim(),
            SourceAuthority = request.SourceAuthority, SourceReference = Clean(request.SourceReference), FromDate = request.FromDate,
            ToDate = request.ToDate, CreatedByUserId = userId, CreatedAtUtc = now, UpdatedAtUtc = now };
        var year = await db.ProgramAcademicYears.Where(x => x.Id == validation.ProgramYearId).Select(x => new { x.AcademicYearDefinition.NameAr, x.AcademicYearDefinition.NameEn }).SingleAsync(ct);
        var period = await PlanPeriod(db, request, ct);
        (plan.TitleAr, plan.TitleEn) = Titles(request.Type, request.FromDate, request.ToDate, validation.SubjectAr, validation.SubjectEn, validation.TermAr, validation.TermEn, validation.TermStart, year.NameAr, year.NameEn, period?.NameAr, period?.NameEn);
        AddChildren(plan, request, now); ApplyDays(db, plan, request.DayDetails); db.TeachingPlans.Add(plan); Audit(db, plan.Id, "Created", userId, now);
        try { await db.SaveChangesAsync(ct); }
        catch (DbUpdateConcurrencyException) { return Conflict(Failure(409, "plans.changed", "The plan changed. Reload its current state before continuing.")); }
        return Ok(Success(new { plan.Id, plan.TitleAr, plan.TitleEn }));
    }

    [Authorize(Policy = SchoolPermissionPolicies.AcademicsManage), HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] SaveTeachingPlanRequest request, CancellationToken ct)
    {
        await using var db = await RequireDb(ct); if (db is null) return Unauthorized();
        var plan = await db.TeachingPlans.Include(x => x.Targets).Include(x => x.Items).Include(x => x.DayDetails).SingleOrDefaultAsync(x => x.Id == id, ct);
        if (plan is null || !await CanAccessPlan(db, id, ct)) return NotFound(Failure(404, "plans.not_found", "Teaching plan was not found."));
        if (plan.Status != TeachingPlanStatus.Draft) return Conflict(Failure(409, "plans.draft_only", "Only a draft plan can be edited."));
        if (request.DayDetails is not null && request.ExpectedRevision is null) return Conflict(Failure(409, "plans.revision_required", "Send the current plan revision when editing days."));
        if (request.ExpectedRevision.HasValue && request.ExpectedRevision != plan.Revision) return Conflict(Failure(409, "plans.changed", "Reload the latest plan before editing."));
        if (request.DayDetails is null && plan.DayDetails.Count > 0)
            request = request with { DayDetails = plan.DayDetails.Select(x => new SaveTeachingPlanDayRequest(x.Date, x.Details)).ToArray() };
        request = await ResolveCalendarPeriod(db, request, ct);
        var validation = await Validate(db, request, id, ct); if (validation.Error is not null) return validation.Error;
        plan.Type=request.Type; plan.ProgramAcademicYearId=validation.ProgramYearId; plan.GradeOfferingId=validation.GradeOfferingId;
        plan.GradeSubjectOfferingId=request.GradeSubjectOfferingId; plan.AcademicTermId=request.AcademicTermId; plan.ParentPlanId=request.ParentPlanId;
        plan.Details=request.Details.Trim(); plan.SourceAuthority=request.SourceAuthority; plan.SourceReference=Clean(request.SourceReference);
        plan.FromDate=request.FromDate; plan.ToDate=request.ToDate; plan.UpdatedAtUtc=DateTimeOffset.UtcNow; plan.Revision++;
        var year = await db.ProgramAcademicYears.Where(x => x.Id == validation.ProgramYearId).Select(x => new { x.AcademicYearDefinition.NameAr, x.AcademicYearDefinition.NameEn }).SingleAsync(ct);
        var period = await PlanPeriod(db, request, ct);
        (plan.TitleAr, plan.TitleEn)=Titles(request.Type,request.FromDate,request.ToDate,validation.SubjectAr,validation.SubjectEn,validation.TermAr,validation.TermEn,validation.TermStart,year.NameAr,year.NameEn,period?.NameAr,period?.NameEn);
        db.TeachingPlanTargets.RemoveRange(plan.Targets); db.TeachingPlanItems.RemoveRange(plan.Items); plan.Targets=[]; plan.Items=[];
        AddChildren(plan,request,plan.UpdatedAtUtc); ApplyDays(db, plan, request.DayDetails); Audit(db,id,"Updated",CurrentUserId(),plan.UpdatedAtUtc);
        try { await db.SaveChangesAsync(ct); }
        catch (DbUpdateConcurrencyException) { return Conflict(Failure(409, "plans.changed", "The plan changed. Reload its current state before continuing.")); }
        return Ok(Success(new { plan.Id, plan.TitleAr, plan.TitleEn }));
    }

    [Authorize(Policy = SchoolPermissionPolicies.AcademicsManage), HttpPost("{id:guid}/{action}")]
    public async Task<IActionResult> Transition(Guid id, string action, CancellationToken ct)
    {
        await using var db = await RequireDb(ct); if (db is null) return Unauthorized();
        var plan = await db.TeachingPlans.Include(x => x.Targets).SingleOrDefaultAsync(x => x.Id == id, ct);
        if (plan is null || !await CanAccessPlan(db,id,ct)) return NotFound(Failure(404,"plans.not_found","Teaching plan was not found."));
        var next = action.ToLowerInvariant() switch
        {
            "submit" when plan.Status == TeachingPlanStatus.Draft => TeachingPlanStatus.Submitted,
            "approve" when plan.Status == TeachingPlanStatus.Submitted => TeachingPlanStatus.Approved,
            "publish" when plan.Status == TeachingPlanStatus.Approved => TeachingPlanStatus.Published,
            "archive" when plan.Status == TeachingPlanStatus.Published => TeachingPlanStatus.Archived,
            _ => (TeachingPlanStatus?)null
        };
        if (next is null) return Conflict(Failure(409,"plans.invalid_transition","The requested plan transition is not allowed."));
        if (next == TeachingPlanStatus.Published && await HasPublishedOverlap(db,plan,ct))
            return Conflict(Failure(409,"plans.overlap","A published plan already covers the same subject, scope and period."));
        var now=DateTimeOffset.UtcNow; var user=CurrentUserId(); plan.Status=next.Value; plan.UpdatedAtUtc=now; plan.Revision++;
        if(next==TeachingPlanStatus.Submitted){plan.SubmittedAtUtc=now;plan.SubmittedByUserId=user;}
        if(next==TeachingPlanStatus.Approved){plan.ApprovedAtUtc=now;plan.ApprovedByUserId=user;}
        if(next==TeachingPlanStatus.Published){plan.PublishedAtUtc=now;plan.PublishedByUserId=user;}
        await SchoolOperationNotifications.Plan(db, plan, action.ToLowerInvariant(), user, now, ct);
        Audit(db,id,next.Value.ToString(),user,now);
        try { await db.SaveChangesAsync(ct); }
        catch (DbUpdateConcurrencyException) { return Conflict(Failure(409, "plans.changed", "The plan changed. Reload its current state before continuing.")); }
        return Ok(Success(new { status=next.Value }));
    }

    [Authorize(Policy = SchoolPermissionPolicies.AcademicsManage), HttpPost("{id:guid}/return-for-edit")]
    public async Task<IActionResult> ReturnForEdit(Guid id, [FromBody] ReturnTeachingPlanRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Reason) || request.Reason.Trim().Length > 1000)
            return BadRequest(Failure(400, "plans.return_reason_required", "Enter a return reason of up to 1000 characters."));
        await using var db = await RequireDb(ct); if (db is null) return Unauthorized();
        var plan = await db.TeachingPlans.Include(x => x.Targets).SingleOrDefaultAsync(x => x.Id == id, ct);
        if (plan is null || !await CanAccessPlan(db, id, ct)) return NotFound(Failure(404, "plans.not_found", "Teaching plan was not found."));
        if (plan.Status != TeachingPlanStatus.Submitted) return Conflict(Failure(409, "plans.invalid_transition", "Only a submitted plan can be returned."));
        var now = DateTimeOffset.UtcNow; var actor = CurrentUserId();
        plan.Status = TeachingPlanStatus.Draft; plan.Revision++; plan.UpdatedAtUtc = now;
        db.TeachingPlanAudits.Add(new() { Id = Guid.NewGuid(), TeachingPlanId = id, Action = "Returned", Details = request.Reason.Trim(), ActorUserId = actor, CreatedAtUtc = now });
        await SchoolOperationNotifications.Plan(db, plan, "return", actor, now, ct);
        try { await db.SaveChangesAsync(ct); }
        catch (DbUpdateConcurrencyException) { return Conflict(Failure(409, "plans.changed", "The plan changed. Reload its current state before continuing.")); }
        return Ok(Success(new { status = plan.Status }));
    }

    [Authorize(Policy = SchoolPermissionPolicies.AcademicsManage), RequestSizeLimit(30_000_000), HttpPost("{planId:guid}/documents")]
    public async Task<IActionResult> Upload(Guid planId, IFormFile file, [FromForm] TeachingPlanDocumentPurpose purpose = TeachingPlanDocumentPurpose.Supporting, CancellationToken ct = default)
    {
        await using var db = await RequireDb(ct); if (db is null) return Unauthorized();
        var plan = await db.TeachingPlans.SingleOrDefaultAsync(x => x.Id == planId, ct);
        if (plan is null || !await CanAccessPlan(db,planId,ct)) return NotFound(Failure(404,"plans.not_found","Teaching plan was not found."));
        if (plan.Status != TeachingPlanStatus.Draft) return Conflict(Failure(409,"plans.draft_only","Attachments can only be changed while the plan is a draft."));
        var max=configuration.GetValue<long?>("SchoolDocuments:MaxFileSizeBytes") ?? 26_214_400;
        var extension=Path.GetExtension(file.FileName); if(file.Length<1 || file.Length>max || !AllowedExtensions.Contains(extension))
            return BadRequest(Failure(400,"documents.invalid_file","The file type or size is not allowed."));
        var documentId=Guid.NewGuid(); var versionId=Guid.NewGuid(); var now=DateTimeOffset.UtcNow; var user=CurrentUserId();
        await using var input=file.OpenReadStream(); var stored=await storage.SaveAsync(SchoolCode(),documentId,versionId,input,extension,ct);
        var document=new SchoolDocument{Id=documentId,Title=Path.GetFileNameWithoutExtension(file.FileName),CreatedByUserId=user,CreatedAtUtc=now,UpdatedAtUtc=now};
        document.Versions.Add(new SchoolDocumentVersion{Id=versionId,VersionNumber=1,OriginalFileName=Path.GetFileName(file.FileName),ContentType=file.ContentType,
            SizeBytes=stored.SizeBytes,Sha256=stored.Sha256,StorageKey=stored.StorageKey,ValidationStatus=SchoolDocumentValidationStatus.Pending,CreatedByUserId=user,CreatedAtUtc=now});
        db.Documents.Add(document); db.TeachingPlanDocuments.Add(new TeachingPlanDocument{Id=Guid.NewGuid(),TeachingPlanId=planId,DocumentId=documentId,Purpose=purpose,SortOrder=await db.TeachingPlanDocuments.CountAsync(x=>x.TeachingPlanId==planId,ct)});
        db.DocumentAudits.Add(new SchoolDocumentAudit{Id=Guid.NewGuid(),DocumentId=documentId,Action="Uploaded",ActorUserId=user,CreatedAtUtc=now});
        await db.SaveChangesAsync(ct); return Ok(Success(new { documentId, document.Title, fileName=file.FileName, stored.SizeBytes }));
    }

    [Authorize(Policy = SchoolPermissionPolicies.AcademicsView), HttpGet("documents/{documentId:guid}/content")]
    public async Task<IActionResult> Download(Guid documentId, CancellationToken ct)
    {
        await using var db=await RequireDb(ct); if(db is null)return Unauthorized();
        var planIds=await db.TeachingPlanDocuments.Where(x=>x.DocumentId==documentId).Select(x=>x.TeachingPlanId).ToArrayAsync(ct);
        var allowed=false; foreach(var id in planIds) if(await CanAccessPlan(db,id,ct)){allowed=true;break;}
        if(!allowed)return NotFound(Failure(404,"documents.not_found","Document was not found."));
        var version=await db.DocumentVersions.AsNoTracking().Where(x=>x.DocumentId==documentId && x.ValidationStatus!=SchoolDocumentValidationStatus.Rejected)
            .OrderByDescending(x=>x.VersionNumber).FirstOrDefaultAsync(ct);
        if(version is null)return NotFound(Failure(404,"documents.not_found","Document was not found."));
        var stream=await storage.OpenReadAsync(version.StorageKey,ct); if(stream is null)return NotFound(Failure(404,"documents.content_missing","Document content was not found."));
        return File(stream,version.ContentType,version.OriginalFileName);
    }

    private async Task<(IActionResult? Error, Guid ProgramYearId, Guid GradeOfferingId, string SubjectAr, string SubjectEn, string? TermAr, string? TermEn, DateOnly? TermStart)> Validate(SchoolsDbContext db, SaveTeachingPlanRequest r, Guid? editingId, CancellationToken ct)
    {
        var hasDays = r.DayDetails is { Count: > 0 };
        if ((r.Type != TeachingPlanType.Weekly && hasDays) || (hasDays && !WeeklyPlanDayRules.IsValid(r.FromDate, r.ToDate, r.DayDetails!.Select(x => (x.Date, x.Details)).ToArray())) ||
            (r.Type == TeachingPlanType.Weekly && (r.DayDetails is not null || editingId is null) && !hasDays))
            return (BadRequest(Failure(400,"plans.days_invalid","A weekly plan requires at least one unique day within its week, with up to 4000 characters per day.")),default,default,"","",null,null,null);
        if(r.FromDate>r.ToDate || (!hasDays && string.IsNullOrWhiteSpace(r.Details)) || r.Details.Trim().Length > 8000)return (BadRequest(Failure(400,"plans.invalid_period","Plan details of up to 8000 characters and a valid date range are required.")),default,default,"","",null,null,null);
        var offering=await db.GradeSubjectOfferings.AsNoTracking().Where(x=>x.Id==r.GradeSubjectOfferingId && x.IsActive && x.GradeOffering.IsActive)
            .Select(x=>new{x.GradeOfferingId,x.GradeOffering.ProgramAcademicYearId,YearStart=x.GradeOffering.ProgramAcademicYear.StartDate,YearEnd=x.GradeOffering.ProgramAcademicYear.EndDate,
                SubjectAr=x.CurriculumGradeSubject.Subject.NameAr,SubjectEn=x.CurriculumGradeSubject.Subject.NameEn}).SingleOrDefaultAsync(ct);
        if(offering is null)return (BadRequest(Failure(400,"plans.subject_invalid","The selected grade subject is unavailable.")),default,default,"","",null,null,null);
        if(r.FromDate<offering.YearStart || r.ToDate>offering.YearEnd)return (BadRequest(Failure(400,"plans.outside_year","The plan period must be inside the academic year.")),default,default,"","",null,null,null);
        var classScoped=r.Type is TeachingPlanType.Monthly or TeachingPlanType.Weekly or TeachingPlanType.Daily;
        var ids=r.ClassSectionIds.Distinct().ToArray(); if(classScoped && ids.Length==0 || !classScoped && ids.Length>0)
            return (BadRequest(Failure(400,"plans.scope_invalid","The selected plan type has an invalid class scope.")),default,default,"","",null,null,null);
        if(ids.Length>0){var accessible=await AccessibleSections(db,ct).Where(x=>ids.Contains(x.Id)&&x.GradeOfferingId==offering.GradeOfferingId).Select(x=>x.Id).ToArrayAsync(ct);
            if(accessible.Length!=ids.Length)return (Forbid(),default,default,"","",null,null,null);}
        string? termAr=null,termEn=null; DateOnly? termStart=null;
        if(r.Type!=TeachingPlanType.Yearly){if(!r.AcademicTermId.HasValue)return (BadRequest(Failure(400,"plans.term_required","An academic term is required.")),default,default,"","",null,null,null);
            var term=await db.AcademicTerms.AsNoTracking().Where(x=>x.Id==r.AcademicTermId && x.ProgramAcademicYearId==offering.ProgramAcademicYearId && x.IsActive).Select(x=>new{x.NameAr,x.NameEn,x.StartDate,x.EndDate}).SingleOrDefaultAsync(ct);
            if(term is null || r.FromDate<term.StartDate || r.ToDate>term.EndDate)return (BadRequest(Failure(400,"plans.outside_term","The plan period must be inside the selected term.")),default,default,"","",null,null,null);
            if(r.Type is TeachingPlanType.Monthly or TeachingPlanType.Weekly)
            {
                var unchangedLegacyPeriod = editingId.HasValue && await db.TeachingPlans.AnyAsync(x => x.Id == editingId && x.Type == r.Type && x.AcademicTermId == r.AcademicTermId && x.FromDate == r.FromDate && x.ToDate == r.ToDate, ct);
                var periods = await db.AcademicPeriods.AsNoTracking().Where(p => p.IsActive && p.AcademicTermId == r.AcademicTermId && p.StartDate >= term.StartDate && p.EndDate <= term.EndDate).ToArrayAsync(ct);
                var valid = periods.Any(p => TeachingPlanPeriodRules.Matches(r.Type,r.FromDate,r.ToDate,p.StartDate,p.EndDate));
                if(!unchangedLegacyPeriod && !valid)return (BadRequest(Failure(400,"plans.period_invalid","Choose an active configured term period and a week within it.")),default,default,"","",null,null,null);
            }
            termAr=term.NameAr;termEn=term.NameEn;termStart=term.StartDate;}
        if(r.Type==TeachingPlanType.Daily && r.FromDate!=r.ToDate)return (BadRequest(Failure(400,"plans.daily_single_day","A daily plan must cover one day.")),default,default,"","",null,null,null);
        if(r.Type==TeachingPlanType.Weekly && r.ToDate.DayNumber-r.FromDate.DayNumber>6)return (BadRequest(Failure(400,"plans.week_too_long","A weekly plan cannot exceed seven days.")),default,default,"","",null,null,null);
        return (null,offering.ProgramAcademicYearId,offering.GradeOfferingId,offering.SubjectAr,offering.SubjectEn,termAr,termEn,termStart);
    }

    private static void AddChildren(TeachingPlan plan, SaveTeachingPlanRequest request, DateTimeOffset now)
    {
        foreach(var id in request.ClassSectionIds.Distinct()) plan.Targets.Add(new TeachingPlanTarget{Id=Guid.NewGuid(),ClassSectionId=id});
        var order=0; foreach(var item in request.Items.Where(x=>!string.IsNullOrWhiteSpace(x.Title))) plan.Items.Add(new TeachingPlanItem{Id=Guid.NewGuid(),SortOrder=order++,Title=item.Title.Trim(),Details=Clean(item.Details),FromDate=item.FromDate,ToDate=item.ToDate});
    }
    private static void ApplyDays(SchoolsDbContext db, TeachingPlan plan, IReadOnlyList<SaveTeachingPlanDayRequest>? days)
    {
        if (days is null) return;
        var removed = plan.DayDetails.Where(x => !days.Any(d => d.Date == x.Date)).ToArray();
        db.RemoveRange(removed); foreach (var row in removed) plan.DayDetails.Remove(row);
        foreach (var day in days)
        {
            var row = plan.DayDetails.SingleOrDefault(x => x.Date == day.Date);
            if (row is null) { row = new() { Id = Guid.NewGuid(), Date = day.Date, DayOfWeek = day.Date.DayOfWeek }; plan.DayDetails.Add(row); }
            row.Details = day.Details.Trim();
        }
    }

    [Authorize(Policy = SchoolPermissionPolicies.AcademicsView), HttpGet("week-days")]
    public async Task<IActionResult> WeekDays(Guid gradeSubjectOfferingId, DateOnly fromDate, DateOnly toDate, CancellationToken ct)
    {
        await using var db = await RequireDb(ct); if (db is null) return Unauthorized();
        if (fromDate > toDate || toDate.DayNumber - fromDate.DayNumber > 6) return BadRequest(Failure(400,"plans.days_invalid","Choose one week."));
        var scope = await db.GradeSubjectOfferings.Where(x => x.Id == gradeSubjectOfferingId && x.IsActive)
            .Select(x => new { x.GradeOfferingId, YearId = x.GradeOffering.ProgramAcademicYearId, ProgramId = x.GradeOffering.ProgramAcademicYear.EducationProgramId,
                Start = x.GradeOffering.ProgramAcademicYear.StartDate, End = x.GradeOffering.ProgramAcademicYear.EndDate }).SingleOrDefaultAsync(ct);
        if (scope is null || fromDate < scope.Start || toDate > scope.End || !await AccessibleSections(db,ct).AnyAsync(x => x.GradeOfferingId == scope.GradeOfferingId,ct)) return Forbid();
        var studyDays = await db.SchoolDaySchedules.Where(x => x.IsActive && x.BranchId == null && x.EducationProgramId == scope.ProgramId).Select(x => x.DayOfWeek).Distinct().ToArrayAsync(ct);
        var closures = await db.SchoolCalendarEvents.Where(x => x.IsActive && x.IsSchoolClosed && x.BranchId == null && x.StartDate <= toDate && x.EndDate >= fromDate &&
            (x.EducationProgramId == null || x.EducationProgramId == scope.ProgramId) && (x.ProgramAcademicYearId == null || x.ProgramAcademicYearId == scope.YearId))
            .Select(x => new { x.StartDate, x.EndDate, x.NameAr, x.NameEn }).ToArrayAsync(ct);
        return Ok(Success(Enumerable.Range(0,toDate.DayNumber-fromDate.DayNumber+1).Select(i => {
            var date = fromDate.AddDays(i); var closure = closures.FirstOrDefault(x => x.StartDate <= date && x.EndDate >= date);
            return new { Date = date, DayOfWeek = (int)date.DayOfWeek, IsStudyDay = studyDays.Length == 0 || studyDays.Contains(date.DayOfWeek), IsClosed = closure is not null, ClosureNameAr = closure?.NameAr, ClosureNameEn = closure?.NameEn };
        }).ToArray()));
    }
    private static async Task<SaveTeachingPlanRequest> ResolveCalendarPeriod(SchoolsDbContext db, SaveTeachingPlanRequest request, CancellationToken ct)
    {
        if (request.Type is not (TeachingPlanType.Yearly or TeachingPlanType.Term)) return request;
        var yearId = await db.GradeSubjectOfferings.Where(x => x.Id == request.GradeSubjectOfferingId)
            .Select(x => (Guid?)x.GradeOffering.ProgramAcademicYearId).SingleOrDefaultAsync(ct);
        if (yearId is null) return request;
        if (request.Type == TeachingPlanType.Yearly)
        {
            var year = await db.ProgramAcademicYears.Where(x => x.Id == yearId && x.IsActive)
                .Select(x => new { x.StartDate, x.EndDate }).SingleOrDefaultAsync(ct);
            if (year is not null) return request with { FromDate = year.StartDate, ToDate = year.EndDate, AcademicTermId = null };
        }
        else
        {
            var term = await db.AcademicTerms.Where(x => x.Id == request.AcademicTermId && x.ProgramAcademicYearId == yearId && x.IsActive)
                .Select(x => new { x.StartDate, x.EndDate }).SingleOrDefaultAsync(ct);
            if (term is not null) return request with { FromDate = term.StartDate, ToDate = term.EndDate };
        }
        return request with { FromDate = DateOnly.MaxValue, ToDate = DateOnly.MinValue };
    }

    private static Task<AcademicPeriod?> PlanPeriod(SchoolsDbContext db, SaveTeachingPlanRequest r, CancellationToken ct) =>
        db.AcademicPeriods.AsNoTracking().Where(p => p.IsActive && p.AcademicTermId == r.AcademicTermId && p.StartDate <= r.FromDate && p.EndDate >= r.ToDate).OrderBy(p => p.SortOrder).ThenBy(p => p.StartDate).FirstOrDefaultAsync(ct);

    private static (string,string) Titles(TeachingPlanType type,DateOnly from,DateOnly to,string subjectAr,string subjectEn,string? termAr,string? termEn,DateOnly? termStart,string yearAr,string yearEn,string? periodAr=null,string? periodEn=null)
    {
        var weekNumber=termStart.HasValue?((WeeklyPlanDayRules.SaturdayStart(from).DayNumber-WeeklyPlanDayRules.SaturdayStart(termStart.Value).DayNumber)/7)+1:((from.Day-1)/7)+1;
        var ar=type switch{TeachingPlanType.Yearly=>$"خطة عام {yearAr}",TeachingPlanType.Term=>$"خطة {termAr} لعام {yearAr}",TeachingPlanType.Monthly=>$"خطة شهر {ArabicMonths[from.Month-1]} لعام {yearAr}",TeachingPlanType.Weekly=>$"خطة الأسبوع {weekNumber} من شهر {ArabicMonths[from.Month-1]} {from.Year}",TeachingPlanType.Daily=>$"خطة يوم {from.Day} {ArabicMonths[from.Month-1]} {from.Year}",_=>"خطة تدريس"};
        var culture=CultureInfo.GetCultureInfo("en-US"); var en=type switch{TeachingPlanType.Yearly=>"Yearly plan",TeachingPlanType.Term=>$"{termEn} plan",TeachingPlanType.Monthly=>$"{from.ToString("MMMM yyyy",culture)} plan",TeachingPlanType.Weekly=>$"Week {weekNumber} - {from.ToString("MMMM yyyy",culture)} plan",TeachingPlanType.Daily=>$"Daily plan - {from.ToString("d MMMM yyyy",culture)}",_=>"Teaching plan"};
        if (periodAr is not null && type == TeachingPlanType.Monthly) return ($"خطة شهر {periodAr} لعام {yearAr}", $"{periodEn} plan for academic year {yearEn}");
        if (periodAr is not null && type == TeachingPlanType.Weekly) return ($"خطة الأسبوع {weekNumber} من شهر {periodAr} لعام {yearAr} — {subjectAr}", $"Week {weekNumber} - {periodEn} for academic year {yearEn} — {subjectEn}");
        return type switch
        {
            TeachingPlanType.Yearly => (ar, $"Plan for academic year {yearEn}"),
            TeachingPlanType.Term => (ar, $"{termEn} plan for academic year {yearEn}"),
            TeachingPlanType.Monthly => (ar, $"{from.ToString("MMMM", culture)} plan for academic year {yearEn}"),
            _ => ($"{ar} — {subjectAr}", $"{en} — {subjectEn}")
        };
    }
    private async Task<bool> HasPublishedOverlap(SchoolsDbContext db,TeachingPlan plan,CancellationToken ct)
    {
        var targetIds=plan.Targets.Select(x=>x.ClassSectionId).ToArray();
        return await db.TeachingPlans.AnyAsync(x=>x.Id!=plan.Id&&x.Status==TeachingPlanStatus.Published&&x.Type==plan.Type&&x.GradeSubjectOfferingId==plan.GradeSubjectOfferingId&&x.FromDate<=plan.ToDate&&x.ToDate>=plan.FromDate&&
            (targetIds.Length==0?!x.Targets.Any():x.Targets.Any(t=>targetIds.Contains(t.ClassSectionId))),ct);
    }
    private IQueryable<ClassSection> AccessibleSections(SchoolsDbContext db,CancellationToken ct)
    {
        var user=CurrentUserId(); if(User.IsInRole(SchoolIdentitySeed.SchoolAdminRoleCode))return db.ClassSections.Where(x=>x.IsActive&&x.GradeOffering.IsActive);
        var today=DateOnly.FromDateTime(DateTime.UtcNow);
        return db.ClassSections.Where(x=>x.IsActive&&x.GradeOffering.IsActive&&db.ClassSectionTeacherScopes.Any(s=>s.ClassSectionId==x.Id&&s.IsActive&&s.TeacherGradeSubjectScope.IsActive&&
            (s.TeacherGradeSubjectScope.TeacherUserId==user || s.TeacherGradeSubjectScope.TeacherUser.DepartmentMemberships.Any(m=>m.IsActive&&m.StartsOn<=today&&(!m.EndsOn.HasValue||m.EndsOn>=today)&&
                (m.Department.Leaderships.Any(l=>l.UserId==user&&l.IsActive&&l.StartsOn<=today&&(!l.EndsOn.HasValue||l.EndsOn>=today)) ||
                 m.Department.ParentDepartment!=null&&m.Department.ParentDepartment.Leaderships.Any(l=>l.UserId==user&&l.IsActive&&l.StartsOn<=today&&(!l.EndsOn.HasValue||l.EndsOn>=today)))))));
    }
    private async Task<bool> CanAccessPlan(SchoolsDbContext db,Guid id,CancellationToken ct)
    {
        if(User.IsInRole(SchoolIdentitySeed.SchoolAdminRoleCode))return await db.TeachingPlans.AnyAsync(x=>x.Id==id,ct);
        var ids=AccessibleSections(db,ct).Select(x=>x.Id);
        return await db.TeachingPlans.AnyAsync(x=>x.Id==id&&((!x.Targets.Any()&&db.ClassSections.Any(s=>ids.Contains(s.Id)&&s.GradeOfferingId==x.GradeOfferingId))||x.Targets.Any(t=>ids.Contains(t.ClassSectionId))),ct);
    }
    private static string? Clean(string? value)=>string.IsNullOrWhiteSpace(value)?null:value.Trim();
    private static void Audit(SchoolsDbContext db,Guid id,string action,Guid actor,DateTimeOffset now)=>db.TeachingPlanAudits.Add(new(){Id=Guid.NewGuid(),TeachingPlanId=id,Action=action,ActorUserId=actor,CreatedAtUtc=now});
    private async Task<SchoolsDbContext?> RequireDb(CancellationToken ct)=>await dbFactory.CreateAsync(SchoolCode(),ct);
    private string SchoolCode()=>User.FindFirst(SchoolClaimTypes.SchoolCode)?.Value??string.Empty;
    private Guid CurrentUserId()=>Guid.TryParse(User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value,out var id)?id:Guid.Empty;
    private ApiResponse<T> Success<T>(T data)=>ApiResponse<T>.Success(data,correlationId:HttpContext.TraceIdentifier);
    private ApiResponse<object?> Failure(int status,string code,string message)=>ApiResponse<object?>.Failure(status,code,message,correlationId:HttpContext.TraceIdentifier);
}

public sealed record SaveTeachingPlanRequest(TeachingPlanType Type,Guid GradeSubjectOfferingId,Guid? AcademicTermId,Guid? ParentPlanId,
    DateOnly FromDate,DateOnly ToDate,string Details,TeachingPlanSourceAuthority SourceAuthority,string? SourceReference,
    IReadOnlyList<Guid> ClassSectionIds,IReadOnlyList<SaveTeachingPlanItemRequest> Items)
{ public IReadOnlyList<SaveTeachingPlanDayRequest>? DayDetails { get; init; } public int? ExpectedRevision { get; init; } }
public sealed record SaveTeachingPlanDayRequest(DateOnly Date, string Details);
public sealed record SaveTeachingPlanItemRequest(string Title,string? Details,DateOnly? FromDate,DateOnly? ToDate);
public sealed record TeachingPlanClassOption(Guid Id,string NameAr,string NameEn,Guid GradeOfferingId,Guid ProgramAcademicYearId,string AcademicYearNameAr,string AcademicYearNameEn,DateOnly AcademicYearStart,DateOnly AcademicYearEnd,string GradeNameAr,string GradeNameEn,DateOnly SchoolLocalDate,IReadOnlyList<TeachingPlanSubjectOption> Subjects,IReadOnlyList<TeachingPlanTermOption> Terms);
public sealed record TeachingPlanSubjectOption(Guid Id,string NameAr,string NameEn);
public sealed record TeachingPlanTermOption(Guid Id,string NameAr,string NameEn,DateOnly StartDate,DateOnly EndDate)
{
    public IReadOnlyList<TeachingPlanPeriodOption> Periods { get; init; } = [];
}
public sealed record TeachingPlanPeriodOption(Guid Id,string NameAr,string NameEn,DateOnly StartDate,DateOnly EndDate);
public sealed record TeachingPlanTargetResponse(Guid ClassSectionId,string NameAr,string NameEn);
public sealed record TeachingPlanSummary(Guid Id,TeachingPlanType Type,TeachingPlanStatus Status,string TitleAr,string TitleEn,DateOnly FromDate,DateOnly ToDate,string SubjectNameAr,string SubjectNameEn,IReadOnlyList<TeachingPlanTargetResponse> Targets,int DocumentCount,int Revision);
public sealed record ReturnTeachingPlanRequest(string Reason);
public sealed record TeachingPlanItemResponse(Guid Id,int SortOrder,string Title,string? Details,DateOnly? FromDate,DateOnly? ToDate);
public sealed record TeachingPlanDocumentResponse(Guid DocumentId,TeachingPlanDocumentPurpose Purpose,string Title,string FileName,long SizeBytes);
public sealed record TeachingPlanDetails(Guid Id,TeachingPlanType Type,TeachingPlanStatus Status,string TitleAr,string TitleEn,string Details,TeachingPlanSourceAuthority SourceAuthority,string? SourceReference,Guid ProgramAcademicYearId,Guid GradeOfferingId,Guid GradeSubjectOfferingId,Guid? AcademicTermId,Guid? ParentPlanId,DateOnly FromDate,DateOnly ToDate,int Revision,IReadOnlyList<TeachingPlanTargetResponse> Targets,IReadOnlyList<TeachingPlanItemResponse> Items,IReadOnlyList<TeachingPlanDocumentResponse> Documents) { public string? ReturnReason { get; init; } public IReadOnlyList<SaveTeachingPlanDayRequest> DayDetails { get; init; } = []; }
