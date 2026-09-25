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

[ApiController, Authorize, Route("api/schools/v1/class-structure")]
public sealed class SchoolClassStructureController(ISchoolDbContextFactory dbFactory) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] bool includeDeleted = false, CancellationToken ct = default)
    {
        if (!Can("view")) return Forbid();
        await using var db = await RequireDb(ct); if (db is null) return Unauthorized();
        var result = new SchoolClassStructureResponse(
            await Items(includeDeleted ? db.EducationStages.IgnoreQueryFilters() : db.EducationStages, ct),
            await Items(includeDeleted ? db.EducationTracks.IgnoreQueryFilters() : db.EducationTracks, ct),
            await Items(includeDeleted ? db.GradeLevels.IgnoreQueryFilters() : db.GradeLevels, ct),
            await Items(includeDeleted ? db.GradeOfferings.IgnoreQueryFilters() : db.GradeOfferings, ct),
            await Items(includeDeleted ? db.ClassSections.IgnoreQueryFilters() : db.ClassSections, ct),
            await Items(includeDeleted ? db.ClassRoomAssignments.IgnoreQueryFilters() : db.ClassRoomAssignments, ct));
        return Ok(ApiResponse<SchoolClassStructureResponse>.Success(result, correlationId: HttpContext.TraceIdentifier));
    }

    [HttpPost("{kind}")]
    public async Task<IActionResult> Create(string kind, [FromBody] SaveClassStructureRequest request, CancellationToken ct)
    {
        if (!Can("manage")) return Forbid();
        var normalized = Normalize(kind); if (normalized == Kind.Invalid) return InvalidKind();
        await using var db = await RequireDb(ct); if (db is null) return Unauthorized();
        if (!TryNames(request, normalized, out var code, out var ar, out var en)) return Invalid();
        var id = Guid.NewGuid(); var now = DateTimeOffset.UtcNow;
        switch (normalized)
        {
            case Kind.Stage:
                if (!request.ParentId.HasValue || !ValidStageSchedule(request) || !await db.EducationPrograms.AnyAsync(x => x.Id == request.ParentId && x.IsActive, ct)) return InvalidParent();
                db.EducationStages.Add(new EducationStage { Id=id, EducationProgramId=request.ParentId.Value, Code=code, NameAr=ar, NameEn=en, SortOrder=request.SortOrder??0, DailyLessonCount=request.DailyLessonCount!.Value, DailyBreakCount=request.DailyBreakCount!.Value, CreatedAtUtc=now, UpdatedAtUtc=now }); break;
            case Kind.Track:
                if (!request.ParentId.HasValue || !await db.EducationStages.AnyAsync(x => x.Id == request.ParentId && x.IsActive, ct)) return InvalidParent();
                db.EducationTracks.Add(new EducationTrack { Id=id, EducationStageId=request.ParentId.Value, Code=code, NameAr=ar, NameEn=en, SortOrder=request.SortOrder??0, CreatedAtUtc=now, UpdatedAtUtc=now }); break;
            case Kind.Grade:
                if (!request.ParentId.HasValue || !await ValidGradeParents(db, request.ParentId.Value, request.SecondaryParentId, ct)) return InvalidParent();
                db.GradeLevels.Add(new GradeLevel { Id=id, EducationStageId=request.ParentId.Value, EducationTrackId=request.SecondaryParentId, Code=code, NameAr=ar, NameEn=en, SortOrder=request.SortOrder??0, CreatedAtUtc=now, UpdatedAtUtc=now }); break;
            case Kind.Offering:
                if (!request.ParentId.HasValue || !request.SecondaryParentId.HasValue) return InvalidParent();
                if (request.Capacity is < 1 or > 100000) return Invalid();
                var generated = await BuildOfferingIdentity(db, request.ParentId.Value, request.SecondaryParentId.Value, ct); if (generated is null) return InvalidParent();
                (code, ar, en) = generated.Value;
                db.GradeOfferings.Add(new GradeOffering { Id=id, ProgramAcademicYearId=request.ParentId.Value, GradeLevelId=request.SecondaryParentId.Value, Code=code, NameAr=ar, NameEn=en, Capacity=request.Capacity, Status=GradeOfferingStatus.Draft, IsActive=false, CreatedAtUtc=now, UpdatedAtUtc=now }); break;
            case Kind.Section:
                if (!request.ParentId.HasValue || !request.Capacity.HasValue || request.Capacity is < 1 or > 10000 || !Enum.TryParse<SchoolShift>(request.Shift,true,out var shift) || !await db.GradeOfferings.AnyAsync(x => x.Id==request.ParentId&&x.Status!=GradeOfferingStatus.Closed,ct)) return InvalidParent();
                var sectionNumber = await db.ClassSections.IgnoreQueryFilters().CountAsync(x => x.GradeOfferingId == request.ParentId, ct) + 1;
                code = $"CS-{id:N}";
                ar = $"فصل {sectionNumber}";
                en = $"Class {sectionNumber}";
                db.ClassSections.Add(new ClassSection { Id=id, GradeOfferingId=request.ParentId.Value, Code=code, NameAr=ar, NameEn=en, Capacity=request.Capacity.Value, Shift=shift, CreatedAtUtc=now, UpdatedAtUtc=now }); break;
            case Kind.Assignment:
                var assignmentError = await ValidateAssignment(db, request, null, ct); if (assignmentError is not null) return assignmentError;
                db.ClassRoomAssignments.Add(new ClassRoomAssignment { Id=id, ClassSectionId=request.ParentId!.Value, RoomId=request.SecondaryParentId!.Value, EffectiveFrom=request.StartDate!.Value, EffectiveTo=request.EndDate!.Value, StartsAt=request.StartsAt, EndsAt=request.EndsAt, IsPrimary=request.IsPrimary??true, CreatedAtUtc=now, UpdatedAtUtc=now }); break;
        }
        try { await db.SaveChangesAsync(ct); } catch (DbUpdateException) { return Duplicate(); }
        return StatusCode(201, ApiResponse<object>.Success(new { id }, 201, correlationId: HttpContext.TraceIdentifier));
    }

    [HttpPut("{kind}/{id:guid}")]
    public async Task<IActionResult> Update(string kind, Guid id, [FromBody] SaveClassStructureRequest request, CancellationToken ct)
    {
        if (!Can("manage")) return Forbid();
        var normalized=Normalize(kind); await using var db=await RequireDb(ct); if(db is null)return Unauthorized();
        var entity=await Find(db,normalized,id,false,ct); if(entity is null)return Missing();
        if(!TryNames(request,normalized,out var code,out var ar,out var en))return Invalid(); var now=DateTimeOffset.UtcNow;
        if(normalized==Kind.Offering)
        {
            if(!request.ParentId.HasValue||!request.SecondaryParentId.HasValue)return InvalidParent();
            var generated=await BuildOfferingIdentity(db,request.ParentId.Value,request.SecondaryParentId.Value,ct);if(generated is null)return InvalidParent();
            (code,ar,en)=generated.Value;
        }
        if(entity is ClassSection movingSection&&request.ParentId!=movingSection.GradeOfferingId&&await WouldRemoveLastActiveSection(db,movingSection,ct))return Conflict(Failure(409,"class_structure.last_section","An active grade must keep at least one active class section."));
        if(entity is GradeOffering movingOffering&&(request.ParentId!=movingOffering.ProgramAcademicYearId||request.SecondaryParentId!=movingOffering.GradeLevelId)&&(movingOffering.IsActive||await db.ClassSections.AnyAsync(x=>x.GradeOfferingId==id,ct)||await db.GradeSubjectOfferings.AnyAsync(x=>x.GradeOfferingId==id,ct)))return Conflict(Failure(409,"class_structure.offering_in_use","The year or grade cannot be changed after the offering is activated or used."));
        if(entity is EducationStage&&ValidStageSchedule(request))
        {
            var total=request.DailyLessonCount!.Value+request.DailyBreakCount!.Value;
            if(await db.WeeklyTimetableSlots.AnyAsync(x=>x.IsActive&&x.ClassSection.GradeOffering.GradeLevel.EducationStageId==id&&x.SlotNumber>total,ct)||
               await db.WeeklyTimetableSlots.Where(x=>x.IsActive&&x.IsBreak&&x.ClassSection.GradeOffering.GradeLevel.EducationStageId==id)
                   .GroupBy(x=>new{x.ClassSectionId,x.DayOfWeek}).AnyAsync(x=>x.Count()>request.DailyBreakCount.Value,ct))
                return Conflict(Failure(409,"class_structure.stage_schedule_in_use","Remove timetable cells outside the new limits before reducing them."));
        }
        switch(entity)
        {
            case EducationStage x when request.ParentId.HasValue && ValidStageSchedule(request) && await db.EducationPrograms.AnyAsync(p=>p.Id==request.ParentId&&p.IsActive,ct): x.EducationProgramId=request.ParentId.Value;x.Code=code;x.NameAr=ar;x.NameEn=en;x.SortOrder=request.SortOrder??0;x.DailyLessonCount=request.DailyLessonCount!.Value;x.DailyBreakCount=request.DailyBreakCount!.Value;x.UpdatedAtUtc=now;break;
            case EducationTrack x when request.ParentId.HasValue && await db.EducationStages.AnyAsync(p=>p.Id==request.ParentId&&p.IsActive,ct): x.EducationStageId=request.ParentId.Value;x.Code=code;x.NameAr=ar;x.NameEn=en;x.SortOrder=request.SortOrder??0;x.UpdatedAtUtc=now;break;
            case GradeLevel x when request.ParentId.HasValue && await ValidGradeParents(db,request.ParentId.Value,request.SecondaryParentId,ct): x.EducationStageId=request.ParentId.Value;x.EducationTrackId=request.SecondaryParentId;x.Code=code;x.NameAr=ar;x.NameEn=en;x.SortOrder=request.SortOrder??0;x.UpdatedAtUtc=now;break;
            case GradeOffering x when request.ParentId.HasValue&&request.SecondaryParentId.HasValue&&request.Capacity is not < 1 and not > 100000: x.ProgramAcademicYearId=request.ParentId.Value;x.GradeLevelId=request.SecondaryParentId.Value;x.Code=code;x.NameAr=ar;x.NameEn=en;x.Capacity=request.Capacity;x.UpdatedAtUtc=now;break;
            case ClassSection x when request.ParentId.HasValue&&request.Capacity is >=1 and <=10000&&Enum.TryParse<SchoolShift>(request.Shift,true,out var shift)&&await db.GradeOfferings.AnyAsync(p=>p.Id==request.ParentId&&p.Status!=GradeOfferingStatus.Closed,ct)&&!await db.ClassRoomAssignments.AnyAsync(a=>a.ClassSectionId==id&&a.IsActive&&a.Room.Capacity<request.Capacity.Value,ct): x.GradeOfferingId=request.ParentId.Value;x.Capacity=request.Capacity.Value;x.Shift=shift;x.UpdatedAtUtc=now;break;
            case ClassRoomAssignment x:
                var error=await ValidateAssignment(db,request,id,ct);if(error is not null)return error;x.ClassSectionId=request.ParentId!.Value;x.RoomId=request.SecondaryParentId!.Value;x.EffectiveFrom=request.StartDate!.Value;x.EffectiveTo=request.EndDate!.Value;x.StartsAt=request.StartsAt;x.EndsAt=request.EndsAt;x.IsPrimary=request.IsPrimary??true;x.UpdatedAtUtc=now;break;
            default:return InvalidParent();
        }
        try{await db.SaveChangesAsync(ct);}catch(DbUpdateException){return Duplicate();}
        return Ok(ApiResponse<object?>.Success(null,correlationId:HttpContext.TraceIdentifier));
    }

    [HttpPut("{kind}/{id:guid}/status")]
    public async Task<IActionResult> Status(string kind,Guid id,[FromBody] ChangeAcademicStatusRequest request,CancellationToken ct)
    {
        if(!Can("manage"))return Forbid();var normalized=Normalize(kind);await using var db=await RequireDb(ct);if(db is null)return Unauthorized();var entity=await Find(db,normalized,id,false,ct);if(entity is null)return Missing();
        if(entity is GradeOffering offering&&request.IsActive&&!await db.ClassSections.AnyAsync(x=>x.GradeOfferingId==id&&x.IsActive,ct))return Conflict(Failure(409,"class_structure.section_required","Create at least one active class section before activating the grade."));
        if(entity is ClassSection activatingSection&&request.IsActive&&!await db.GradeOfferings.AnyAsync(x=>x.Id==activatingSection.GradeOfferingId&&x.Status!=GradeOfferingStatus.Closed,ct))return Conflict(Failure(409,"class_structure.parent_unavailable","The grade offering is closed."));
        if(entity is ClassSection section&&!request.IsActive&&await WouldRemoveLastActiveSection(db,section,ct))return Conflict(Failure(409,"class_structure.last_section","An active grade must keep at least one active class section."));
        if(entity is ClassRoomAssignment assignment&&request.IsActive){var validation=await ValidateAssignment(db,new SaveClassStructureRequest(null,null,null,assignment.ClassSectionId,assignment.RoomId,StartDate:assignment.EffectiveFrom,EndDate:assignment.EffectiveTo,StartsAt:assignment.StartsAt,EndsAt:assignment.EndsAt,IsPrimary:assignment.IsPrimary),id,ct);if(validation is not null)return validation;}
        SetActive(entity,request.IsActive);SetUpdated(entity,DateTimeOffset.UtcNow);await db.SaveChangesAsync(ct);return Ok(ApiResponse<object?>.Success(null,correlationId:HttpContext.TraceIdentifier));
    }

    [HttpDelete("{kind}/{id:guid}")]
    public async Task<IActionResult> Delete(string kind,Guid id,CancellationToken ct)
    {
        if(!Can("delete"))return Forbid();var normalized=Normalize(kind);await using var db=await RequireDb(ct);if(db is null)return Unauthorized();var entity=await Find(db,normalized,id,false,ct);if(entity is not ISoftDeletableSchoolEntity soft)return Missing();if(entity is ClassSection section&&await WouldRemoveLastActiveSection(db,section,ct))return Conflict(Failure(409,"class_structure.last_section","An active grade must keep at least one active class section."));if(await HasChildren(db,normalized,id,ct))return Conflict(Failure(409,"class_structure.has_children","Delete or move child records first."));
        soft.IsDeleted=true;soft.DeletedAtUtc=DateTimeOffset.UtcNow;soft.DeletedByUserId=CurrentUserId();SetActive(entity,false);SetUpdated(entity,DateTimeOffset.UtcNow);await db.SaveChangesAsync(ct);return Ok(ApiResponse<object?>.Success(null,correlationId:HttpContext.TraceIdentifier));
    }

    [HttpPost("{kind}/{id:guid}/restore")]
    public async Task<IActionResult> Restore(string kind,Guid id,CancellationToken ct)
    {
        if(!Can("restore"))return Forbid();var normalized=Normalize(kind);await using var db=await RequireDb(ct);if(db is null)return Unauthorized();var entity=await Find(db,normalized,id,true,ct);if(entity is not ISoftDeletableSchoolEntity soft||!soft.IsDeleted)return Missing();if(!await ParentAvailable(db,entity,ct))return Conflict(Failure(409,"class_structure.parent_unavailable","Restore the parent record first."));soft.IsDeleted=false;soft.DeletedAtUtc=null;soft.DeletedByUserId=null;SetUpdated(entity,DateTimeOffset.UtcNow);try{await db.SaveChangesAsync(ct);}catch(DbUpdateException){return Duplicate();}return Ok(ApiResponse<object?>.Success(null,correlationId:HttpContext.TraceIdentifier));
    }

    private async Task<IActionResult?> ValidateAssignment(SchoolsDbContext db,SaveClassStructureRequest r,Guid? currentId,CancellationToken ct)
    {
        if(!r.ParentId.HasValue||!r.SecondaryParentId.HasValue||!r.StartDate.HasValue||!r.EndDate.HasValue||r.StartDate>r.EndDate||(r.StartsAt.HasValue!=r.EndsAt.HasValue)||(r.StartsAt.HasValue&&r.StartsAt>=r.EndsAt))return Invalid();
        var section=await db.ClassSections.SingleOrDefaultAsync(x=>x.Id==r.ParentId&&x.IsActive,ct);var room=await db.SchoolRooms.Include(x=>x.RoomType).SingleOrDefaultAsync(x=>x.Id==r.SecondaryParentId&&x.IsActive,ct);
        if(section is null||room is null||!room.IsSchedulable||!room.RoomType.IsClassroom||room.Capacity<section.Capacity)return BadRequest(Failure(400,"class_structure.room_invalid","Select an active classroom with enough capacity."));
        var candidates=await db.ClassRoomAssignments.AsNoTracking().Where(x=>x.RoomId==room.Id&&x.IsActive&&x.Id!=currentId&&x.EffectiveFrom<=r.EndDate&&x.EffectiveTo>=r.StartDate).ToListAsync(ct);
        var overlap=candidates.Any(x=>!r.StartsAt.HasValue||!x.StartsAt.HasValue||r.StartsAt<x.EndsAt&&r.EndsAt>x.StartsAt);if(overlap)return Conflict(Failure(409,"class_structure.room_conflict","The room is already assigned during the selected period."));return null;
    }
    private static Task<bool> ValidGradeParents(SchoolsDbContext db,Guid stageId,Guid? trackId,CancellationToken ct)=>trackId.HasValue?db.EducationTracks.AnyAsync(x=>x.Id==trackId&&x.EducationStageId==stageId&&x.IsActive,ct):db.EducationStages.AnyAsync(x=>x.Id==stageId&&x.IsActive,ct);
    private static async Task<(string Code,string NameAr,string NameEn)?> BuildOfferingIdentity(SchoolsDbContext db,Guid yearId,Guid gradeId,CancellationToken ct)
    {
        var year=await db.ProgramAcademicYears.AsNoTracking().SingleOrDefaultAsync(x=>x.Id==yearId&&x.IsActive,ct);
        var grade=await db.GradeLevels.AsNoTracking().Include(x=>x.EducationStage).SingleOrDefaultAsync(x=>x.Id==gradeId&&x.IsActive,ct);
        if(year is null||grade is null||grade.EducationStage.EducationProgramId!=year.EducationProgramId)return null;
        var code=$"GO-{year.Id:N}"[..11]+$"-{grade.Id:N}"[..9];
        return(code,Limit($"{grade.NameAr} - {year.NameAr}",150),Limit($"{grade.NameEn} - {year.NameEn}",150));
    }
    private static string Limit(string value,int max)=>value.Length<=max?value:value[..max];
    private static async Task<bool> WouldRemoveLastActiveSection(SchoolsDbContext db,ClassSection section,CancellationToken ct)=>await db.GradeOfferings.AnyAsync(x=>x.Id==section.GradeOfferingId&&x.IsActive,ct)&&!await db.ClassSections.AnyAsync(x=>x.GradeOfferingId==section.GradeOfferingId&&x.Id!=section.Id&&x.IsActive,ct);
    private static async Task<bool> HasChildren(SchoolsDbContext db,Kind kind,Guid id,CancellationToken ct)=>kind switch{Kind.Stage=>await db.EducationTracks.AnyAsync(x=>x.EducationStageId==id,ct)||await db.GradeLevels.AnyAsync(x=>x.EducationStageId==id,ct),Kind.Track=>await db.GradeLevels.AnyAsync(x=>x.EducationTrackId==id,ct),Kind.Grade=>await db.GradeOfferings.AnyAsync(x=>x.GradeLevelId==id,ct),Kind.Offering=>await db.ClassSections.AnyAsync(x=>x.GradeOfferingId==id,ct),Kind.Section=>await db.ClassRoomAssignments.AnyAsync(x=>x.ClassSectionId==id,ct)||await db.ClassSectionTeacherScopes.AnyAsync(x=>x.ClassSectionId==id,ct)||await db.ClassSectionSubjects.AnyAsync(x=>x.ClassSectionId==id,ct)||await db.WeeklyTimetableSlots.AnyAsync(x=>x.ClassSectionId==id,ct),_=>false};
    private static async Task<bool> ParentAvailable(SchoolsDbContext db,object entity,CancellationToken ct)=>entity switch{EducationStage x=>await db.EducationPrograms.AnyAsync(p=>p.Id==x.EducationProgramId,ct),EducationTrack x=>await db.EducationStages.AnyAsync(p=>p.Id==x.EducationStageId,ct),GradeLevel x=>await db.EducationStages.AnyAsync(p=>p.Id==x.EducationStageId,ct)&&(!x.EducationTrackId.HasValue||await db.EducationTracks.AnyAsync(p=>p.Id==x.EducationTrackId,ct)),GradeOffering x=>await db.ProgramAcademicYears.AnyAsync(p=>p.Id==x.ProgramAcademicYearId,ct)&&await db.GradeLevels.AnyAsync(p=>p.Id==x.GradeLevelId,ct),ClassSection x=>await db.GradeOfferings.AnyAsync(p=>p.Id==x.GradeOfferingId&&p.Status!=GradeOfferingStatus.Closed,ct),ClassRoomAssignment x=>await db.ClassSections.AnyAsync(p=>p.Id==x.ClassSectionId,ct)&&await db.SchoolRooms.AnyAsync(p=>p.Id==x.RoomId,ct),_=>false};
    private static async Task<object?> Find(SchoolsDbContext db,Kind kind,Guid id,bool deleted,CancellationToken ct)=>kind switch{Kind.Stage=>await(deleted?db.EducationStages.IgnoreQueryFilters():db.EducationStages).SingleOrDefaultAsync(x=>x.Id==id,ct),Kind.Track=>await(deleted?db.EducationTracks.IgnoreQueryFilters():db.EducationTracks).SingleOrDefaultAsync(x=>x.Id==id,ct),Kind.Grade=>await(deleted?db.GradeLevels.IgnoreQueryFilters():db.GradeLevels).SingleOrDefaultAsync(x=>x.Id==id,ct),Kind.Offering=>await(deleted?db.GradeOfferings.IgnoreQueryFilters():db.GradeOfferings).SingleOrDefaultAsync(x=>x.Id==id,ct),Kind.Section=>await(deleted?db.ClassSections.IgnoreQueryFilters():db.ClassSections).SingleOrDefaultAsync(x=>x.Id==id,ct),Kind.Assignment=>await(deleted?db.ClassRoomAssignments.IgnoreQueryFilters():db.ClassRoomAssignments).SingleOrDefaultAsync(x=>x.Id==id,ct),_=>null};
    private static async Task<IReadOnlyList<ClassStructureItemResponse>> Items<TEntity>(IQueryable<TEntity> query,CancellationToken ct)where TEntity:class=>(await query.AsNoTracking().ToListAsync(ct)).Select(Map).ToArray();
    private static ClassStructureItemResponse Map(object value)=>value switch{EducationStage x=>Item(x.Id,x.EducationProgramId,null,null,x.Code,x.NameAr,x.NameEn,x.SortOrder,null,null,null,null,null,null,null,x.IsActive,x.IsDeleted,x.DeletedAtUtc,dailyLessons:x.DailyLessonCount,dailyBreaks:x.DailyBreakCount),EducationTrack x=>Item(x.Id,x.EducationStageId,null,null,x.Code,x.NameAr,x.NameEn,x.SortOrder,null,null,null,null,null,null,null,x.IsActive,x.IsDeleted,x.DeletedAtUtc),GradeLevel x=>Item(x.Id,x.EducationStageId,x.EducationTrackId,null,x.Code,x.NameAr,x.NameEn,x.SortOrder,null,null,null,null,null,null,null,x.IsActive,x.IsDeleted,x.DeletedAtUtc),GradeOffering x=>Item(x.Id,x.ProgramAcademicYearId,x.GradeLevelId,null,x.Code,x.NameAr,x.NameEn,null,x.Capacity,x.Status.ToString(),null,null,null,null,null,x.IsActive,x.IsDeleted,x.DeletedAtUtc),ClassSection x=>Item(x.Id,x.GradeOfferingId,null,null,x.Code,x.NameAr,x.NameEn,null,x.Capacity,null,x.Shift.ToString(),null,null,null,null,x.IsActive,x.IsDeleted,x.DeletedAtUtc),ClassRoomAssignment x=>Item(x.Id,x.ClassSectionId,x.RoomId,x.RoomId,"","","",null,null,null,null,x.EffectiveFrom,x.EffectiveTo,x.StartsAt,x.EndsAt,x.IsActive,x.IsDeleted,x.DeletedAtUtc,x.IsPrimary),_=>throw new ArgumentOutOfRangeException()};
    private static ClassStructureItemResponse Item(Guid id,Guid? parent,Guid? second,Guid? room,string code,string ar,string en,int? sort,int? capacity,string? status,string? shift,DateOnly? start,DateOnly? end,TimeOnly? starts,TimeOnly? ends,bool active,bool deleted,DateTimeOffset? deletedAt,bool? primary=null,int? dailyLessons=null,int? dailyBreaks=null)=>new(id,parent,second,room,code,ar,en,sort,capacity,status,shift,start,end,starts,ends,primary,active,deleted,deletedAt,dailyLessons,dailyBreaks);
    private static bool ValidStageSchedule(SaveClassStructureRequest request)=>request.DailyLessonCount is >=1 and <=20&&request.DailyBreakCount is >=0 and <=10&&request.DailyLessonCount+request.DailyBreakCount<=24;
    private static bool TryNames(SaveClassStructureRequest r,Kind kind,out string code,out string ar,out string en){code=r.Code?.Trim().ToUpperInvariant()??"";ar=r.NameAr?.Trim()??"";en=r.NameEn?.Trim()??"";if(kind is Kind.Assignment or Kind.Offering or Kind.Section)return true;return Regex.IsMatch(code,"^[A-Z0-9][A-Z0-9._-]{0,49}$")&&ar.Length is >0 and <=150&&en.Length is >0 and <=150;}
    private static Kind Normalize(string value)=>value.Trim().ToLowerInvariant()switch{"stages"=>Kind.Stage,"tracks"=>Kind.Track,"grades"=>Kind.Grade,"offerings"=>Kind.Offering,"sections"=>Kind.Section,"room-assignments"=>Kind.Assignment,_=>Kind.Invalid};
    private static void SetActive(object e,bool value){switch(e){case EducationStage x:x.IsActive=value;break;case EducationTrack x:x.IsActive=value;break;case GradeLevel x:x.IsActive=value;break;case GradeOffering x:x.IsActive=value;x.Status=value?GradeOfferingStatus.Active:GradeOfferingStatus.Draft;break;case ClassSection x:x.IsActive=value;break;case ClassRoomAssignment x:x.IsActive=value;break;}}
    private static void SetUpdated(object e,DateTimeOffset now){switch(e){case EducationStage x:x.UpdatedAtUtc=now;break;case EducationTrack x:x.UpdatedAtUtc=now;break;case GradeLevel x:x.UpdatedAtUtc=now;break;case GradeOffering x:x.UpdatedAtUtc=now;break;case ClassSection x:x.UpdatedAtUtc=now;break;case ClassRoomAssignment x:x.UpdatedAtUtc=now;break;}}
    private bool Can(string action)=>User.HasClaim(SchoolClaimTypes.Permission,$"school.academics.{action}");
    private Task<SchoolsDbContext?> RequireDb(CancellationToken ct)=>dbFactory.CreateAsync(User.FindFirst(SchoolClaimTypes.SchoolCode)?.Value??"",ct);
    private Guid? CurrentUserId()=>Guid.TryParse(User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value,out var id)?id:null;
    private IActionResult Invalid()=>BadRequest(Failure(400,"class_structure.invalid","Enter valid data."));private IActionResult InvalidKind()=>BadRequest(Failure(400,"class_structure.kind_invalid","Setting kind is invalid."));private IActionResult InvalidParent()=>BadRequest(Failure(400,"class_structure.parent_invalid","The selected parent is invalid."));private IActionResult Duplicate()=>Conflict(Failure(409,"class_structure.duplicate","The code or combination is already in use."));private IActionResult Missing()=>NotFound(Failure(404,"class_structure.not_found","The record was not found."));private ApiResponse<object?> Failure(int status,string code,string message)=>ApiResponse<object?>.Failure(status,code,message,correlationId:HttpContext.TraceIdentifier);
    private enum Kind{Invalid,Stage,Track,Grade,Offering,Section,Assignment}
}

public sealed record SaveClassStructureRequest(string? Code,string? NameAr,string? NameEn,Guid? ParentId=null,Guid? SecondaryParentId=null,int? SortOrder=null,int? Capacity=null,string? Shift=null,DateOnly? StartDate=null,DateOnly? EndDate=null,[property:System.Text.Json.Serialization.JsonConverter(typeof(FlexibleNullableTimeOnlyJsonConverter))]TimeOnly? StartsAt=null,[property:System.Text.Json.Serialization.JsonConverter(typeof(FlexibleNullableTimeOnlyJsonConverter))]TimeOnly? EndsAt=null,bool? IsPrimary=null,int? DailyLessonCount=null,int? DailyBreakCount=null);
public sealed record ClassStructureItemResponse(Guid Id,Guid? ParentId,Guid? SecondaryParentId,Guid? RoomId,string Code,string NameAr,string NameEn,int? SortOrder,int? Capacity,string? Status,string? Shift,DateOnly? StartDate,DateOnly? EndDate,TimeOnly? StartsAt,TimeOnly? EndsAt,bool? IsPrimary,bool IsActive,bool IsDeleted,DateTimeOffset? DeletedAtUtc,int? DailyLessonCount,int? DailyBreakCount);
public sealed record SchoolClassStructureResponse(IReadOnlyList<ClassStructureItemResponse> Stages,IReadOnlyList<ClassStructureItemResponse> Tracks,IReadOnlyList<ClassStructureItemResponse> Grades,IReadOnlyList<ClassStructureItemResponse> Offerings,IReadOnlyList<ClassStructureItemResponse> Sections,IReadOnlyList<ClassStructureItemResponse> RoomAssignments);
