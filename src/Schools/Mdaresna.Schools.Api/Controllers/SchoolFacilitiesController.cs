using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Text.RegularExpressions;
using Mdaresna.Api.Contracts;
using Mdaresna.Schools.Api.Auth;
using Mdaresna.Schools.Domain.Facilities;
using Mdaresna.Schools.Infrastructure.Identity;
using Mdaresna.Schools.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Mdaresna.Schools.Api.Controllers;

[ApiController]
[Route("api/schools/v1/facilities")]
public sealed class SchoolFacilitiesController(ISchoolDbContextFactory dbFactory) : ControllerBase
{
    [Authorize(Policy = SchoolPermissionPolicies.FacilitiesView)]
    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] bool includeDeleted = false,
        CancellationToken cancellationToken = default)
    {
        await using var db = await RequireDb(cancellationToken); if (db is null) return Unauthorized();
        var branches = includeDeleted ? db.SchoolBranches.IgnoreQueryFilters() : db.SchoolBranches;
        var buildings = includeDeleted ? db.SchoolBuildings.IgnoreQueryFilters() : db.SchoolBuildings;
        var floors = includeDeleted ? db.BuildingFloors.IgnoreQueryFilters() : db.BuildingFloors;
        var roomTypes = includeDeleted ? db.SchoolRoomTypes.IgnoreQueryFilters() : db.SchoolRoomTypes;
        var capabilities = includeDeleted ? db.RoomCapabilities.IgnoreQueryFilters() : db.RoomCapabilities;
        var rooms = includeDeleted ? db.SchoolRooms.IgnoreQueryFilters() : db.SchoolRooms;
        var result = new SchoolFacilitiesResponse(
            await branches.AsNoTracking().OrderBy(x => x.Code).Select(x =>
                new FacilityItemResponse(x.Id, null, null, x.Code, x.NameAr, x.NameEn, x.IsActive, x.IsDeleted,
                    x.DeletedAtUtc, x.Address, null, null, null, Array.Empty<Guid>(), null, null)).ToArrayAsync(cancellationToken),
            await buildings.AsNoTracking().OrderBy(x => x.Code).Select(x =>
                new FacilityItemResponse(x.Id, x.BranchId, null, x.Code, x.NameAr, x.NameEn, x.IsActive, x.IsDeleted,
                    x.DeletedAtUtc, null, null, null, null, Array.Empty<Guid>(), null, null)).ToArrayAsync(cancellationToken),
            await floors.AsNoTracking().OrderBy(x => x.SortOrder).ThenBy(x => x.Code).Select(x =>
                new FacilityItemResponse(x.Id, x.BuildingId, null, x.Code, x.NameAr, x.NameEn, x.IsActive, x.IsDeleted,
                    x.DeletedAtUtc, null, x.SortOrder, null, null, Array.Empty<Guid>(), null, null)).ToArrayAsync(cancellationToken),
            await roomTypes.AsNoTracking().OrderBy(x => x.Code).Select(x =>
                new FacilityItemResponse(x.Id, null, null, x.Code, x.NameAr, x.NameEn, x.IsActive, x.IsDeleted,
                    x.DeletedAtUtc, null, null, x.IsLaboratory, null, Array.Empty<Guid>(), null, x.IsClassroom)).ToArrayAsync(cancellationToken),
            await capabilities.AsNoTracking().OrderBy(x => x.Code).Select(x =>
                new FacilityItemResponse(x.Id, null, null, x.Code, x.NameAr, x.NameEn, x.IsActive, x.IsDeleted,
                    x.DeletedAtUtc, null, null, null, null, Array.Empty<Guid>(), null, null)).ToArrayAsync(cancellationToken),
            await rooms.AsNoTracking().OrderBy(x => x.Code).Select(x =>
                new FacilityItemResponse(x.Id, x.FloorId, x.RoomTypeId, x.Code, x.NameAr, x.NameEn, x.IsActive,
                    x.IsDeleted, x.DeletedAtUtc, null, null, null, x.Capacity,
                    x.Capabilities.Select(c => c.CapabilityId).ToArray(), x.IsSchedulable, null)).ToArrayAsync(cancellationToken));
        return Ok(ApiResponse<SchoolFacilitiesResponse>.Success(result, correlationId: HttpContext.TraceIdentifier));
    }

    [Authorize(Policy = SchoolPermissionPolicies.FacilitiesManage)]
    [HttpPost("{kind}")]
    public async Task<IActionResult> Create(string kind, [FromBody] SaveFacilityItemRequest request,
        CancellationToken cancellationToken)
    {
        if (!TryNormalize(request, out var code, out var ar, out var en)) return Invalid();
        await using var db = await RequireDb(cancellationToken); if (db is null) return Unauthorized();
        var now = DateTimeOffset.UtcNow; Guid id;
        switch (NormalizeKind(kind))
        {
            case FacilityKind.Branch:
                if (await db.SchoolBranches.AnyAsync(x => x.Code == code, cancellationToken)) return Duplicate();
                var branch = new SchoolBranch { Id = Guid.NewGuid(), Code = code, NameAr = ar, NameEn = en,
                    Address = CleanOptional(request.Address, 500), IsActive = true, CreatedAtUtc = now, UpdatedAtUtc = now };
                db.SchoolBranches.Add(branch); id = branch.Id; break;
            case FacilityKind.Building:
                if (!request.ParentId.HasValue || !await db.SchoolBranches.AnyAsync(x => x.Id == request.ParentId, cancellationToken)) return InvalidParent();
                if (await db.SchoolBuildings.AnyAsync(x => x.BranchId == request.ParentId && x.Code == code, cancellationToken)) return Duplicate();
                var building = new SchoolBuilding { Id = Guid.NewGuid(), BranchId = request.ParentId.Value, Code = code,
                    NameAr = ar, NameEn = en, IsActive = true, CreatedAtUtc = now, UpdatedAtUtc = now };
                db.SchoolBuildings.Add(building); id = building.Id; break;
            case FacilityKind.Floor:
                if (!request.ParentId.HasValue || !await db.SchoolBuildings.AnyAsync(x => x.Id == request.ParentId, cancellationToken)) return InvalidParent();
                if (request.SortOrder is < -1000 or > 1000) return Invalid();
                if (await db.BuildingFloors.AnyAsync(x => x.BuildingId == request.ParentId && x.Code == code, cancellationToken)) return Duplicate();
                var floor = new BuildingFloor { Id = Guid.NewGuid(), BuildingId = request.ParentId.Value, Code = code,
                    NameAr = ar, NameEn = en, SortOrder = request.SortOrder ?? 0, IsActive = true,
                    CreatedAtUtc = now, UpdatedAtUtc = now };
                db.BuildingFloors.Add(floor); id = floor.Id; break;
            case FacilityKind.RoomType:
                if (await db.SchoolRoomTypes.AnyAsync(x => x.Code == code, cancellationToken)) return Duplicate();
                var roomType = new SchoolRoomType { Id = Guid.NewGuid(), Code = code, NameAr = ar, NameEn = en,
                    IsLaboratory = request.IsLaboratory ?? false, IsClassroom = request.IsClassroom ?? false,
                    IsActive = true, CreatedAtUtc = now, UpdatedAtUtc = now };
                db.SchoolRoomTypes.Add(roomType); id = roomType.Id; break;
            case FacilityKind.Capability:
                if (await db.RoomCapabilities.AnyAsync(x => x.Code == code, cancellationToken)) return Duplicate();
                var capability = new RoomCapability { Id = Guid.NewGuid(), Code = code, NameAr = ar, NameEn = en,
                    IsActive = true, CreatedAtUtc = now, UpdatedAtUtc = now };
                db.RoomCapabilities.Add(capability); id = capability.Id; break;
            case FacilityKind.Room:
                if (!request.ParentId.HasValue || !request.RoomTypeId.HasValue || request.Capacity is < 0 or > 10000 ||
                    !await db.BuildingFloors.AnyAsync(x => x.Id == request.ParentId, cancellationToken) ||
                    !await db.SchoolRoomTypes.AnyAsync(x => x.Id == request.RoomTypeId && x.IsActive, cancellationToken)) return InvalidParent();
                if (await db.SchoolRooms.AnyAsync(x => x.FloorId == request.ParentId && x.Code == code, cancellationToken)) return Duplicate();
                var capabilityIds = request.CapabilityIds.Distinct().ToArray();
                if (await db.RoomCapabilities.CountAsync(x => capabilityIds.Contains(x.Id) && x.IsActive, cancellationToken) != capabilityIds.Length) return InvalidCapabilities();
                var room = new SchoolRoom { Id = Guid.NewGuid(), FloorId = request.ParentId.Value,
                    RoomTypeId = request.RoomTypeId.Value, Code = code, NameAr = ar, NameEn = en,
                    Capacity = request.Capacity ?? 0, IsSchedulable = request.IsSchedulable ?? true, IsActive = true,
                    CreatedAtUtc = now, UpdatedAtUtc = now };
                room.Capabilities = capabilityIds.Select(x => new SchoolRoomCapability { RoomId = room.Id,
                    CapabilityId = x, Room = room }).ToList();
                db.SchoolRooms.Add(room); id = room.Id; break;
            default: return InvalidKind();
        }
        try { await db.SaveChangesAsync(cancellationToken); }
        catch (DbUpdateException) { return Duplicate(); }
        return StatusCode(201, ApiResponse<object>.Success(new { id }, statusCode: 201,
            correlationId: HttpContext.TraceIdentifier));
    }

    [Authorize(Policy = SchoolPermissionPolicies.FacilitiesManage)]
    [HttpPut("{kind}/{id:guid}")]
    public async Task<IActionResult> Update(string kind, Guid id, [FromBody] SaveFacilityItemRequest request,
        CancellationToken cancellationToken)
    {
        if (!TryNormalize(request, out var code, out var ar, out var en)) return Invalid();
        await using var db = await RequireDb(cancellationToken); if (db is null) return Unauthorized();
        var now = DateTimeOffset.UtcNow;
        switch (NormalizeKind(kind))
        {
            case FacilityKind.Branch:
                var branch = await db.SchoolBranches.SingleOrDefaultAsync(x => x.Id == id, cancellationToken); if (branch is null) return Missing();
                branch.Code = code; branch.NameAr = ar; branch.NameEn = en; branch.Address = CleanOptional(request.Address, 500); branch.UpdatedAtUtc = now; break;
            case FacilityKind.Building:
                var building = await db.SchoolBuildings.SingleOrDefaultAsync(x => x.Id == id, cancellationToken); if (building is null) return Missing();
                if (!request.ParentId.HasValue || !await db.SchoolBranches.AnyAsync(x => x.Id == request.ParentId, cancellationToken)) return InvalidParent();
                building.BranchId = request.ParentId.Value; building.Code = code; building.NameAr = ar; building.NameEn = en; building.UpdatedAtUtc = now; break;
            case FacilityKind.Floor:
                var floor = await db.BuildingFloors.SingleOrDefaultAsync(x => x.Id == id, cancellationToken); if (floor is null) return Missing();
                if (!request.ParentId.HasValue || request.SortOrder is < -1000 or > 1000 || !await db.SchoolBuildings.AnyAsync(x => x.Id == request.ParentId, cancellationToken)) return InvalidParent();
                floor.BuildingId = request.ParentId.Value; floor.Code = code; floor.NameAr = ar; floor.NameEn = en; floor.SortOrder = request.SortOrder ?? 0; floor.UpdatedAtUtc = now; break;
            case FacilityKind.RoomType:
                var type = await db.SchoolRoomTypes.SingleOrDefaultAsync(x => x.Id == id, cancellationToken); if (type is null) return Missing();
                if (!(request.IsClassroom ?? false) && await db.ClassRoomAssignments.AnyAsync(x => x.Room.RoomTypeId == id && x.IsActive, cancellationToken))
                    return Conflict(Failure(409, "facilities.classroom_in_use", "This room type is used by active class assignments."));
                type.Code = code; type.NameAr = ar; type.NameEn = en; type.IsLaboratory = request.IsLaboratory ?? false;
                type.IsClassroom = request.IsClassroom ?? false; type.UpdatedAtUtc = now; break;
            case FacilityKind.Capability:
                var capability = await db.RoomCapabilities.SingleOrDefaultAsync(x => x.Id == id, cancellationToken); if (capability is null) return Missing();
                capability.Code = code; capability.NameAr = ar; capability.NameEn = en; capability.UpdatedAtUtc = now; break;
            case FacilityKind.Room:
                var room = await db.SchoolRooms.Include(x => x.Capabilities).SingleOrDefaultAsync(x => x.Id == id, cancellationToken); if (room is null) return Missing();
                if (!request.ParentId.HasValue || !request.RoomTypeId.HasValue || request.Capacity is < 0 or > 10000 ||
                    !await db.BuildingFloors.AnyAsync(x => x.Id == request.ParentId, cancellationToken) ||
                    !await db.SchoolRoomTypes.AnyAsync(x => x.Id == request.RoomTypeId && x.IsActive, cancellationToken)) return InvalidParent();
                if (await db.ClassRoomAssignments.AnyAsync(x => x.RoomId == id && x.IsActive, cancellationToken) &&
                    (!(request.IsSchedulable ?? true) || !await db.SchoolRoomTypes.AnyAsync(x => x.Id == request.RoomTypeId && x.IsClassroom, cancellationToken) ||
                     await db.ClassRoomAssignments.AnyAsync(x => x.RoomId == id && x.IsActive && x.ClassSection.Capacity > (request.Capacity ?? 0), cancellationToken)))
                    return Conflict(Failure(409, "facilities.room_in_use", "The room is assigned to an active class and must remain a schedulable classroom with enough capacity."));
                var capabilityIds = request.CapabilityIds.Distinct().ToArray();
                if (await db.RoomCapabilities.CountAsync(x => capabilityIds.Contains(x.Id) && x.IsActive, cancellationToken) != capabilityIds.Length) return InvalidCapabilities();
                db.SchoolRoomCapabilities.RemoveRange(room.Capabilities);
                room.FloorId = request.ParentId.Value; room.RoomTypeId = request.RoomTypeId.Value; room.Code = code;
                room.NameAr = ar; room.NameEn = en; room.Capacity = request.Capacity ?? 0;
                room.IsSchedulable = request.IsSchedulable ?? true; room.UpdatedAtUtc = now;
                room.Capabilities = capabilityIds.Select(x => new SchoolRoomCapability { RoomId = room.Id,
                    CapabilityId = x, Room = room }).ToList(); break;
            default: return InvalidKind();
        }
        try { await db.SaveChangesAsync(cancellationToken); }
        catch (DbUpdateException) { return Duplicate(); }
        return Ok(ApiResponse<object?>.Success(null, correlationId: HttpContext.TraceIdentifier));
    }

    [Authorize(Policy = SchoolPermissionPolicies.FacilitiesManage)]
    [HttpPut("{kind}/{id:guid}/status")]
    public async Task<IActionResult> Status(string kind, Guid id, [FromBody] ChangeFacilityStatusRequest request,
        CancellationToken cancellationToken)
    {
        var normalized = NormalizeKind(kind);
        if (!request.IsActive && normalized is FacilityKind.Room or FacilityKind.RoomType)
        {
            await using var db = await RequireDb(cancellationToken); if (db is null) return Unauthorized();
            var inUse = normalized == FacilityKind.Room
                ? await db.ClassRoomAssignments.AnyAsync(x => x.RoomId == id && x.IsActive, cancellationToken)
                : await db.ClassRoomAssignments.AnyAsync(x => x.Room.RoomTypeId == id && x.IsActive, cancellationToken);
            if (inUse) return Conflict(Failure(409, "facilities.classroom_in_use",
                "The classroom is used by an active class assignment."));
        }
        return await Mutate(kind, id, cancellationToken, entity =>
        {
            SetActive(entity, request.IsActive); SetUpdated(entity, DateTimeOffset.UtcNow); return null;
        });
    }

    [Authorize(Policy = SchoolPermissionPolicies.FacilitiesDelete)]
    [HttpDelete("{kind}/{id:guid}")]
    public async Task<IActionResult> Delete(string kind, Guid id, CancellationToken cancellationToken)
    {
        await using var db = await RequireDb(cancellationToken); if (db is null) return Unauthorized();
        var normalized = NormalizeKind(kind); var entity = await Find(db, normalized, id, false, cancellationToken);
        if (entity is null) return Missing();
        if (await HasChildren(db, normalized, id, cancellationToken)) return Conflict(Failure(409,
            "facilities.has_children", "Delete or move child records first."));
        var now = DateTimeOffset.UtcNow; var soft = (ISoftDeletableSchoolEntity)entity;
        soft.IsDeleted = true; soft.DeletedAtUtc = now; soft.DeletedByUserId = CurrentUserId();
        SetActive(entity, false); SetUpdated(entity, now); await db.SaveChangesAsync(cancellationToken);
        return Ok(ApiResponse<object?>.Success(null, correlationId: HttpContext.TraceIdentifier));
    }

    [Authorize(Policy = SchoolPermissionPolicies.FacilitiesRestore)]
    [HttpPost("{kind}/{id:guid}/restore")]
    public async Task<IActionResult> Restore(string kind, Guid id, CancellationToken cancellationToken)
    {
        await using var db = await RequireDb(cancellationToken); if (db is null) return Unauthorized();
        var normalized = NormalizeKind(kind); var entity = await Find(db, normalized, id, true, cancellationToken);
        if (entity is not ISoftDeletableSchoolEntity soft || !soft.IsDeleted) return Missing();
        if (!await ParentAvailable(db, normalized, entity, cancellationToken)) return Conflict(Failure(409,
            "facilities.parent_unavailable", "Restore the parent record first."));
        if (await RestoreConflicts(db, normalized, entity, cancellationToken)) return Duplicate();
        soft.IsDeleted = false; soft.DeletedAtUtc = null; soft.DeletedByUserId = null;
        SetUpdated(entity, DateTimeOffset.UtcNow); await db.SaveChangesAsync(cancellationToken);
        return Ok(ApiResponse<object?>.Success(null, correlationId: HttpContext.TraceIdentifier));
    }

    private async Task<IActionResult> Mutate(string kind, Guid id, CancellationToken ct, Func<object, IActionResult?> mutation)
    {
        await using var db = await RequireDb(ct); if (db is null) return Unauthorized();
        var entity = await Find(db, NormalizeKind(kind), id, false, ct); if (entity is null) return Missing();
        var error = mutation(entity); if (error is not null) return error;
        await db.SaveChangesAsync(ct); return Ok(ApiResponse<object?>.Success(null, correlationId: HttpContext.TraceIdentifier));
    }

    private static async Task<object?> Find(SchoolsDbContext db, FacilityKind kind, Guid id, bool deleted, CancellationToken ct) => kind switch
    {
        FacilityKind.Branch => await (deleted ? db.SchoolBranches.IgnoreQueryFilters() : db.SchoolBranches).SingleOrDefaultAsync(x => x.Id == id, ct),
        FacilityKind.Building => await (deleted ? db.SchoolBuildings.IgnoreQueryFilters() : db.SchoolBuildings).SingleOrDefaultAsync(x => x.Id == id, ct),
        FacilityKind.Floor => await (deleted ? db.BuildingFloors.IgnoreQueryFilters() : db.BuildingFloors).SingleOrDefaultAsync(x => x.Id == id, ct),
        FacilityKind.RoomType => await (deleted ? db.SchoolRoomTypes.IgnoreQueryFilters() : db.SchoolRoomTypes).SingleOrDefaultAsync(x => x.Id == id, ct),
        FacilityKind.Capability => await (deleted ? db.RoomCapabilities.IgnoreQueryFilters() : db.RoomCapabilities).SingleOrDefaultAsync(x => x.Id == id, ct),
        FacilityKind.Room => await (deleted ? db.SchoolRooms.IgnoreQueryFilters() : db.SchoolRooms).SingleOrDefaultAsync(x => x.Id == id, ct),
        _ => null
    };

    private static Task<bool> HasChildren(SchoolsDbContext db, FacilityKind kind, Guid id, CancellationToken ct) => kind switch
    {
        FacilityKind.Branch => db.SchoolBuildings.AnyAsync(x => x.BranchId == id, ct),
        FacilityKind.Building => db.BuildingFloors.AnyAsync(x => x.BuildingId == id, ct),
        FacilityKind.Floor => db.SchoolRooms.AnyAsync(x => x.FloorId == id, ct),
        FacilityKind.RoomType => db.SchoolRooms.AnyAsync(x => x.RoomTypeId == id, ct),
        FacilityKind.Capability => db.SchoolRooms.AnyAsync(x => x.Capabilities.Any(c => c.CapabilityId == id), ct),
        FacilityKind.Room => db.ClassRoomAssignments.AnyAsync(x => x.RoomId == id, ct),
        _ => Task.FromResult(false)
    };

    private static async Task<bool> ParentAvailable(SchoolsDbContext db, FacilityKind kind, object entity, CancellationToken ct) => kind switch
    {
        FacilityKind.Building => await db.SchoolBranches.AnyAsync(x => x.Id == ((SchoolBuilding)entity).BranchId, ct),
        FacilityKind.Floor => await db.SchoolBuildings.AnyAsync(x => x.Id == ((BuildingFloor)entity).BuildingId, ct),
        FacilityKind.Room => await db.BuildingFloors.AnyAsync(x => x.Id == ((SchoolRoom)entity).FloorId, ct) &&
            await db.SchoolRoomTypes.AnyAsync(x => x.Id == ((SchoolRoom)entity).RoomTypeId, ct),
        _ => true
    };

    private static Task<bool> RestoreConflicts(SchoolsDbContext db, FacilityKind kind, object entity, CancellationToken ct) => kind switch
    {
        FacilityKind.Branch => db.SchoolBranches.AnyAsync(x => x.Code == ((SchoolBranch)entity).Code && x.Id != ((SchoolBranch)entity).Id, ct),
        FacilityKind.Building => db.SchoolBuildings.AnyAsync(x => x.BranchId == ((SchoolBuilding)entity).BranchId && x.Code == ((SchoolBuilding)entity).Code && x.Id != ((SchoolBuilding)entity).Id, ct),
        FacilityKind.Floor => db.BuildingFloors.AnyAsync(x => x.BuildingId == ((BuildingFloor)entity).BuildingId && x.Code == ((BuildingFloor)entity).Code && x.Id != ((BuildingFloor)entity).Id, ct),
        FacilityKind.RoomType => db.SchoolRoomTypes.AnyAsync(x => x.Code == ((SchoolRoomType)entity).Code && x.Id != ((SchoolRoomType)entity).Id, ct),
        FacilityKind.Capability => db.RoomCapabilities.AnyAsync(x => x.Code == ((RoomCapability)entity).Code && x.Id != ((RoomCapability)entity).Id, ct),
        FacilityKind.Room => db.SchoolRooms.AnyAsync(x => x.FloorId == ((SchoolRoom)entity).FloorId && x.Code == ((SchoolRoom)entity).Code && x.Id != ((SchoolRoom)entity).Id, ct),
        _ => Task.FromResult(true)
    };

    private static void SetActive(object entity, bool active)
    {
        switch (entity) { case SchoolBranch x: x.IsActive = active; break; case SchoolBuilding x: x.IsActive = active; break;
            case BuildingFloor x: x.IsActive = active; break; case SchoolRoomType x: x.IsActive = active; break;
            case RoomCapability x: x.IsActive = active; break; case SchoolRoom x: x.IsActive = active; break; }
    }
    private static void SetUpdated(object entity, DateTimeOffset now)
    {
        switch (entity) { case SchoolBranch x: x.UpdatedAtUtc = now; break; case SchoolBuilding x: x.UpdatedAtUtc = now; break;
            case BuildingFloor x: x.UpdatedAtUtc = now; break; case SchoolRoomType x: x.UpdatedAtUtc = now; break;
            case RoomCapability x: x.UpdatedAtUtc = now; break; case SchoolRoom x: x.UpdatedAtUtc = now; break; }
    }
    private static FacilityKind NormalizeKind(string value) => value.Trim().ToLowerInvariant() switch
    {
        "branches" => FacilityKind.Branch, "buildings" => FacilityKind.Building, "floors" => FacilityKind.Floor,
        "room-types" => FacilityKind.RoomType, "capabilities" => FacilityKind.Capability, "rooms" => FacilityKind.Room,
        _ => FacilityKind.Invalid
    };
    private static bool TryNormalize(SaveFacilityItemRequest request, out string code, out string ar, out string en)
    {
        code = request.Code?.Trim().ToUpperInvariant() ?? string.Empty; ar = request.NameAr?.Trim() ?? string.Empty;
        en = request.NameEn?.Trim() ?? string.Empty;
        return Regex.IsMatch(code, "^[A-Z0-9][A-Z0-9._-]{0,49}$") && ar.Length is > 0 and <= 150 && en.Length is > 0 and <= 150;
    }
    private static string? CleanOptional(string? value, int max) => string.IsNullOrWhiteSpace(value) ? null : value.Trim()[..Math.Min(value.Trim().Length, max)];
    private Task<SchoolsDbContext?> RequireDb(CancellationToken ct) => dbFactory.CreateAsync(User.FindFirst(SchoolClaimTypes.SchoolCode)?.Value ?? string.Empty, ct);
    private Guid? CurrentUserId() => Guid.TryParse(User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value, out var id) ? id : null;
    private IActionResult Invalid() => BadRequest(Failure(400, "facilities.invalid", "Enter valid code and names."));
    private IActionResult InvalidKind() => BadRequest(Failure(400, "facilities.kind_invalid", "Facility kind is invalid."));
    private IActionResult InvalidParent() => BadRequest(Failure(400, "facilities.parent_invalid", "The selected parent or room type is invalid."));
    private IActionResult InvalidCapabilities() => BadRequest(Failure(400, "facilities.capabilities_invalid", "One or more capabilities are invalid."));
    private IActionResult Duplicate() => Conflict(Failure(409, "facilities.duplicate", "The code is already in use."));
    private IActionResult Missing() => NotFound(Failure(404, "facilities.not_found", "The facility record was not found."));
    private ApiResponse<object?> Failure(int status, string code, string message) => ApiResponse<object?>.Failure(status, code, message, correlationId: HttpContext.TraceIdentifier);
    private enum FacilityKind { Invalid, Branch, Building, Floor, RoomType, Capability, Room }
}

public sealed record SaveFacilityItemRequest([Required] string Code, [Required] string NameAr, [Required] string NameEn,
    Guid? ParentId = null, Guid? RoomTypeId = null, string? Address = null, int? SortOrder = null,
    bool? IsLaboratory = null, bool? IsClassroom = null, int? Capacity = null, bool? IsSchedulable = null,
    IReadOnlyList<Guid>? Capabilities = null)
{
    public IReadOnlyList<Guid> CapabilityIds => Capabilities ?? [];
}
public sealed record ChangeFacilityStatusRequest(bool IsActive);
public sealed record FacilityItemResponse(Guid Id, Guid? ParentId, Guid? RoomTypeId, string Code, string NameAr,
    string NameEn, bool IsActive, bool IsDeleted, DateTimeOffset? DeletedAtUtc, string? Address, int? SortOrder,
    bool? IsLaboratory, int? Capacity, IReadOnlyList<Guid> CapabilityIds, bool? IsSchedulable = null,
    bool? IsClassroom = null);
public sealed record SchoolFacilitiesResponse(IReadOnlyList<FacilityItemResponse> Branches,
    IReadOnlyList<FacilityItemResponse> Buildings, IReadOnlyList<FacilityItemResponse> Floors,
    IReadOnlyList<FacilityItemResponse> RoomTypes, IReadOnlyList<FacilityItemResponse> Capabilities,
    IReadOnlyList<FacilityItemResponse> Rooms);
