using System.IdentityModel.Tokens.Jwt;
using System.Text.RegularExpressions;
using Mdaresna.Api.Contracts;
using Mdaresna.Schools.Api.Auth;
using Mdaresna.Schools.Domain.Academics;
using Mdaresna.Schools.Domain.Facilities;
using Mdaresna.Schools.Infrastructure.Identity;
using Mdaresna.Schools.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Mdaresna.Schools.Api.Controllers;

[ApiController, Authorize, Route("api/schools/v1/curriculum")]
public sealed class SchoolCurriculumController(ISchoolDbContextFactory dbFactory) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] bool includeDeleted = false, CancellationToken ct = default)
    {
        if (!Can("view")) return Forbid();
        await using var db = await RequireDb(ct); if (db is null) return Unauthorized();
        var response = new SchoolCurriculumResponse(
            await MapItems(includeDeleted ? db.Subjects.IgnoreQueryFilters() : db.Subjects, ct),
            await MapItems(includeDeleted ? db.CurriculumPlans.IgnoreQueryFilters() : db.CurriculumPlans, ct),
            await MapItems(includeDeleted ? db.CurriculumGradeSubjects.IgnoreQueryFilters() : db.CurriculumGradeSubjects, ct),
            await MapItems(includeDeleted ? db.Books.IgnoreQueryFilters() : db.Books, ct),
            await MapItems(includeDeleted ? db.BookVersions.IgnoreQueryFilters() : db.BookVersions, ct),
            await MapItems(includeDeleted ? db.BookRoles.IgnoreQueryFilters() : db.BookRoles, ct),
            await MapItems(includeDeleted ? db.CurriculumSubjectBooks.IgnoreQueryFilters() : db.CurriculumSubjectBooks, ct),
            await MapItems(includeDeleted ? db.GradeSubjectOfferings.IgnoreQueryFilters() : db.GradeSubjectOfferings, ct));
        return Ok(ApiResponse<SchoolCurriculumResponse>.Success(response, correlationId: HttpContext.TraceIdentifier));
    }

    [HttpGet("plans/{id:guid}/activation-review")]
    public async Task<IActionResult> ReviewPlanActivation(Guid id, CancellationToken ct)
    {
        if (!Can("manage")) return Forbid();
        await using var db = await RequireDb(ct); if (db is null) return Unauthorized();
        if (!await db.CurriculumPlans.AnyAsync(x => x.Id == id, ct)) return Missing();
        var issues = await PlanActivationIssues(db, id, ct);
        return Ok(ApiResponse<CurriculumPlanActivationReviewResponse>.Success(
            new CurriculumPlanActivationReviewResponse(issues), correlationId: HttpContext.TraceIdentifier));
    }

    [HttpPost("{kind}")]
    public async Task<IActionResult> Create(string kind, [FromBody] SaveCurriculumRequest request, CancellationToken ct)
    {
        if (!Can("manage")) return Forbid();
        var normalized = Normalize(kind); if (normalized == Kind.Invalid) return InvalidKind();
        await using var db = await RequireDb(ct); if (db is null) return Unauthorized();
        if (normalized == Kind.Offering && await OfferingParentIssue(db, request, ct) is { } createIssue)
            return BadRequest(Failure(400, createIssue.Code, createIssue.Message));
        var id = Guid.NewGuid(); var now = DateTimeOffset.UtcNow;
        if (!await Add(db, normalized, id, request, now, ct)) return Invalid();
        try { await db.SaveChangesAsync(ct); } catch (DbUpdateException) { return Duplicate(); }
        return StatusCode(201, ApiResponse<object>.Success(new { id }, 201, correlationId: HttpContext.TraceIdentifier));
    }

    [HttpPut("{kind}/{id:guid}")]
    public async Task<IActionResult> Update(string kind, Guid id, [FromBody] SaveCurriculumRequest request, CancellationToken ct)
    {
        if (!Can("manage")) return Forbid();
        var normalized = Normalize(kind); if (normalized == Kind.Invalid) return InvalidKind();
        await using var db = await RequireDb(ct); if (db is null) return Unauthorized();
        var entity = await Find(db, normalized, id, false, ct); if (entity is null) return Missing();
        if (entity is BookRole { IsSystem: true }) return Conflict(Failure(409, "curriculum.system_role", "System book roles cannot be changed."));
        if (!await IsMutable(db, entity, ct)) return Conflict(Failure(409, "curriculum.immutable", "An active or historically used curriculum snapshot cannot be changed."));
        if (normalized == Kind.Offering && await OfferingParentIssue(db, request, ct) is { } updateIssue)
            return BadRequest(Failure(400, updateIssue.Code, updateIssue.Message));
        if (!await Apply(db, entity, request, DateTimeOffset.UtcNow, ct)) return Invalid();
        try { await db.SaveChangesAsync(ct); } catch (DbUpdateException) { return Duplicate(); }
        return Ok(ApiResponse<object?>.Success(null, correlationId: HttpContext.TraceIdentifier));
    }

    [HttpPut("{kind}/{id:guid}/status")]
    public async Task<IActionResult> Status(string kind, Guid id, [FromBody] ChangeCurriculumStatusRequest request, CancellationToken ct)
    {
        if (!Can("manage")) return Forbid();
        var normalized = Normalize(kind); await using var db = await RequireDb(ct); if (db is null) return Unauthorized();
        var entity = await Find(db, normalized, id, false, ct); if (entity is null) return Missing();
        if (entity is BookRole { IsSystem: true }) return Conflict(Failure(409, "curriculum.system_role", "System book roles cannot be changed."));
        if (entity is CurriculumPlan plan)
        {
            if (!Enum.TryParse<CurriculumPlanStatus>(request.Status, true, out var status)) return Invalid();
            if (status == CurriculumPlanStatus.Active)
            {
                var issues = await PlanActivationIssues(db, id, ct);
                if (issues.Count > 0 && request.AllowIncomplete != true)
                    return Conflict(Failure(409, "curriculum.plan_incomplete", "Review the missing curriculum data and explicitly confirm activation."));
            }
            if (plan.Status == CurriculumPlanStatus.Active && status == CurriculumPlanStatus.Draft && await db.ProgramAcademicYears.AnyAsync(x => x.CurriculumPlanId == id, ct))
                return Conflict(Failure(409, "curriculum.plan_assigned", "An assigned curriculum plan cannot return to draft."));
            plan.Status=status; plan.IsActive=status==CurriculumPlanStatus.Active;
        }
        else if (entity is GradeSubjectOffering offering)
        {
            if (!Enum.TryParse<GradeSubjectOfferingStatus>(request.Status, true, out var status)) return Invalid();
            offering.Status=status; offering.IsActive=status==GradeSubjectOfferingStatus.Active;
        }
        else
        {
            if (!await IsMutable(db, entity, ct)) return Conflict(Failure(409, "curriculum.immutable", "An active or historically used curriculum snapshot cannot be changed."));
            SetActive(entity, request.IsActive ?? false);
        }
        SetUpdated(entity, DateTimeOffset.UtcNow); await db.SaveChangesAsync(ct);
        return Ok(ApiResponse<object?>.Success(null, correlationId: HttpContext.TraceIdentifier));
    }

    [HttpPut("program-years/{id:guid}/plan")]
    public async Task<IActionResult> AssignPlan(Guid id, [FromBody] AssignCurriculumPlanRequest request, CancellationToken ct)
    {
        if (!Can("manage")) return Forbid();
        await using var db = await RequireDb(ct); if (db is null) return Unauthorized();
        var year = await db.ProgramAcademicYears.SingleOrDefaultAsync(x => x.Id == id, ct); if (year is null) return Missing();
        if (year.StartDate <= DateOnly.FromDateTime(DateTime.UtcNow) && year.CurriculumPlanId.HasValue && year.CurriculumPlanId != request.CurriculumPlanId)
            return Conflict(Failure(409, "curriculum.year_started", "The assigned curriculum plan cannot be changed after the academic year starts."));
        if (year.StartDate <= DateOnly.FromDateTime(DateTime.UtcNow) && !year.CurriculumPlanId.HasValue && !request.CurriculumPlanId.HasValue)
            return Conflict(Failure(409, "curriculum.year_started", "Select a curriculum plan for the already-started legacy academic year."));
        if (request.CurriculumPlanId.HasValue)
        {
            var valid = await db.CurriculumPlans.AnyAsync(x => x.Id == request.CurriculumPlanId && x.EducationProgramId == year.EducationProgramId && x.Status == CurriculumPlanStatus.Active, ct);
            if (!valid) return Invalid();
        }
        year.CurriculumPlanId=request.CurriculumPlanId; year.UpdatedAtUtc=DateTimeOffset.UtcNow; await db.SaveChangesAsync(ct);
        return Ok(ApiResponse<object?>.Success(null, correlationId: HttpContext.TraceIdentifier));
    }

    [HttpDelete("{kind}/{id:guid}")]
    public async Task<IActionResult> Delete(string kind, Guid id, CancellationToken ct)
    {
        if (!Can("delete")) return Forbid();
        var normalized=Normalize(kind); await using var db=await RequireDb(ct); if(db is null)return Unauthorized();
        var entity=await Find(db,normalized,id,false,ct); if(entity is not ISoftDeletableSchoolEntity soft)return Missing();
        if(entity is BookRole { IsSystem:true }) return Conflict(Failure(409,"curriculum.system_role","System book roles cannot be deleted."));
        if(!await IsMutable(db,entity,ct)) return Conflict(Failure(409,"curriculum.immutable","An active or historically used curriculum snapshot cannot be changed."));
        if(await HasChildren(db,normalized,id,ct))return Conflict(Failure(409,"curriculum.has_children","Delete or move dependent records first."));
        soft.IsDeleted=true; soft.DeletedAtUtc=DateTimeOffset.UtcNow; soft.DeletedByUserId=CurrentUserId(); SetActive(entity,false); SetUpdated(entity,DateTimeOffset.UtcNow); await db.SaveChangesAsync(ct);
        return Ok(ApiResponse<object?>.Success(null,correlationId:HttpContext.TraceIdentifier));
    }

    [HttpPost("{kind}/{id:guid}/restore")]
    public async Task<IActionResult> Restore(string kind,Guid id,CancellationToken ct)
    {
        if(!Can("restore"))return Forbid(); var normalized=Normalize(kind); await using var db=await RequireDb(ct); if(db is null)return Unauthorized();
        var entity=await Find(db,normalized,id,true,ct); if(entity is not ISoftDeletableSchoolEntity soft||!soft.IsDeleted)return Missing();
        if(!await CanRestore(db,entity,ct)) return Conflict(Failure(409,"curriculum.parent_deleted","Restore the active parent records first."));
        soft.IsDeleted=false;soft.DeletedAtUtc=null;soft.DeletedByUserId=null;SetUpdated(entity,DateTimeOffset.UtcNow);
        try{await db.SaveChangesAsync(ct);}catch(DbUpdateException){return Duplicate();}
        return Ok(ApiResponse<object?>.Success(null,correlationId:HttpContext.TraceIdentifier));
    }

    private static async Task<bool> Add(SchoolsDbContext db, Kind kind, Guid id, SaveCurriculumRequest r, DateTimeOffset now, CancellationToken ct)
    {
        var names=Names(r); if (NeedsNames(kind) && names is null) return false;
        switch(kind)
        {
            case Kind.Subject: db.Subjects.Add(new Subject { Id=id, Code=names!.Value.Code, NameAr=names.Value.Ar, NameEn=names.Value.En, CreatedAtUtc=now, UpdatedAtUtc=now }); return true;
            case Kind.Plan:
                if(!r.ParentId.HasValue||!await db.EducationPrograms.AnyAsync(x=>x.Id==r.ParentId&&x.IsActive,ct)||string.IsNullOrWhiteSpace(r.VersionLabel))return false;
                db.CurriculumPlans.Add(new CurriculumPlan{Id=id,EducationProgramId=r.ParentId.Value,Code=names!.Value.Code,NameAr=names.Value.Ar,NameEn=names.Value.En,VersionLabel=r.VersionLabel.Trim(),CreatedAtUtc=now,UpdatedAtUtc=now});return true;
            case Kind.GradeSubject:
                if(!r.ParentId.HasValue||!r.SecondaryParentId.HasValue||!r.ThirdParentId.HasValue||!r.WeeklyPeriods.HasValue||r.WeeklyPeriods is <1 or >100||r.TermNumber is <1 or >10||!await ValidGradeSubjectParents(db,r.ParentId.Value,r.SecondaryParentId.Value,r.ThirdParentId.Value,ct))return false;
                db.CurriculumGradeSubjects.Add(new CurriculumGradeSubject{Id=id,CurriculumPlanId=r.ParentId.Value,GradeLevelId=r.SecondaryParentId.Value,SubjectId=r.ThirdParentId.Value,TermNumber=r.TermNumber??0,WeeklyPeriods=r.WeeklyPeriods.Value,IsRequired=r.IsRequired??true,InstructionLanguage=Trim(r.Language,40),SortOrder=r.SortOrder??0,CreatedAtUtc=now,UpdatedAtUtc=now});return true;
            case Kind.Book: db.Books.Add(new Book{Id=id,Code=names!.Value.Code,NameAr=names.Value.Ar,NameEn=names.Value.En,Publisher=Trim(r.Publisher,200),CreatedAtUtc=now,UpdatedAtUtc=now});return true;
            case Kind.BookVersion:
                if(!r.ParentId.HasValue||!await db.Books.AnyAsync(x=>x.Id==r.ParentId&&x.IsActive,ct)||!ValidBookVersion(r))return false;
                db.BookVersions.Add(new BookVersion{Id=id,BookId=r.ParentId.Value,EditionCode=r.EditionCode!.Trim().ToUpperInvariant(),VersionLabel=r.VersionLabel!.Trim(),PublicationYear=r.PublicationYear!.Value,Language=r.Language!.Trim(),Isbn=Trim(r.Isbn,32),CreatedAtUtc=now,UpdatedAtUtc=now});return true;
            case Kind.BookRole: db.BookRoles.Add(new BookRole{Id=id,Code=names!.Value.Code,NameAr=names.Value.Ar,NameEn=names.Value.En,CreatedAtUtc=now,UpdatedAtUtc=now});return true;
            case Kind.SubjectBook:
                if(!r.ParentId.HasValue||!r.SecondaryParentId.HasValue||!r.ThirdParentId.HasValue||!await ValidSubjectBookParents(db,r.ParentId.Value,r.SecondaryParentId.Value,r.ThirdParentId.Value,ct))return false;
                db.CurriculumSubjectBooks.Add(new CurriculumSubjectBook{Id=id,CurriculumGradeSubjectId=r.ParentId.Value,BookVersionId=r.SecondaryParentId.Value,BookRoleId=r.ThirdParentId.Value,SortOrder=r.SortOrder??0,CreatedAtUtc=now,UpdatedAtUtc=now});return true;
            case Kind.Offering:
                if(!r.ParentId.HasValue||!r.SecondaryParentId.HasValue||!await ValidOfferingParents(db,r.ParentId.Value,r.SecondaryParentId.Value,ct))return false;
                db.GradeSubjectOfferings.Add(new GradeSubjectOffering{Id=id,GradeOfferingId=r.ParentId.Value,CurriculumGradeSubjectId=r.SecondaryParentId.Value,CreatedAtUtc=now,UpdatedAtUtc=now});return true;
            default:return false;
        }
    }

    private static async Task<bool> Apply(SchoolsDbContext db,object entity,SaveCurriculumRequest r,DateTimeOffset now,CancellationToken ct)
    {
        var names=Names(r);
        switch(entity)
        {
            case Subject x when names.HasValue:x.Code=names.Value.Code;x.NameAr=names.Value.Ar;x.NameEn=names.Value.En;break;
            case CurriculumPlan x when names.HasValue&&x.Status==CurriculumPlanStatus.Draft&&r.ParentId.HasValue&&await db.EducationPrograms.AnyAsync(p=>p.Id==r.ParentId&&p.IsActive,ct)&&!string.IsNullOrWhiteSpace(r.VersionLabel):x.EducationProgramId=r.ParentId.Value;x.Code=names.Value.Code;x.NameAr=names.Value.Ar;x.NameEn=names.Value.En;x.VersionLabel=r.VersionLabel.Trim();break;
            case CurriculumGradeSubject x when r.ParentId.HasValue&&r.SecondaryParentId.HasValue&&r.ThirdParentId.HasValue&&r.WeeklyPeriods.HasValue&&r.WeeklyPeriods is >0 and <=100&&await ValidGradeSubjectParents(db,r.ParentId.Value,r.SecondaryParentId.Value,r.ThirdParentId.Value,ct):x.CurriculumPlanId=r.ParentId.Value;x.GradeLevelId=r.SecondaryParentId.Value;x.SubjectId=r.ThirdParentId.Value;x.TermNumber=r.TermNumber??0;x.WeeklyPeriods=r.WeeklyPeriods.Value;x.IsRequired=r.IsRequired??true;x.InstructionLanguage=Trim(r.Language,40);x.SortOrder=r.SortOrder??0;break;
            case Book x when names.HasValue:x.Code=names.Value.Code;x.NameAr=names.Value.Ar;x.NameEn=names.Value.En;x.Publisher=Trim(r.Publisher,200);break;
            case BookVersion x when r.ParentId.HasValue&&ValidBookVersion(r):x.BookId=r.ParentId.Value;x.EditionCode=r.EditionCode!.Trim().ToUpperInvariant();x.VersionLabel=r.VersionLabel!.Trim();x.PublicationYear=r.PublicationYear!.Value;x.Language=r.Language!.Trim();x.Isbn=Trim(r.Isbn,32);break;
            case BookRole x when names.HasValue:x.Code=names.Value.Code;x.NameAr=names.Value.Ar;x.NameEn=names.Value.En;break;
            case CurriculumSubjectBook x when r.ParentId.HasValue&&r.SecondaryParentId.HasValue&&r.ThirdParentId.HasValue&&await ValidSubjectBookParents(db,r.ParentId.Value,r.SecondaryParentId.Value,r.ThirdParentId.Value,ct):x.CurriculumGradeSubjectId=r.ParentId.Value;x.BookVersionId=r.SecondaryParentId.Value;x.BookRoleId=r.ThirdParentId.Value;x.SortOrder=r.SortOrder??0;break;
            case GradeSubjectOffering x when r.ParentId.HasValue&&r.SecondaryParentId.HasValue&&await ValidOfferingParents(db,r.ParentId.Value,r.SecondaryParentId.Value,ct):x.GradeOfferingId=r.ParentId.Value;x.CurriculumGradeSubjectId=r.SecondaryParentId.Value;break;
            default:return false;
        }
        SetUpdated(entity,now);return true;
    }

    private static async Task<bool> ValidGradeSubjectParents(SchoolsDbContext db,Guid planId,Guid gradeId,Guid subjectId,CancellationToken ct)
    { var plan=await db.CurriculumPlans.SingleOrDefaultAsync(x=>x.Id==planId&&x.Status==CurriculumPlanStatus.Draft,ct); if(plan is null)return false; var grade=await db.GradeLevels.Include(x=>x.EducationStage).SingleOrDefaultAsync(x=>x.Id==gradeId&&x.IsActive,ct); return grade?.EducationStage.EducationProgramId==plan.EducationProgramId&&await db.Subjects.AnyAsync(x=>x.Id==subjectId&&x.IsActive,ct); }
    private static async Task<bool> ValidSubjectBookParents(SchoolsDbContext db,Guid gradeSubjectId,Guid versionId,Guid roleId,CancellationToken ct)
    { var item=await db.CurriculumGradeSubjects.Include(x=>x.CurriculumPlan).SingleOrDefaultAsync(x=>x.Id==gradeSubjectId,ct); return item?.CurriculumPlan.Status==CurriculumPlanStatus.Draft&&await db.BookVersions.AnyAsync(x=>x.Id==versionId&&x.IsActive,ct)&&await db.BookRoles.AnyAsync(x=>x.Id==roleId&&x.IsActive,ct); }
    private static async Task<bool> ValidOfferingParents(SchoolsDbContext db,Guid offeringId,Guid gradeSubjectId,CancellationToken ct)
    { return await OfferingParentIssue(db,new SaveCurriculumRequest(ParentId:offeringId,SecondaryParentId:gradeSubjectId),ct) is null; }
    private static async Task<(string Code,string Message)?> OfferingParentIssue(SchoolsDbContext db,SaveCurriculumRequest request,CancellationToken ct)
    {
        if(!request.ParentId.HasValue||!request.SecondaryParentId.HasValue)return("curriculum.offering_selection_required","Select a grade offering and a grade subject.");
        var offering=await db.GradeOfferings.Include(x=>x.ProgramAcademicYear).SingleOrDefaultAsync(x=>x.Id==request.ParentId,ct);
        if(offering is null)return("curriculum.offering_grade_unavailable","The selected grade offering is unavailable.");
        var item=await db.CurriculumGradeSubjects.Include(x=>x.CurriculumPlan).SingleOrDefaultAsync(x=>x.Id==request.SecondaryParentId&&x.IsActive,ct);
        if(item is null)return("curriculum.offering_subject_inactive","The selected grade subject is unavailable or inactive.");
        if(offering.GradeLevelId!=item.GradeLevelId)return("curriculum.offering_grade_mismatch","The grade offering and curriculum subject belong to different grades.");
        if(item.CurriculumPlan.Status!=CurriculumPlanStatus.Active)return("curriculum.offering_plan_inactive","Activate the curriculum plan before creating subject offerings.");
        if(!offering.ProgramAcademicYear.CurriculumPlanId.HasValue)return("curriculum.offering_plan_not_assigned","Assign the active curriculum plan to the academic year first.");
        if(offering.ProgramAcademicYear.CurriculumPlanId!=item.CurriculumPlanId)return("curriculum.offering_plan_mismatch","The academic year uses a different curriculum plan.");
        return null;
    }
    private static bool ValidBookVersion(SaveCurriculumRequest r)=>!string.IsNullOrWhiteSpace(r.EditionCode)&&r.EditionCode.Length<=50&&!string.IsNullOrWhiteSpace(r.VersionLabel)&&r.VersionLabel.Length<=100&&r.PublicationYear is >=1900 and <=2200&&!string.IsNullOrWhiteSpace(r.Language)&&r.Language.Length<=40;
    private static async Task<IReadOnlyList<CurriculumPlanActivationIssueResponse>> PlanActivationIssues(SchoolsDbContext db,Guid planId,CancellationToken ct)
    {
        var subjects=await db.CurriculumGradeSubjects.AsNoTracking().Where(x=>x.CurriculumPlanId==planId)
            .Select(x=>new {x.IsActive,ActiveBooks=x.Books.Count(b=>b.IsActive&&b.BookVersion.IsActive&&b.BookVersion.Book.IsActive),PrimaryBooks=x.Books.Count(b=>b.IsActive&&b.BookVersion.IsActive&&b.BookVersion.Book.IsActive&&b.BookRole.IsActive&&b.BookRole.Code=="PRIMARY")})
            .ToListAsync(ct);
        var issues=new List<CurriculumPlanActivationIssueResponse>();
        var active=subjects.Where(x=>x.IsActive).ToArray();
        if(active.Length==0)issues.Add(new("no_active_grade_subjects",0));
        var inactiveCount=subjects.Count(x=>!x.IsActive);if(inactiveCount>0)issues.Add(new("inactive_grade_subjects",inactiveCount));
        var withoutBooks=active.Count(x=>x.ActiveBooks==0);if(withoutBooks>0)issues.Add(new("grade_subjects_without_books",withoutBooks));
        var withoutPrimary=active.Count(x=>x.ActiveBooks>0&&x.PrimaryBooks==0);if(withoutPrimary>0)issues.Add(new("grade_subjects_without_primary_book",withoutPrimary));
        return issues;
    }
    private static (string Code,string Ar,string En)? Names(SaveCurriculumRequest r){var c=r.Code?.Trim().ToUpperInvariant()??"";var a=r.NameAr?.Trim()??"";var e=r.NameEn?.Trim()??"";return Regex.IsMatch(c,"^[A-Z0-9][A-Z0-9._-]{0,49}$")&&a.Length is >0 and <=150&&e.Length is >0 and <=150?(c,a,e):null;}
    private static bool NeedsNames(Kind k)=>k is Kind.Subject or Kind.Plan or Kind.Book or Kind.BookRole;
    private static string? Trim(string? v,int max)=>string.IsNullOrWhiteSpace(v)?null:v.Trim()[..Math.Min(v.Trim().Length,max)];

    private static async Task<bool> IsMutable(SchoolsDbContext db,object e,CancellationToken ct)=>e switch
    { CurriculumPlan p=>p.Status==CurriculumPlanStatus.Draft,CurriculumGradeSubject x=>await db.CurriculumPlans.AnyAsync(p=>p.Id==x.CurriculumPlanId&&p.Status==CurriculumPlanStatus.Draft,ct),CurriculumSubjectBook x=>await db.CurriculumGradeSubjects.AnyAsync(s=>s.Id==x.CurriculumGradeSubjectId&&s.CurriculumPlan.Status==CurriculumPlanStatus.Draft,ct),BookVersion x=>!await db.CurriculumSubjectBooks.AnyAsync(b=>b.BookVersionId==x.Id&&b.CurriculumGradeSubject.CurriculumPlan.Status!=CurriculumPlanStatus.Draft,ct),_=>true};
    private static async Task<bool> CanRestore(SchoolsDbContext db,object e,CancellationToken ct)=>e switch
    { CurriculumPlan x=>await db.EducationPrograms.IgnoreQueryFilters().AnyAsync(p=>p.Id==x.EducationProgramId&&!p.IsDeleted,ct),CurriculumGradeSubject x=>await db.CurriculumPlans.IgnoreQueryFilters().AnyAsync(p=>p.Id==x.CurriculumPlanId&&!p.IsDeleted,ct)&&await db.GradeLevels.IgnoreQueryFilters().AnyAsync(g=>g.Id==x.GradeLevelId&&!g.IsDeleted,ct)&&await db.Subjects.IgnoreQueryFilters().AnyAsync(s=>s.Id==x.SubjectId&&!s.IsDeleted,ct),BookVersion x=>await db.Books.IgnoreQueryFilters().AnyAsync(b=>b.Id==x.BookId&&!b.IsDeleted,ct),CurriculumSubjectBook x=>await db.CurriculumGradeSubjects.IgnoreQueryFilters().AnyAsync(s=>s.Id==x.CurriculumGradeSubjectId&&!s.IsDeleted,ct)&&await db.BookVersions.IgnoreQueryFilters().AnyAsync(b=>b.Id==x.BookVersionId&&!b.IsDeleted,ct)&&await db.BookRoles.IgnoreQueryFilters().AnyAsync(r=>r.Id==x.BookRoleId&&!r.IsDeleted,ct),GradeSubjectOffering x=>await db.GradeOfferings.IgnoreQueryFilters().AnyAsync(o=>o.Id==x.GradeOfferingId&&!o.IsDeleted,ct)&&await db.CurriculumGradeSubjects.IgnoreQueryFilters().AnyAsync(s=>s.Id==x.CurriculumGradeSubjectId&&!s.IsDeleted,ct),_=>true};
    private static async Task<bool> HasChildren(SchoolsDbContext db,Kind kind,Guid id,CancellationToken ct)=>kind switch
    { Kind.Subject=>await db.CurriculumGradeSubjects.AnyAsync(x=>x.SubjectId==id,ct),Kind.Plan=>await db.CurriculumGradeSubjects.AnyAsync(x=>x.CurriculumPlanId==id,ct)||await db.ProgramAcademicYears.AnyAsync(x=>x.CurriculumPlanId==id,ct),Kind.GradeSubject=>await db.CurriculumSubjectBooks.AnyAsync(x=>x.CurriculumGradeSubjectId==id,ct)||await db.GradeSubjectOfferings.AnyAsync(x=>x.CurriculumGradeSubjectId==id,ct),Kind.Book=>await db.BookVersions.AnyAsync(x=>x.BookId==id,ct),Kind.BookVersion=>await db.CurriculumSubjectBooks.AnyAsync(x=>x.BookVersionId==id,ct),Kind.BookRole=>await db.CurriculumSubjectBooks.AnyAsync(x=>x.BookRoleId==id,ct),_=>false};
    private static async Task<object?> Find(SchoolsDbContext db,Kind k,Guid id,bool deleted,CancellationToken ct)=>k switch
    { Kind.Subject=>await(deleted?db.Subjects.IgnoreQueryFilters():db.Subjects).SingleOrDefaultAsync(x=>x.Id==id,ct),Kind.Plan=>await(deleted?db.CurriculumPlans.IgnoreQueryFilters():db.CurriculumPlans).SingleOrDefaultAsync(x=>x.Id==id,ct),Kind.GradeSubject=>await(deleted?db.CurriculumGradeSubjects.IgnoreQueryFilters():db.CurriculumGradeSubjects).SingleOrDefaultAsync(x=>x.Id==id,ct),Kind.Book=>await(deleted?db.Books.IgnoreQueryFilters():db.Books).SingleOrDefaultAsync(x=>x.Id==id,ct),Kind.BookVersion=>await(deleted?db.BookVersions.IgnoreQueryFilters():db.BookVersions).SingleOrDefaultAsync(x=>x.Id==id,ct),Kind.BookRole=>await(deleted?db.BookRoles.IgnoreQueryFilters():db.BookRoles).SingleOrDefaultAsync(x=>x.Id==id,ct),Kind.SubjectBook=>await(deleted?db.CurriculumSubjectBooks.IgnoreQueryFilters():db.CurriculumSubjectBooks).SingleOrDefaultAsync(x=>x.Id==id,ct),Kind.Offering=>await(deleted?db.GradeSubjectOfferings.IgnoreQueryFilters():db.GradeSubjectOfferings).SingleOrDefaultAsync(x=>x.Id==id,ct),_=>null};
    private static Kind Normalize(string v)=>v.Trim().ToLowerInvariant() switch{"subjects"=>Kind.Subject,"plans"=>Kind.Plan,"grade-subjects"=>Kind.GradeSubject,"books"=>Kind.Book,"book-versions"=>Kind.BookVersion,"book-roles"=>Kind.BookRole,"subject-books"=>Kind.SubjectBook,"offerings"=>Kind.Offering,_=>Kind.Invalid};
    private static async Task<IReadOnlyList<CurriculumItemResponse>> MapItems<TEntity>(IQueryable<TEntity> q,CancellationToken ct) where TEntity:class=>(await q.AsNoTracking().ToListAsync(ct)).Select(Map).ToArray();
    private static CurriculumItemResponse Map(object e)=>e switch
    { Subject x=>Item(x.Id,x.Code,x.NameAr,x.NameEn,x.IsActive,x.IsDeleted,x.DeletedAtUtc),CurriculumPlan x=>Item(x.Id,x.Code,x.NameAr,x.NameEn,x.IsActive,x.IsDeleted,x.DeletedAtUtc,x.EducationProgramId,versionLabel:x.VersionLabel,status:x.Status.ToString()),CurriculumGradeSubject x=>Item(x.Id,"","","",x.IsActive,x.IsDeleted,x.DeletedAtUtc,x.CurriculumPlanId,x.GradeLevelId,x.SubjectId,termNumber:x.TermNumber,weeklyPeriods:x.WeeklyPeriods,isRequired:x.IsRequired,language:x.InstructionLanguage,sortOrder:x.SortOrder),Book x=>Item(x.Id,x.Code,x.NameAr,x.NameEn,x.IsActive,x.IsDeleted,x.DeletedAtUtc,publisher:x.Publisher),BookVersion x=>Item(x.Id,x.EditionCode,x.VersionLabel,x.VersionLabel,x.IsActive,x.IsDeleted,x.DeletedAtUtc,x.BookId,versionLabel:x.VersionLabel,editionCode:x.EditionCode,publicationYear:x.PublicationYear,language:x.Language,isbn:x.Isbn),BookRole x=>Item(x.Id,x.Code,x.NameAr,x.NameEn,x.IsActive,x.IsDeleted,x.DeletedAtUtc,isSystem:x.IsSystem),CurriculumSubjectBook x=>Item(x.Id,"","","",x.IsActive,x.IsDeleted,x.DeletedAtUtc,x.CurriculumGradeSubjectId,x.BookVersionId,x.BookRoleId,sortOrder:x.SortOrder),GradeSubjectOffering x=>Item(x.Id,"","","",x.IsActive,x.IsDeleted,x.DeletedAtUtc,x.GradeOfferingId,x.CurriculumGradeSubjectId,status:x.Status.ToString()),_=>throw new ArgumentOutOfRangeException()};
    private static CurriculumItemResponse Item(Guid id,string code,string ar,string en,bool active,bool deleted,DateTimeOffset? deletedAt,Guid? parent=null,Guid? second=null,Guid? third=null,string? versionLabel=null,string? status=null,int? termNumber=null,int? weeklyPeriods=null,bool? isRequired=null,string? language=null,int? sortOrder=null,string? publisher=null,string? editionCode=null,int? publicationYear=null,string? isbn=null,bool? isSystem=null)=>new(id,parent,second,third,code,ar,en,active,deleted,deletedAt,versionLabel,status,termNumber,weeklyPeriods,isRequired,language,sortOrder,publisher,editionCode,publicationYear,isbn,isSystem);
    private static void SetActive(object e,bool v){switch(e){case Subject x:x.IsActive=v;break;case CurriculumPlan x:x.IsActive=v;break;case CurriculumGradeSubject x:x.IsActive=v;break;case Book x:x.IsActive=v;break;case BookVersion x:x.IsActive=v;break;case BookRole x:x.IsActive=v;break;case CurriculumSubjectBook x:x.IsActive=v;break;case GradeSubjectOffering x:x.IsActive=v;break;}}
    private static void SetUpdated(object e,DateTimeOffset v){switch(e){case Subject x:x.UpdatedAtUtc=v;break;case CurriculumPlan x:x.UpdatedAtUtc=v;break;case CurriculumGradeSubject x:x.UpdatedAtUtc=v;break;case Book x:x.UpdatedAtUtc=v;break;case BookVersion x:x.UpdatedAtUtc=v;break;case BookRole x:x.UpdatedAtUtc=v;break;case CurriculumSubjectBook x:x.UpdatedAtUtc=v;break;case GradeSubjectOffering x:x.UpdatedAtUtc=v;break;}}
    private bool Can(string action)=>User.HasClaim(SchoolClaimTypes.Permission,$"school.academics.{action}");
    private Task<SchoolsDbContext?> RequireDb(CancellationToken ct)=>dbFactory.CreateAsync(User.FindFirst(SchoolClaimTypes.SchoolCode)?.Value??"",ct);
    private Guid? CurrentUserId()=>Guid.TryParse(User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value,out var id)?id:null;
    private IActionResult Invalid()=>BadRequest(Failure(400,"curriculum.invalid","Enter valid curriculum data and active parents."));
    private IActionResult InvalidKind()=>BadRequest(Failure(400,"curriculum.kind_invalid","Curriculum item kind is invalid."));
    private IActionResult Duplicate()=>Conflict(Failure(409,"curriculum.duplicate","The code or relationship is already in use."));
    private IActionResult Missing()=>NotFound(Failure(404,"curriculum.not_found","The curriculum item was not found."));
    private ApiResponse<object?> Failure(int status,string code,string message)=>ApiResponse<object?>.Failure(status,code,message,correlationId:HttpContext.TraceIdentifier);
    private enum Kind { Invalid,Subject,Plan,GradeSubject,Book,BookVersion,BookRole,SubjectBook,Offering }
}

public sealed record SaveCurriculumRequest(string? Code=null,string? NameAr=null,string? NameEn=null,Guid? ParentId=null,Guid? SecondaryParentId=null,Guid? ThirdParentId=null,string? VersionLabel=null,int? TermNumber=null,int? WeeklyPeriods=null,bool? IsRequired=null,string? Language=null,int? SortOrder=null,string? Publisher=null,string? EditionCode=null,int? PublicationYear=null,string? Isbn=null);
public sealed record ChangeCurriculumStatusRequest(bool? IsActive=null,string? Status=null,bool? AllowIncomplete=null);
public sealed record AssignCurriculumPlanRequest(Guid? CurriculumPlanId);
public sealed record CurriculumPlanActivationIssueResponse(string Code,int AffectedCount);
public sealed record CurriculumPlanActivationReviewResponse(IReadOnlyList<CurriculumPlanActivationIssueResponse> Issues);
public sealed record CurriculumItemResponse(Guid Id,Guid? ParentId,Guid? SecondaryParentId,Guid? ThirdParentId,string Code,string NameAr,string NameEn,bool IsActive,bool IsDeleted,DateTimeOffset? DeletedAtUtc,string? VersionLabel,string? Status,int? TermNumber,int? WeeklyPeriods,bool? IsRequired,string? Language,int? SortOrder,string? Publisher,string? EditionCode,int? PublicationYear,string? Isbn,bool? IsSystem);
public sealed record SchoolCurriculumResponse(IReadOnlyList<CurriculumItemResponse> Subjects,IReadOnlyList<CurriculumItemResponse> Plans,IReadOnlyList<CurriculumItemResponse> GradeSubjects,IReadOnlyList<CurriculumItemResponse> Books,IReadOnlyList<CurriculumItemResponse> BookVersions,IReadOnlyList<CurriculumItemResponse> BookRoles,IReadOnlyList<CurriculumItemResponse> SubjectBooks,IReadOnlyList<CurriculumItemResponse> Offerings);
