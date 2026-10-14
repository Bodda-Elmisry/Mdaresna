using System.IdentityModel.Tokens.Jwt;
using System.Text.Json;
using Mdaresna.Api.Contracts;
using Mdaresna.Schools.Api.Auth;
using Mdaresna.Schools.Domain.Academics;
using Mdaresna.Schools.Domain.Identity;
using Mdaresna.Schools.Infrastructure.Persistence;
using Mdaresna.Schools.Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Mdaresna.Schools.Api.Controllers;

[ApiController, Authorize, Route("api/schools/v1/class-assignments")]
public sealed class SchoolClassAssignmentsController(ISchoolDbContextFactory factory) : ControllerBase
{
    private Guid UserId => Guid.TryParse(User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value, out var id) ? id : Guid.Empty;
    private Task<SchoolsDbContext?> Db(CancellationToken ct) => factory.CreateAsync(User.FindFirst(SchoolClaimTypes.SchoolCode)?.Value ?? "", ct);
    private Task<bool> CanManage(SchoolsDbContext db, CancellationToken ct) => ClassOperationalScope.HasPermission(db, UserId, "school.operations.manage", ct);
    private IActionResult Error(int status, string code) => StatusCode(status,
        ApiResponse<object?>.Failure(status, code, "راجع بيانات التكليف وصلاحياتك ثم أعد المحاولة. / Check assignment and permissions.", correlationId: HttpContext.TraceIdentifier));
    private IActionResult Success(object value) => Ok(ApiResponse<object>.Success(value, correlationId: HttpContext.TraceIdentifier));

    [HttpGet]
    public async Task<IActionResult> List([FromQuery] Guid? classSectionId, [FromQuery] Guid? userId, CancellationToken ct)
    {
        await using var db = await Db(ct); if (db is null) return Unauthorized();
        var query = db.ClassOperationalAssignments.AsNoTracking();
        if (!await CanManage(db, ct)) query = query.Where(x => x.UserId == UserId);
        if (classSectionId.HasValue) query = query.Where(x => x.Grants.Any(g => g.ClassSectionId == classSectionId));
        if (userId.HasValue) query = query.Where(x => x.UserId == userId);
        return Success(await query.OrderByDescending(x => x.CreatedAtUtc).Select(x => new
        {
            x.Id, x.UserId, UserName = x.User.Person.DisplayName, UserKind = x.User.Kind.ToString(),
            x.BuildingFloorId, x.Responsibility, x.StartsOn, x.EndsOn, x.IsActive, x.Revision,
            Grants = x.Grants.Select(g => new { g.ClassSectionId, ClassNameAr = g.ClassSection.NameAr,
                ClassNameEn = g.ClassSection.NameEn, g.Action, g.GradeSubjectOfferingId }).ToArray()
        }).ToArrayAsync(ct));
    }

    [HttpGet("options")]
    public async Task<IActionResult> Options(CancellationToken ct)
    {
        await using var db = await Db(ct); if (db is null) return Unauthorized();
        if (!await CanManage(db, ct)) return Forbid();
        var today = await ClassOperationalScope.Today(db, ct);
        return Success(new
        {
            Today = today,
            Users = await db.LocalUsers.Where(x => x.Status != LocalUserStatus.Disabled).Select(x => new
            { x.Id, Name = x.Person.DisplayName, Kind = x.Kind.ToString(), Permissions = x.Roles.Where(r => r.Role.IsActive).SelectMany(r => r.Role.Permissions.Select(p => p.Permission.Code)).Distinct().ToArray() }).ToArrayAsync(ct),
            Floors = await db.BuildingFloors.Where(x => x.IsActive).Select(x => new { x.Id, x.NameAr, x.NameEn, BuildingNameAr = x.Building.NameAr, BuildingNameEn = x.Building.NameEn }).ToArrayAsync(ct),
            Classes = await db.ClassSections.Where(x => x.IsActive && x.GradeOffering.IsActive && x.GradeOffering.Status != GradeOfferingStatus.Closed).Select(x => new
            {
                x.Id, x.NameAr, x.NameEn, x.GradeOffering.ProgramAcademicYearId,
                YearNameAr = x.GradeOffering.ProgramAcademicYear.NameAr, YearNameEn = x.GradeOffering.ProgramAcademicYear.NameEn,
                StartsOn = x.GradeOffering.ProgramAcademicYear.StartDate, EndsOn = x.GradeOffering.ProgramAcademicYear.EndDate,
                FloorId = x.RoomAssignments.Where(r => r.IsActive && r.IsPrimary && r.EffectiveFrom <= today && r.EffectiveTo >= today)
                    .OrderByDescending(r => r.EffectiveFrom).Select(r => (Guid?)r.Room.FloorId).FirstOrDefault(),
                Subjects = db.ClassSectionSubjects.Where(s => s.ClassSectionId == x.Id && s.IsActive && s.GradeSubjectOffering.IsActive)
                    .Select(s => new { Id = s.GradeSubjectOfferingId, NameAr = s.GradeSubjectOffering.CurriculumGradeSubject.Subject.NameAr, NameEn = s.GradeSubjectOffering.CurriculumGradeSubject.Subject.NameEn }).ToArray()
            }).ToArrayAsync(ct),
            Actions = ClassOperationalActions.Permissions.Select(x => new { Action = x.Key, Permission = x.Value }).ToArray()
        });
    }

    [HttpPost]
    public Task<IActionResult> Create([FromBody] SaveClassAssignmentRequest request, CancellationToken ct) => Save(null, request, ct);
    [HttpPut("{id:guid}")]
    public Task<IActionResult> Update(Guid id, [FromBody] SaveClassAssignmentRequest request, CancellationToken ct) => Save(id, request, ct);

    private async Task<IActionResult> Save(Guid? id, SaveClassAssignmentRequest request, CancellationToken ct)
    {
        await using var db = await Db(ct); if (db is null) return Unauthorized();
        if (!await CanManage(db, ct)) return Forbid();
        if (id.HasValue && !request.IsActive)
        {
            var ended = await db.ClassOperationalAssignments.SingleOrDefaultAsync(x => x.Id == id, ct);
            if (ended is null) return Error(404, "assignments.not_found");
            if (ended.Revision != request.Revision) return Error(409, "assignments.conflict");
            if (!ended.IsActive) return Success(new { ended.Id, ended.Revision });
            var changedAt = DateTimeOffset.UtcNow;
            ended.IsActive = false; ended.Revision++; ended.UpdatedAtUtc = changedAt;
            db.ClassOperationalAssignmentAudits.Add(new() { Id = Guid.NewGuid(), Assignment = ended, ActorUserId = UserId,
                Action = "Ended", SnapshotJson = JsonSerializer.Serialize(new { Before = new { IsActive = true }, After = new { IsActive = false } }), CreatedAtUtc = changedAt });
            db.SchoolUserNotifications.Add(new() { Id = Guid.NewGuid(), RecipientUserId = ended.UserId, Type = "ClassOperationalAssignment",
                TitleAr = "انتهاء تكليف تشغيلي", TitleEn = "Class assignment ended", BodyAr = $"تم إنهاء تكليفك: {ended.Responsibility}. راجع تكليفاتك لمعرفة نطاق العمل الحالي.",
                BodyEn = $"Your assignment ended: {ended.Responsibility}. Review your assignments for current scope.", RelatedEntityType = "ClassOperationalAssignment",
                RelatedEntityId = ended.Id.ToString(), CreatedAtUtc = changedAt, UpdatedAtUtc = changedAt });
            try { await db.SaveChangesAsync(ct); } catch (DbUpdateConcurrencyException) { return Error(409, "assignments.conflict"); }
            return Success(new { ended.Id, ended.Revision });
        }
        if (string.IsNullOrWhiteSpace(request.Responsibility) || request.Responsibility.Trim().Length > 200 ||
            request.StartsOn > request.EndsOn || request.Grants is null || request.Grants.Count == 0 || request.Grants.Count > 1000 ||
            request.Grants.Distinct().Count() != request.Grants.Count ||
            request.Grants.Any(x => !ClassOperationalActions.Permissions.ContainsKey(x.Action) ||
                (x.Action.StartsWith("exams.") != x.GradeSubjectOfferingId.HasValue))) return Error(400, "assignments.invalid");
        if (!await db.LocalUsers.AnyAsync(x => x.Id == request.UserId && x.Status != LocalUserStatus.Disabled &&
            (x.Kind == SchoolUserKind.Teacher || x.Kind == SchoolUserKind.Employee), ct)) return Error(400, "assignments.user_invalid");
        var classIds = request.Grants.Select(x => x.ClassSectionId).Distinct().ToArray();
        var classes = await db.ClassSections.Where(x => classIds.Contains(x.Id) && x.IsActive && x.GradeOffering.IsActive)
            .Select(x => new { x.Id, x.GradeOffering.ProgramAcademicYearId, Start = x.GradeOffering.ProgramAcademicYear.StartDate, End = x.GradeOffering.ProgramAcademicYear.EndDate }).ToArrayAsync(ct);
        if (classes.Length != classIds.Length || classes.Select(x => x.ProgramAcademicYearId).Distinct().Count() != 1 ||
            classes.Any(x => request.StartsOn < x.Start || request.EndsOn > x.End)) return Error(400, "assignments.period_invalid");
        foreach (var grant in request.Grants)
        {
            // A manager cannot delegate an action outside their own current permission ceiling.
            if (!await ClassOperationalScope.HasPermission(db, UserId, ClassOperationalActions.Permissions[grant.Action], ct)) return Forbid();
            if (grant.GradeSubjectOfferingId.HasValue && !await db.ClassSectionSubjects.AnyAsync(x => x.ClassSectionId == grant.ClassSectionId &&
                x.GradeSubjectOfferingId == grant.GradeSubjectOfferingId && x.IsActive && x.GradeSubjectOffering.IsActive, ct)) return Error(400, "assignments.subject_invalid");
        }
        if (request.BuildingFloorId.HasValue)
        {
            if (!await db.BuildingFloors.AnyAsync(x => x.Id == request.BuildingFloorId && x.IsActive, ct)) return Error(400, "assignments.floor_invalid");
            var today = await ClassOperationalScope.Today(db, ct);
            foreach (var classId in classIds)
                if (!await db.ClassRoomAssignments.AnyAsync(r => r.ClassSectionId == classId && r.IsActive && r.IsPrimary &&
                    r.EffectiveFrom <= today && r.EffectiveTo >= today && r.Room.FloorId == request.BuildingFloorId, ct)) return Error(400, "assignments.floor_invalid");
        }
        var now = DateTimeOffset.UtcNow;
        var assignment = id.HasValue ? await db.ClassOperationalAssignments.Include(x => x.Grants).SingleOrDefaultAsync(x => x.Id == id, ct) : null;
        if (id.HasValue && assignment is null) return Error(404, "assignments.not_found");
        if (assignment is not null && assignment.Revision != request.Revision) return Error(409, "assignments.conflict");
        var previous = assignment is null ? null : JsonSerializer.Serialize(new { assignment.UserId, assignment.BuildingFloorId, assignment.Responsibility,
            assignment.StartsOn, assignment.EndsOn, assignment.IsActive, Grants = assignment.Grants.Select(x => new { x.ClassSectionId, x.Action, x.GradeSubjectOfferingId }) });
        var previousUser = assignment?.UserId;
        if (assignment is null)
        {
            assignment = new() { Id = Guid.NewGuid(), CreatedByUserId = UserId, CreatedAtUtc = now };
            db.ClassOperationalAssignments.Add(assignment);
        }
        else { db.ClassOperationalGrants.RemoveRange(assignment.Grants); assignment.Revision++; }
        assignment.UserId = request.UserId; assignment.BuildingFloorId = request.BuildingFloorId;
        assignment.Responsibility = request.Responsibility.Trim(); assignment.StartsOn = request.StartsOn; assignment.EndsOn = request.EndsOn;
        assignment.IsActive = request.IsActive; assignment.UpdatedAtUtc = now;
        assignment.Grants = request.Grants.Select(x => new ClassOperationalGrant { Id = Guid.NewGuid(), Assignment = assignment,
            ClassSectionId = x.ClassSectionId, Action = x.Action, GradeSubjectOfferingId = x.GradeSubjectOfferingId }).ToList();
        db.ClassOperationalAssignmentAudits.Add(new() { Id = Guid.NewGuid(), Assignment = assignment, ActorUserId = UserId,
            Action = id.HasValue ? "Updated" : "Created", SnapshotJson = JsonSerializer.Serialize(new { Before = previous, After = request }), CreatedAtUtc = now });
        void Notify(Guid recipient, bool ended) => db.SchoolUserNotifications.Add(new()
        {
            Id = Guid.NewGuid(), RecipientUserId = recipient, Type = "ClassOperationalAssignment",
            TitleAr = ended ? "انتهاء تكليف تشغيلي" : "تكليف تشغيلي للفصول", TitleEn = ended ? "Class assignment ended" : "Class operational assignment",
            BodyAr = $"{(ended ? "انتهى أو تغير تكليفك" : "تم إنشاء أو تحديث تكليفك")}: {assignment.Responsibility}. الفترة {assignment.StartsOn:yyyy-MM-dd} — {assignment.EndsOn:yyyy-MM-dd}. راجع الفصول والإجراءات في تكليفاتي.",
            BodyEn = $"{(ended ? "Your assignment ended or changed" : "Your assignment was created or updated")}: {assignment.Responsibility}. {assignment.StartsOn:yyyy-MM-dd} — {assignment.EndsOn:yyyy-MM-dd}. Review classes and actions in My assignments.",
            RelatedEntityType = "ClassOperationalAssignment", RelatedEntityId = assignment.Id.ToString(), CreatedAtUtc = now, UpdatedAtUtc = now
        });
        if (previousUser.HasValue && previousUser != request.UserId) Notify(previousUser.Value, true);
        Notify(request.UserId, !request.IsActive);
        try { await db.SaveChangesAsync(ct); }
        catch (DbUpdateConcurrencyException) { return Error(409, "assignments.conflict"); }
        return Success(new { assignment.Id, assignment.Revision });
    }
}

public sealed record SaveClassAssignmentRequest(Guid UserId, Guid? BuildingFloorId, string Responsibility, DateOnly StartsOn,
    DateOnly EndsOn, bool IsActive, int Revision, IReadOnlyList<ClassAssignmentGrantRequest> Grants);
public sealed record ClassAssignmentGrantRequest(Guid ClassSectionId, string Action, Guid? GradeSubjectOfferingId);
