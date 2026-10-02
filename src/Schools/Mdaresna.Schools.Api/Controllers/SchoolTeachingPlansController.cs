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
    public async Task<IActionResult> Options(CancellationToken ct)
    {
        await using var db = await RequireDb(ct); if (db is null) return Unauthorized();
        var timeZoneId = await db.SchoolInformation.AsNoTracking().Select(x => x.TimeZoneId).SingleOrDefaultAsync(ct) ?? "UTC";
        var schoolLocalDate = clock.Now(timeZoneId).Date;
        var sections = await AccessibleSections(db, ct)
            .OrderByDescending(x => x.GradeOffering.ProgramAcademicYear.StartDate).ThenBy(x => x.NameAr)
            .Select(x => new TeachingPlanClassOption(x.Id, x.NameAr, x.NameEn, x.GradeOfferingId,
                x.GradeOffering.ProgramAcademicYearId, x.GradeOffering.ProgramAcademicYear.NameAr, x.GradeOffering.ProgramAcademicYear.NameEn,
                x.GradeOffering.ProgramAcademicYear.StartDate, x.GradeOffering.ProgramAcademicYear.EndDate,
                x.GradeOffering.GradeLevel.NameAr, x.GradeOffering.GradeLevel.NameEn, schoolLocalDate,
                x.GradeOffering.SubjectOfferings.Where(s => s.IsActive && s.Status != GradeSubjectOfferingStatus.Closed)
                    .OrderBy(s => s.CurriculumGradeSubject.SortOrder)
                    .Select(s => new TeachingPlanSubjectOption(s.Id, s.CurriculumGradeSubject.Subject.NameAr, s.CurriculumGradeSubject.Subject.NameEn)).ToArray(),
                x.GradeOffering.ProgramAcademicYear.Terms.Where(t => t.IsActive).OrderBy(t => t.SortOrder)
                    .Select(t => new TeachingPlanTermOption(t.Id, t.NameAr, t.NameEn, t.StartDate, t.EndDate)).ToArray()))
            .ToArrayAsync(ct);
        return Ok(Success(sections));
    }

    [Authorize(Policy = SchoolPermissionPolicies.AcademicsView), HttpGet]
    public async Task<IActionResult> List([FromQuery] Guid? classSectionId, [FromQuery] Guid? gradeOfferingId, [FromQuery] TeachingPlanType? type, CancellationToken ct)
    {
        await using var db = await RequireDb(ct); if (db is null) return Unauthorized();
        var accessibleIds = AccessibleSections(db, ct).Select(x => x.Id);
        var query = db.TeachingPlans.AsNoTracking().Where(x =>
            (!x.Targets.Any() && db.ClassSections.Any(s => accessibleIds.Contains(s.Id) && s.GradeOfferingId == x.GradeOfferingId)) ||
            x.Targets.Any(t => accessibleIds.Contains(t.ClassSectionId)));
        if (classSectionId.HasValue) query = query.Where(x => x.Targets.Any(t => t.ClassSectionId == classSectionId));
        if (gradeOfferingId.HasValue) query = query.Where(x => x.GradeOfferingId == gradeOfferingId);
        if (type.HasValue) query = query.Where(x => x.Type == type);
        var rows = await query.OrderByDescending(x => x.FromDate).ThenBy(x => x.TitleAr)
            .Select(x => new TeachingPlanSummary(x.Id, x.Type, x.Status, x.TitleAr, x.TitleEn, x.FromDate, x.ToDate,
                x.GradeSubjectOffering.CurriculumGradeSubject.Subject.NameAr,
                x.GradeSubjectOffering.CurriculumGradeSubject.Subject.NameEn,
                x.Targets.Select(t => new TeachingPlanTargetResponse(t.ClassSectionId, t.ClassSection.NameAr, t.ClassSection.NameEn)).ToArray(),
                x.Documents.Count, x.Revision)).ToArrayAsync(ct);
        return Ok(Success(rows));
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
        return Ok(Success(row));
    }

    [Authorize(Policy = SchoolPermissionPolicies.AcademicsManage), HttpPost]
    public async Task<IActionResult> Create([FromBody] SaveTeachingPlanRequest request, CancellationToken ct)
    {
        await using var db = await RequireDb(ct); if (db is null) return Unauthorized();
        var validation = await Validate(db, request, null, ct); if (validation.Error is not null) return validation.Error;
        var now = DateTimeOffset.UtcNow; var userId = CurrentUserId();
        var plan = new TeachingPlan { Id = Guid.NewGuid(), Type = request.Type, ProgramAcademicYearId = validation.ProgramYearId,
            GradeOfferingId = validation.GradeOfferingId, GradeSubjectOfferingId = request.GradeSubjectOfferingId,
            AcademicTermId = request.AcademicTermId, ParentPlanId = request.ParentPlanId, Details = request.Details.Trim(),
            SourceAuthority = request.SourceAuthority, SourceReference = Clean(request.SourceReference), FromDate = request.FromDate,
            ToDate = request.ToDate, CreatedByUserId = userId, CreatedAtUtc = now, UpdatedAtUtc = now };
        (plan.TitleAr, plan.TitleEn) = Titles(request.Type, request.FromDate, request.ToDate, validation.SubjectAr, validation.SubjectEn, validation.TermAr, validation.TermEn, validation.TermStart);
        AddChildren(plan, request, now); db.TeachingPlans.Add(plan); Audit(db, plan.Id, "Created", userId, now);
        await db.SaveChangesAsync(ct); return Ok(Success(new { plan.Id, plan.TitleAr, plan.TitleEn }));
    }

    [Authorize(Policy = SchoolPermissionPolicies.AcademicsManage), HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] SaveTeachingPlanRequest request, CancellationToken ct)
    {
        await using var db = await RequireDb(ct); if (db is null) return Unauthorized();
        var plan = await db.TeachingPlans.Include(x => x.Targets).Include(x => x.Items).SingleOrDefaultAsync(x => x.Id == id, ct);
        if (plan is null || !await CanAccessPlan(db, id, ct)) return NotFound(Failure(404, "plans.not_found", "Teaching plan was not found."));
        if (plan.Status != TeachingPlanStatus.Draft) return Conflict(Failure(409, "plans.draft_only", "Only a draft plan can be edited."));
        var validation = await Validate(db, request, id, ct); if (validation.Error is not null) return validation.Error;
        plan.Type=request.Type; plan.ProgramAcademicYearId=validation.ProgramYearId; plan.GradeOfferingId=validation.GradeOfferingId;
        plan.GradeSubjectOfferingId=request.GradeSubjectOfferingId; plan.AcademicTermId=request.AcademicTermId; plan.ParentPlanId=request.ParentPlanId;
        plan.Details=request.Details.Trim(); plan.SourceAuthority=request.SourceAuthority; plan.SourceReference=Clean(request.SourceReference);
        plan.FromDate=request.FromDate; plan.ToDate=request.ToDate; plan.UpdatedAtUtc=DateTimeOffset.UtcNow; plan.Revision++;
        (plan.TitleAr, plan.TitleEn)=Titles(request.Type,request.FromDate,request.ToDate,validation.SubjectAr,validation.SubjectEn,validation.TermAr,validation.TermEn,validation.TermStart);
        db.TeachingPlanTargets.RemoveRange(plan.Targets); db.TeachingPlanItems.RemoveRange(plan.Items); plan.Targets=[]; plan.Items=[];
        AddChildren(plan,request,plan.UpdatedAtUtc); Audit(db,id,"Updated",CurrentUserId(),plan.UpdatedAtUtc);
        await db.SaveChangesAsync(ct); return Ok(Success(new { plan.Id, plan.TitleAr, plan.TitleEn }));
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
        var now=DateTimeOffset.UtcNow; var user=CurrentUserId(); plan.Status=next.Value; plan.UpdatedAtUtc=now;
        if(next==TeachingPlanStatus.Submitted){plan.SubmittedAtUtc=now;plan.SubmittedByUserId=user;}
        if(next==TeachingPlanStatus.Approved){plan.ApprovedAtUtc=now;plan.ApprovedByUserId=user;}
        if(next==TeachingPlanStatus.Published){plan.PublishedAtUtc=now;plan.PublishedByUserId=user;}
        Audit(db,id,next.Value.ToString(),user,now); await db.SaveChangesAsync(ct); return Ok(Success(new { status=next.Value }));
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
        if(r.FromDate>r.ToDate || string.IsNullOrWhiteSpace(r.Details))return (BadRequest(Failure(400,"plans.invalid_period","Plan details and a valid date range are required.")),default,default,"","",null,null,null);
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
            if(r.Type==TeachingPlanType.Monthly)
            {
                var calendarStart=new DateOnly(r.FromDate.Year,r.FromDate.Month,1); var calendarEnd=calendarStart.AddMonths(1).AddDays(-1);
                var expectedStart=calendarStart<term.StartDate?term.StartDate:calendarStart; var expectedEnd=calendarEnd>term.EndDate?term.EndDate:calendarEnd;
                if(r.FromDate!=expectedStart || r.ToDate!=expectedEnd)return (BadRequest(Failure(400,"plans.month_period_invalid","A monthly plan must match one month within the selected term.")),default,default,"","",null,null,null);
            }
            if(r.Type==TeachingPlanType.Weekly)
            {
                var offset=r.FromDate.DayNumber-term.StartDate.DayNumber; var expectedEnd=r.FromDate.AddDays(6)>term.EndDate?term.EndDate:r.FromDate.AddDays(6);
                if(offset<0 || offset%7!=0 || r.ToDate!=expectedEnd)return (BadRequest(Failure(400,"plans.week_period_invalid","A weekly plan must match one week within the selected term.")),default,default,"","",null,null,null);
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
    private static (string,string) Titles(TeachingPlanType type,DateOnly from,DateOnly to,string subjectAr,string subjectEn,string? termAr,string? termEn,DateOnly? termStart)
    {
        var weekNumber=termStart.HasValue?((from.DayNumber-termStart.Value.DayNumber)/7)+1:((from.Day-1)/7)+1;
        var ar=type switch{TeachingPlanType.Yearly=>"الخطة السنوية",TeachingPlanType.Term=>$"خطة {termAr}",TeachingPlanType.Monthly=>$"خطة شهر {ArabicMonths[from.Month-1]} {from.Year}",TeachingPlanType.Weekly=>$"خطة الأسبوع {weekNumber} من شهر {ArabicMonths[from.Month-1]} {from.Year}",TeachingPlanType.Daily=>$"خطة يوم {from.Day} {ArabicMonths[from.Month-1]} {from.Year}",_=>"خطة تدريس"};
        var culture=CultureInfo.GetCultureInfo("en-US"); var en=type switch{TeachingPlanType.Yearly=>"Yearly plan",TeachingPlanType.Term=>$"{termEn} plan",TeachingPlanType.Monthly=>$"{from.ToString("MMMM yyyy",culture)} plan",TeachingPlanType.Weekly=>$"Week {weekNumber} - {from.ToString("MMMM yyyy",culture)} plan",TeachingPlanType.Daily=>$"Daily plan - {from.ToString("d MMMM yyyy",culture)}",_=>"Teaching plan"};
        return ($"{ar} — {subjectAr}",$"{en} — {subjectEn}");
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
    IReadOnlyList<Guid> ClassSectionIds,IReadOnlyList<SaveTeachingPlanItemRequest> Items);
public sealed record SaveTeachingPlanItemRequest(string Title,string? Details,DateOnly? FromDate,DateOnly? ToDate);
public sealed record TeachingPlanClassOption(Guid Id,string NameAr,string NameEn,Guid GradeOfferingId,Guid ProgramAcademicYearId,string AcademicYearNameAr,string AcademicYearNameEn,DateOnly AcademicYearStart,DateOnly AcademicYearEnd,string GradeNameAr,string GradeNameEn,DateOnly SchoolLocalDate,IReadOnlyList<TeachingPlanSubjectOption> Subjects,IReadOnlyList<TeachingPlanTermOption> Terms);
public sealed record TeachingPlanSubjectOption(Guid Id,string NameAr,string NameEn);
public sealed record TeachingPlanTermOption(Guid Id,string NameAr,string NameEn,DateOnly StartDate,DateOnly EndDate);
public sealed record TeachingPlanTargetResponse(Guid ClassSectionId,string NameAr,string NameEn);
public sealed record TeachingPlanSummary(Guid Id,TeachingPlanType Type,TeachingPlanStatus Status,string TitleAr,string TitleEn,DateOnly FromDate,DateOnly ToDate,string SubjectNameAr,string SubjectNameEn,IReadOnlyList<TeachingPlanTargetResponse> Targets,int DocumentCount,int Revision);
public sealed record TeachingPlanItemResponse(Guid Id,int SortOrder,string Title,string? Details,DateOnly? FromDate,DateOnly? ToDate);
public sealed record TeachingPlanDocumentResponse(Guid DocumentId,TeachingPlanDocumentPurpose Purpose,string Title,string FileName,long SizeBytes);
public sealed record TeachingPlanDetails(Guid Id,TeachingPlanType Type,TeachingPlanStatus Status,string TitleAr,string TitleEn,string Details,TeachingPlanSourceAuthority SourceAuthority,string? SourceReference,Guid ProgramAcademicYearId,Guid GradeOfferingId,Guid GradeSubjectOfferingId,Guid? AcademicTermId,Guid? ParentPlanId,DateOnly FromDate,DateOnly ToDate,int Revision,IReadOnlyList<TeachingPlanTargetResponse> Targets,IReadOnlyList<TeachingPlanItemResponse> Items,IReadOnlyList<TeachingPlanDocumentResponse> Documents);
