using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Text.Json;
using System.Text.Json.Serialization;
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

[ApiController, Authorize, Route("api/schools/v1/academic-settings")]
public sealed class SchoolAcademicSettingsController(ISchoolDbContextFactory dbFactory) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] bool includeDeleted = false, CancellationToken cancellationToken = default)
    {
        if (!HasAny("school.academics.view", "school.operations.view", "school.calendar.view")) return Forbid();
        await using var db = await RequireDb(cancellationToken); if (db is null) return Unauthorized();
        var canViewAcademics = HasAny("school.academics.view");
        var canViewOperations = HasAny("school.operations.view");
        var canViewCalendar = HasAny("school.calendar.view");
        var result = new SchoolAcademicSettingsResponse(
            canViewAcademics ? await Items(includeDeleted ? db.EducationPrograms.IgnoreQueryFilters() : db.EducationPrograms, Kind.Program, cancellationToken) : [],
            canViewAcademics ? await Items(includeDeleted ? db.AcademicYearDefinitions.IgnoreQueryFilters() : db.AcademicYearDefinitions, Kind.YearDefinition, cancellationToken) : [],
            canViewAcademics ? await Items(includeDeleted ? db.ProgramAcademicYears.IgnoreQueryFilters() : db.ProgramAcademicYears, Kind.ProgramYear, cancellationToken) : [],
            canViewAcademics ? await Items(includeDeleted ? db.AcademicTerms.IgnoreQueryFilters() : db.AcademicTerms, Kind.Term, cancellationToken) : [],
            canViewAcademics ? await Items(includeDeleted ? db.AcademicPeriods.IgnoreQueryFilters() : db.AcademicPeriods, Kind.Period, cancellationToken) : [],
            canViewOperations ? await Items(includeDeleted ? db.SchoolDaySchedules.IgnoreQueryFilters() : db.SchoolDaySchedules, Kind.Schedule, cancellationToken) : [],
            canViewCalendar ? await Items(includeDeleted ? db.SchoolCalendarEvents.IgnoreQueryFilters() : db.SchoolCalendarEvents, Kind.CalendarEvent, cancellationToken) : []);
        return Ok(ApiResponse<SchoolAcademicSettingsResponse>.Success(result, correlationId: HttpContext.TraceIdentifier));
    }

    [HttpPost("{kind}")]
    public async Task<IActionResult> Create(string kind, [FromBody] SaveAcademicSettingRequest request, CancellationToken cancellationToken)
    {
        var normalized = Normalize(kind); if (!Can(normalized, "manage")) return Forbid();
        await using var db = await RequireDb(cancellationToken); if (db is null) return Unauthorized();
        var now = DateTimeOffset.UtcNow; var id = Guid.NewGuid();
        if (!TryNames(request, normalized, out var code, out var ar, out var en)) return Invalid();
        switch (normalized)
        {
            case Kind.Program:
                if (!Enum.TryParse<EducationProgramType>(request.ProgramType, true, out var programType)) return Invalid();
                db.EducationPrograms.Add(new EducationProgram { Id=id, Code=code, NameAr=ar, NameEn=en, ProgramType=programType, CreatedAtUtc=now, UpdatedAtUtc=now }); break;
            case Kind.YearDefinition:
                db.AcademicYearDefinitions.Add(new AcademicYearDefinition { Id=id, Code=code, NameAr=ar, NameEn=en, CreatedAtUtc=now, UpdatedAtUtc=now }); break;
            case Kind.ProgramYear:
                if (!request.ParentId.HasValue || !request.SecondaryParentId.HasValue || !ValidDates(request) ||
                    !await db.EducationPrograms.AnyAsync(x => x.Id == request.ParentId && x.IsActive, cancellationToken) ||
                    !await db.AcademicYearDefinitions.AnyAsync(x => x.Id == request.SecondaryParentId && x.IsActive, cancellationToken)) return InvalidParent();
                db.ProgramAcademicYears.Add(new ProgramAcademicYear { Id=id, EducationProgramId=request.ParentId.Value, AcademicYearDefinitionId=request.SecondaryParentId.Value, Code=code, NameAr=ar, NameEn=en, StartDate=request.StartDate!.Value, EndDate=request.EndDate!.Value, CreatedAtUtc=now, UpdatedAtUtc=now }); break;
            case Kind.Term:
                if (!request.ParentId.HasValue || !ValidDates(request) || !await db.ProgramAcademicYears.AnyAsync(x => x.Id == request.ParentId && x.IsActive, cancellationToken)) return InvalidParent();
                db.AcademicTerms.Add(new AcademicTerm { Id=id, ProgramAcademicYearId=request.ParentId.Value, Code=code, NameAr=ar, NameEn=en, StartDate=request.StartDate!.Value, EndDate=request.EndDate!.Value, SortOrder=request.SortOrder ?? 0, CreatedAtUtc=now, UpdatedAtUtc=now }); break;
            case Kind.Period:
                if (!request.ParentId.HasValue || !ValidDates(request) || !await db.AcademicTerms.AnyAsync(x => x.Id == request.ParentId && x.IsActive, cancellationToken)) return InvalidParent();
                db.AcademicPeriods.Add(new AcademicPeriod { Id=id, AcademicTermId=request.ParentId.Value, Code=code, NameAr=ar, NameEn=en, StartDate=request.StartDate!.Value, EndDate=request.EndDate!.Value, SortOrder=request.SortOrder ?? 0, CreatedAtUtc=now, UpdatedAtUtc=now }); break;
            case Kind.Schedule:
                if (!request.ParentId.HasValue || !Enum.TryParse<DayOfWeek>(request.DayOfWeek, true, out var day) || !request.StartsAt.HasValue || !request.EndsAt.HasValue || request.StartsAt >= request.EndsAt ||
                    !await db.EducationPrograms.AnyAsync(x => x.Id == request.ParentId && x.IsActive, cancellationToken) || request.BranchId.HasValue && !await db.SchoolBranches.AnyAsync(x => x.Id == request.BranchId && x.IsActive, cancellationToken)) return InvalidParent();
                db.SchoolDaySchedules.Add(new SchoolDaySchedule { Id=id, EducationProgramId=request.ParentId.Value, BranchId=request.BranchId, DayOfWeek=day, StartsAt=request.StartsAt.Value, EndsAt=request.EndsAt.Value, CreatedAtUtc=now, UpdatedAtUtc=now }); break;
            case Kind.CalendarEvent:
                if (!ValidDates(request) || !Enum.TryParse<SchoolCalendarEventType>(request.EventType, true, out var eventType) ||
                    request.ParentId.HasValue && !await db.EducationPrograms.AnyAsync(x => x.Id == request.ParentId, cancellationToken) ||
                    request.SecondaryParentId.HasValue && !await db.ProgramAcademicYears.AnyAsync(x => x.Id == request.SecondaryParentId, cancellationToken) ||
                    request.BranchId.HasValue && !await db.SchoolBranches.AnyAsync(x => x.Id == request.BranchId, cancellationToken)) return InvalidParent();
                db.SchoolCalendarEvents.Add(new SchoolCalendarEvent { Id=id, EducationProgramId=request.ParentId, ProgramAcademicYearId=request.SecondaryParentId, BranchId=request.BranchId, Code=code, NameAr=ar, NameEn=en, EventType=eventType, StartDate=request.StartDate!.Value, EndDate=request.EndDate!.Value, IsSchoolClosed=request.IsSchoolClosed ?? false, CreatedAtUtc=now, UpdatedAtUtc=now }); break;
            default: return InvalidKind();
        }
        try { await db.SaveChangesAsync(cancellationToken); } catch (DbUpdateException) { return Duplicate(); }
        return StatusCode(201, ApiResponse<object>.Success(new { id }, 201, correlationId: HttpContext.TraceIdentifier));
    }

    [HttpPut("{kind}/{id:guid}")]
    public async Task<IActionResult> Update(string kind, Guid id, [FromBody] SaveAcademicSettingRequest request, CancellationToken cancellationToken)
    {
        var normalized = Normalize(kind); if (!Can(normalized, "manage")) return Forbid();
        await using var db = await RequireDb(cancellationToken); if (db is null) return Unauthorized();
        var entity = await Find(db, normalized, id, false, cancellationToken); if (entity is null) return Missing();
        if (!TryNames(request, normalized, out var code, out var ar, out var en)) return Invalid();
        var now = DateTimeOffset.UtcNow;
        switch (entity)
        {
            case EducationProgram x when Enum.TryParse<EducationProgramType>(request.ProgramType, true, out var type): x.Code=code; x.NameAr=ar; x.NameEn=en; x.ProgramType=type; x.UpdatedAtUtc=now; break;
            case AcademicYearDefinition x: x.Code=code; x.NameAr=ar; x.NameEn=en; x.UpdatedAtUtc=now; break;
            case ProgramAcademicYear x when request.ParentId.HasValue && request.SecondaryParentId.HasValue && ValidDates(request): x.EducationProgramId=request.ParentId.Value; x.AcademicYearDefinitionId=request.SecondaryParentId.Value; x.Code=code; x.NameAr=ar; x.NameEn=en; x.StartDate=request.StartDate!.Value; x.EndDate=request.EndDate!.Value; x.UpdatedAtUtc=now; break;
            case AcademicTerm x when request.ParentId.HasValue && ValidDates(request): x.ProgramAcademicYearId=request.ParentId.Value; x.Code=code; x.NameAr=ar; x.NameEn=en; x.StartDate=request.StartDate!.Value; x.EndDate=request.EndDate!.Value; x.SortOrder=request.SortOrder ?? 0; x.UpdatedAtUtc=now; break;
            case AcademicPeriod x when request.ParentId.HasValue && ValidDates(request): x.AcademicTermId=request.ParentId.Value; x.Code=code; x.NameAr=ar; x.NameEn=en; x.StartDate=request.StartDate!.Value; x.EndDate=request.EndDate!.Value; x.SortOrder=request.SortOrder ?? 0; x.UpdatedAtUtc=now; break;
            case SchoolDaySchedule x when request.ParentId.HasValue && Enum.TryParse<DayOfWeek>(request.DayOfWeek, true, out var day) && request.StartsAt < request.EndsAt: x.EducationProgramId=request.ParentId.Value; x.BranchId=request.BranchId; x.DayOfWeek=day; x.StartsAt=request.StartsAt!.Value; x.EndsAt=request.EndsAt!.Value; x.UpdatedAtUtc=now; break;
            case SchoolCalendarEvent x when ValidDates(request) && Enum.TryParse<SchoolCalendarEventType>(request.EventType, true, out var eventType): x.EducationProgramId=request.ParentId; x.ProgramAcademicYearId=request.SecondaryParentId; x.BranchId=request.BranchId; x.Code=code; x.NameAr=ar; x.NameEn=en; x.EventType=eventType; x.StartDate=request.StartDate!.Value; x.EndDate=request.EndDate!.Value; x.IsSchoolClosed=request.IsSchoolClosed ?? false; x.UpdatedAtUtc=now; break;
            default: return Invalid();
        }
        try { await db.SaveChangesAsync(cancellationToken); } catch (DbUpdateException) { return Duplicate(); }
        return Ok(ApiResponse<object?>.Success(null, correlationId: HttpContext.TraceIdentifier));
    }

    [HttpPut("{kind}/{id:guid}/status")]
    public async Task<IActionResult> Status(string kind, Guid id, [FromBody] ChangeAcademicStatusRequest request, CancellationToken cancellationToken) => await Mutate(kind, id, "manage", false, cancellationToken, entity => SetActive(entity, request.IsActive));

    [HttpDelete("{kind}/{id:guid}")]
    public async Task<IActionResult> Delete(string kind, Guid id, CancellationToken cancellationToken)
    {
        var normalized = Normalize(kind); if (!Can(normalized, "delete")) return Forbid();
        await using var db = await RequireDb(cancellationToken); if (db is null) return Unauthorized();
        var entity = await Find(db, normalized, id, false, cancellationToken); if (entity is not ISoftDeletableSchoolEntity soft) return Missing();
        if (await HasChildren(db, normalized, id, cancellationToken)) return Conflict(Failure(409, "academic_settings.has_children", "Delete or move child records first."));
        var now=DateTimeOffset.UtcNow; soft.IsDeleted=true; soft.DeletedAtUtc=now; soft.DeletedByUserId=CurrentUserId(); SetActive(entity,false); SetUpdated(entity,now); await db.SaveChangesAsync(cancellationToken);
        return Ok(ApiResponse<object?>.Success(null, correlationId: HttpContext.TraceIdentifier));
    }

    [HttpPost("{kind}/{id:guid}/restore")]
    public async Task<IActionResult> Restore(string kind, Guid id, CancellationToken cancellationToken)
    {
        var normalized=Normalize(kind); if (!Can(normalized,"restore")) return Forbid();
        await using var db=await RequireDb(cancellationToken); if(db is null) return Unauthorized();
        var entity=await Find(db,normalized,id,true,cancellationToken); if(entity is not ISoftDeletableSchoolEntity soft || !soft.IsDeleted) return Missing();
        soft.IsDeleted=false; soft.DeletedAtUtc=null; soft.DeletedByUserId=null; SetUpdated(entity,DateTimeOffset.UtcNow);
        try { await db.SaveChangesAsync(cancellationToken); } catch(DbUpdateException) { return Duplicate(); }
        return Ok(ApiResponse<object?>.Success(null, correlationId: HttpContext.TraceIdentifier));
    }

    private async Task<IActionResult> Mutate(string kind, Guid id, string action, bool deleted, CancellationToken ct, Action<object> mutate)
    { var normalized=Normalize(kind); if(!Can(normalized,action)) return Forbid(); await using var db=await RequireDb(ct); if(db is null)return Unauthorized(); var entity=await Find(db,normalized,id,deleted,ct); if(entity is null)return Missing(); mutate(entity); SetUpdated(entity,DateTimeOffset.UtcNow); await db.SaveChangesAsync(ct); return Ok(ApiResponse<object?>.Success(null,correlationId:HttpContext.TraceIdentifier)); }

    private bool Can(Kind kind,string action) { var module=kind switch { Kind.Schedule=>"operations", Kind.CalendarEvent=>"calendar", Kind.Invalid=>"invalid", _=>"academics" }; return User.HasClaim(SchoolClaimTypes.Permission,$"school.{module}.{action}"); }
    private bool HasAny(params string[] permissions)=>permissions.Any(p=>User.HasClaim(SchoolClaimTypes.Permission,p));
    private static bool ValidDates(SaveAcademicSettingRequest r)=>r.StartDate.HasValue&&r.EndDate.HasValue&&r.StartDate<=r.EndDate;
    private static bool TryNames(SaveAcademicSettingRequest r,Kind kind,out string code,out string ar,out string en) { code=r.Code?.Trim().ToUpperInvariant()??string.Empty; ar=r.NameAr?.Trim()??string.Empty; en=r.NameEn?.Trim()??string.Empty; if(kind==Kind.Schedule){code=ar=en="";return true;} return Regex.IsMatch(code,"^[A-Z0-9][A-Z0-9._-]{0,49}$")&&ar.Length is >0 and <=150&&en.Length is >0 and <=150; }
    private static Kind Normalize(string value)=>value.Trim().ToLowerInvariant() switch { "programs"=>Kind.Program,"year-definitions"=>Kind.YearDefinition,"program-years"=>Kind.ProgramYear,"terms"=>Kind.Term,"periods"=>Kind.Period,"schedules"=>Kind.Schedule,"calendar-events"=>Kind.CalendarEvent,_=>Kind.Invalid };
    private static async Task<object?> Find(SchoolsDbContext db,Kind kind,Guid id,bool deleted,CancellationToken ct)=>kind switch { Kind.Program=>await(deleted?db.EducationPrograms.IgnoreQueryFilters():db.EducationPrograms).SingleOrDefaultAsync(x=>x.Id==id,ct),Kind.YearDefinition=>await(deleted?db.AcademicYearDefinitions.IgnoreQueryFilters():db.AcademicYearDefinitions).SingleOrDefaultAsync(x=>x.Id==id,ct),Kind.ProgramYear=>await(deleted?db.ProgramAcademicYears.IgnoreQueryFilters():db.ProgramAcademicYears).SingleOrDefaultAsync(x=>x.Id==id,ct),Kind.Term=>await(deleted?db.AcademicTerms.IgnoreQueryFilters():db.AcademicTerms).SingleOrDefaultAsync(x=>x.Id==id,ct),Kind.Period=>await(deleted?db.AcademicPeriods.IgnoreQueryFilters():db.AcademicPeriods).SingleOrDefaultAsync(x=>x.Id==id,ct),Kind.Schedule=>await(deleted?db.SchoolDaySchedules.IgnoreQueryFilters():db.SchoolDaySchedules).SingleOrDefaultAsync(x=>x.Id==id,ct),Kind.CalendarEvent=>await(deleted?db.SchoolCalendarEvents.IgnoreQueryFilters():db.SchoolCalendarEvents).SingleOrDefaultAsync(x=>x.Id==id,ct),_=>null};
    private static Task<bool> HasChildren(SchoolsDbContext db,Kind kind,Guid id,CancellationToken ct)=>kind switch { Kind.Program=>db.ProgramAcademicYears.AnyAsync(x=>x.EducationProgramId==id,ct),Kind.YearDefinition=>db.ProgramAcademicYears.AnyAsync(x=>x.AcademicYearDefinitionId==id,ct),Kind.ProgramYear=>db.AcademicTerms.AnyAsync(x=>x.ProgramAcademicYearId==id,ct),Kind.Term=>db.AcademicPeriods.AnyAsync(x=>x.AcademicTermId==id,ct),_=>Task.FromResult(false)};
    private static void SetActive(object entity,bool value){switch(entity){case EducationProgram x:x.IsActive=value;break;case AcademicYearDefinition x:x.IsActive=value;break;case ProgramAcademicYear x:x.IsActive=value;break;case AcademicTerm x:x.IsActive=value;break;case AcademicPeriod x:x.IsActive=value;break;case SchoolDaySchedule x:x.IsActive=value;break;case SchoolCalendarEvent x:x.IsActive=value;break;}}
    private static void SetUpdated(object entity,DateTimeOffset value){switch(entity){case EducationProgram x:x.UpdatedAtUtc=value;break;case AcademicYearDefinition x:x.UpdatedAtUtc=value;break;case ProgramAcademicYear x:x.UpdatedAtUtc=value;break;case AcademicTerm x:x.UpdatedAtUtc=value;break;case AcademicPeriod x:x.UpdatedAtUtc=value;break;case SchoolDaySchedule x:x.UpdatedAtUtc=value;break;case SchoolCalendarEvent x:x.UpdatedAtUtc=value;break;}}
    private static async Task<IReadOnlyList<AcademicSettingItemResponse>> Items<TEntity>(IQueryable<TEntity> query,Kind kind,CancellationToken ct) where TEntity:class => (await query.AsNoTracking().ToListAsync(ct)).Select(x=>Map(x!,kind)).ToArray();
    private static AcademicSettingItemResponse Map(object value,Kind kind)=>value switch { EducationProgram x=>Item(x.Id,null,null,null,x.Code,x.NameAr,x.NameEn,x.IsActive,x.IsDeleted,x.DeletedAtUtc,ProgramType:x.ProgramType.ToString()),AcademicYearDefinition x=>Item(x.Id,null,null,null,x.Code,x.NameAr,x.NameEn,x.IsActive,x.IsDeleted,x.DeletedAtUtc),ProgramAcademicYear x=>Item(x.Id,x.EducationProgramId,x.AcademicYearDefinitionId,null,x.Code,x.NameAr,x.NameEn,x.IsActive,x.IsDeleted,x.DeletedAtUtc,x.StartDate,x.EndDate,CurriculumPlanId:x.CurriculumPlanId),AcademicTerm x=>Item(x.Id,x.ProgramAcademicYearId,null,null,x.Code,x.NameAr,x.NameEn,x.IsActive,x.IsDeleted,x.DeletedAtUtc,x.StartDate,x.EndDate,x.SortOrder),AcademicPeriod x=>Item(x.Id,x.AcademicTermId,null,null,x.Code,x.NameAr,x.NameEn,x.IsActive,x.IsDeleted,x.DeletedAtUtc,x.StartDate,x.EndDate,x.SortOrder),SchoolDaySchedule x=>Item(x.Id,x.EducationProgramId,null,x.BranchId,x.DayOfWeek.ToString(),x.DayOfWeek.ToString(),x.DayOfWeek.ToString(),x.IsActive,x.IsDeleted,x.DeletedAtUtc,StartsAt:x.StartsAt,EndsAt:x.EndsAt,DayOfWeek:x.DayOfWeek.ToString()),SchoolCalendarEvent x=>Item(x.Id,x.EducationProgramId,x.ProgramAcademicYearId,x.BranchId,x.Code,x.NameAr,x.NameEn,x.IsActive,x.IsDeleted,x.DeletedAtUtc,x.StartDate,x.EndDate,EventType:x.EventType.ToString(),IsSchoolClosed:x.IsSchoolClosed),_=>throw new ArgumentOutOfRangeException(nameof(kind))};
    private static AcademicSettingItemResponse Item(Guid id,Guid? parent,Guid? secondary,Guid? branch,string code,string ar,string en,bool active,bool deleted,DateTimeOffset? deletedAt,DateOnly? startDate=null,DateOnly? endDate=null,int? sortOrder=null,string? ProgramType=null,string? EventType=null,bool? IsSchoolClosed=null,TimeOnly? StartsAt=null,TimeOnly? EndsAt=null,string? DayOfWeek=null,Guid? CurriculumPlanId=null)=>new(id,parent,secondary,branch,code,ar,en,active,deleted,deletedAt,startDate,endDate,sortOrder,ProgramType,EventType,IsSchoolClosed,StartsAt,EndsAt,DayOfWeek,CurriculumPlanId);
    private Task<SchoolsDbContext?> RequireDb(CancellationToken ct)=>dbFactory.CreateAsync(User.FindFirst(SchoolClaimTypes.SchoolCode)?.Value??string.Empty,ct);
    private Guid? CurrentUserId()=>Guid.TryParse(User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value,out var id)?id:null;
    private IActionResult Invalid()=>BadRequest(Failure(400,"academic_settings.invalid","Enter valid data."));
    private IActionResult InvalidKind()=>BadRequest(Failure(400,"academic_settings.kind_invalid","Setting kind is invalid."));
    private IActionResult InvalidParent()=>BadRequest(Failure(400,"academic_settings.parent_invalid","The selected parent or dates are invalid."));
    private IActionResult Duplicate()=>Conflict(Failure(409,"academic_settings.duplicate","The code or combination is already in use."));
    private IActionResult Missing()=>NotFound(Failure(404,"academic_settings.not_found","The setting was not found."));
    private ApiResponse<object?> Failure(int status,string code,string message)=>ApiResponse<object?>.Failure(status,code,message,correlationId:HttpContext.TraceIdentifier);
    private enum Kind { Invalid,Program,YearDefinition,ProgramYear,Term,Period,Schedule,CalendarEvent }
}

public sealed record SaveAcademicSettingRequest(
    string? Code,
    string? NameAr,
    string? NameEn,
    Guid? ParentId = null,
    Guid? SecondaryParentId = null,
    Guid? BranchId = null,
    string? ProgramType = null,
    DateOnly? StartDate = null,
    DateOnly? EndDate = null,
    int? SortOrder = null,
    string? DayOfWeek = null,
    [property: JsonConverter(typeof(FlexibleNullableTimeOnlyJsonConverter))] TimeOnly? StartsAt = null,
    [property: JsonConverter(typeof(FlexibleNullableTimeOnlyJsonConverter))] TimeOnly? EndsAt = null,
    string? EventType = null,
    bool? IsSchoolClosed = null);
public sealed record ChangeAcademicStatusRequest(bool IsActive);
public sealed record AcademicSettingItemResponse(Guid Id,Guid? ParentId,Guid? SecondaryParentId,Guid? BranchId,string Code,string NameAr,string NameEn,bool IsActive,bool IsDeleted,DateTimeOffset? DeletedAtUtc,DateOnly? StartDate,DateOnly? EndDate,int? SortOrder,string? ProgramType,string? EventType,bool? IsSchoolClosed,TimeOnly? StartsAt,TimeOnly? EndsAt,string? DayOfWeek,Guid? CurriculumPlanId);
public sealed record SchoolAcademicSettingsResponse(IReadOnlyList<AcademicSettingItemResponse> Programs,IReadOnlyList<AcademicSettingItemResponse> YearDefinitions,IReadOnlyList<AcademicSettingItemResponse> ProgramYears,IReadOnlyList<AcademicSettingItemResponse> Terms,IReadOnlyList<AcademicSettingItemResponse> Periods,IReadOnlyList<AcademicSettingItemResponse> Schedules,IReadOnlyList<AcademicSettingItemResponse> CalendarEvents);

public sealed class FlexibleNullableTimeOnlyJsonConverter : JsonConverter<TimeOnly?>
{
    public override TimeOnly? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null) return null;
        if (reader.TokenType != JsonTokenType.String) throw new JsonException("Time must be a string.");

        var value = reader.GetString();
        if (string.IsNullOrWhiteSpace(value)) return null;
        if (TimeOnly.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None, out var time)) return time;
        throw new JsonException("Time must use HH:mm or HH:mm:ss format.");
    }

    public override void Write(Utf8JsonWriter writer, TimeOnly? value, JsonSerializerOptions options)
    {
        if (value.HasValue) writer.WriteStringValue(value.Value.ToString("HH:mm:ss", CultureInfo.InvariantCulture));
        else writer.WriteNullValue();
    }
}
