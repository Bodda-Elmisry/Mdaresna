using Mdaresna.Api.Contracts;
using Mdaresna.Schools.Api.Auth;
using Mdaresna.Schools.Domain.Academics;
using Mdaresna.Schools.Domain.Facilities;
using Mdaresna.Schools.Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Mdaresna.Schools.Api.Controllers;

/// <summary>Bounded administration lists. The legacy aggregate endpoints remain compatible with existing clients.</summary>
[ApiController, Authorize, Route("api/schools/v1/settings-lists")]
public sealed class SchoolSettingsListsController(ISchoolDbContextFactory factory) : ControllerBase
{
    [HttpGet("{area}/{kind}")]
    public async Task<IActionResult> Get(string area, string kind, int pageNumber = 1, int pageSize = 20, bool deleted = false, CancellationToken ct = default, Guid? programAcademicYearId = null, Guid? academicTermId = null, Guid? parentId = null, Guid? educationProgramId = null, Guid? gradeOfferingId = null, Guid? curriculumPlanId = null, Guid? bookId = null)
    {
        if (pageNumber < 1 || pageNumber > 1000000 || pageSize is < 1 or > 100) return BadRequest();
        var permission = area switch { "facilities" => "facilities", "academic" => kind == "schedules" ? "operations" : kind == "calendar-events" ? "calendar" : "academics", "classes" or "curriculum" => "academics", _ => null };
        if (permission is null) return NotFound();
        if (!User.HasClaim(SchoolClaimTypes.Permission, $"school.{permission}.view")) return Forbid();
        await using var db = await factory.CreateAsync(User.FindFirst(SchoolClaimTypes.SchoolCode)?.Value ?? "", ct);
        if (db is null) return Unauthorized();
        if (area == "academic" && kind == "term-groups")
        {
            // Page parents first: a term page must never decide which years exist in the grouped view.
            var terms = db.AcademicTerms.IgnoreQueryFilters().AsNoTracking().Where(x => x.IsDeleted == deleted);
            var years = db.ProgramAcademicYears.IgnoreQueryFilters().AsNoTracking()
                .Where(y => terms.Any(t => t.ProgramAcademicYearId == y.Id));
            var total = await years.CountAsync(ct);
            var page = Math.Min(pageNumber, Math.Max(1, (int)Math.Ceiling(total / (double)pageSize)));
            var rows = await years.Include(y => y.EducationProgram).Include(y => y.AcademicYearDefinition)
                .OrderByDescending(y => y.StartDate).ThenBy(y => y.EducationProgram.Code).ThenBy(y => y.Id)
                .Skip((page - 1) * pageSize).Take(pageSize).ToArrayAsync(ct);
            var ids = rows.Select(y => y.Id).ToArray();
            var counts = await terms.Where(t => ids.Contains(t.ProgramAcademicYearId))
                .GroupBy(t => t.ProgramAcademicYearId).Select(g => new { Id = g.Key, Count = g.Count() }).ToDictionaryAsync(x => x.Id, x => x.Count, ct);
            return Ok(ApiResponse<object>.Success(new
            {
                items = rows.Select(y => new { year = SchoolAcademicSettingsController.MapForList(y),
                    nameAr = $"{y.AcademicYearDefinition.NameAr} — {y.EducationProgram.NameAr}",
                    nameEn = $"{y.AcademicYearDefinition.NameEn} — {y.EducationProgram.NameEn}", termCount = counts.GetValueOrDefault(y.Id) }),
                totalCount = total, pageNumber = page, pageSize, totalPages = Math.Max(1, (int)Math.Ceiling(total / (double)pageSize))
            }, correlationId: HttpContext.TraceIdentifier));
        }
        if (area == "classes" && kind == "grade-program-groups")
        {
            var grades = db.GradeLevels.IgnoreQueryFilters().Where(g => g.IsDeleted == deleted);
            var stages = db.EducationStages.IgnoreQueryFilters().Where(s => grades.Any(g => g.EducationStageId == s.Id));
            var programs = db.EducationPrograms.IgnoreQueryFilters().AsNoTracking().Where(p => stages.Any(s => s.EducationProgramId == p.Id));
            var total = await programs.CountAsync(ct);
            var page = Math.Min(pageNumber, Math.Max(1,(int)Math.Ceiling(total/(double)pageSize)));
            var rows = await programs.OrderBy(p=>p.Code).ThenBy(p=>p.Id).Skip((page-1)*pageSize).Take(pageSize).ToArrayAsync(ct);
            var ids = rows.Select(p=>p.Id).ToArray();
            var counts = await stages.Where(s=>ids.Contains(s.EducationProgramId)).GroupBy(s=>s.EducationProgramId)
                .Select(g=>new {Id=g.Key,Count=g.Count()}).ToDictionaryAsync(x=>x.Id,x=>x.Count,ct);
            return Ok(ApiResponse<object>.Success(new {items=rows.Select(p=>new {id=p.Id,nameAr=p.NameAr,nameEn=p.NameEn,itemCount=counts.GetValueOrDefault(p.Id)}),
                totalCount=total,pageNumber=page,pageSize,totalPages=Math.Max(1,(int)Math.Ceiling(total/(double)pageSize))},correlationId:HttpContext.TraceIdentifier));
        }
        if (area == "curriculum" && kind == "offering-program-groups")
        {
            var offerings = db.GradeSubjectOfferings.IgnoreQueryFilters().AsNoTracking().Where(o=>o.IsDeleted==deleted);
            var programs = db.EducationPrograms.IgnoreQueryFilters().AsNoTracking()
                .Where(p=>offerings.Any(o=>o.GradeOffering.ProgramAcademicYear.EducationProgramId==p.Id));
            var total = await programs.CountAsync(ct);
            var page = Math.Min(pageNumber,Math.Max(1,(int)Math.Ceiling(total/(double)pageSize)));
            var rows = await programs.OrderBy(p=>p.Code).ThenBy(p=>p.Id).Skip((page-1)*pageSize).Take(pageSize).ToArrayAsync(ct);
            var ids = rows.Select(p=>p.Id).ToArray();
            var counts = await offerings.Where(o=>ids.Contains(o.GradeOffering.ProgramAcademicYear.EducationProgramId))
                .GroupBy(o=>o.GradeOffering.ProgramAcademicYear.EducationProgramId)
                .Select(g=>new {Id=g.Key,Count=g.Count()}).ToDictionaryAsync(x=>x.Id,x=>x.Count,ct);
            return Ok(ApiResponse<object>.Success(new {items=rows.Select(p=>new {id=p.Id,nameAr=p.NameAr,nameEn=p.NameEn,itemCount=counts.GetValueOrDefault(p.Id)}),
                totalCount=total,pageNumber=page,pageSize,totalPages=Math.Max(1,(int)Math.Ceiling(total/(double)pageSize))},correlationId:HttpContext.TraceIdentifier));
        }
        async Task<IActionResult> Groups<TParent,TChild>(IQueryable<TParent> parents, IQueryable<TChild> children, string foreignKey, bool leafChildren, params string[] includes) where TParent : class where TChild : class
        {
            var childQuery = children.IgnoreQueryFilters().AsNoTracking();
            // Intermediate ancestors follow the leaf's recycle-bin state, even when the ancestor itself is active.
            if (leafChildren) childQuery = childQuery.Where(c => EF.Property<bool>(c,"IsDeleted") == deleted);
            var parentQuery = parents.IgnoreQueryFilters().AsNoTracking().Where(p => childQuery.Any(c => EF.Property<Guid>(c,foreignKey) == EF.Property<Guid>(p,"Id")));
            var total = await parentQuery.CountAsync(ct);
            var page = Math.Min(pageNumber, Math.Max(1,(int)Math.Ceiling(total/(double)pageSize)));
            foreach (var include in includes) parentQuery = parentQuery.Include(include);
            var rows = await parentQuery.OrderBy(p => EF.Property<string>(p,"Code")).ThenBy(p => EF.Property<Guid>(p,"Id"))
                .Skip((page-1)*pageSize).Take(pageSize).AsSplitQuery().ToArrayAsync(ct);
            var ids = rows.Select(p => EFId(p)).ToArray();
            var counts = await childQuery.Where(c => ids.Contains(EF.Property<Guid>(c,foreignKey))).GroupBy(c => EF.Property<Guid>(c,foreignKey))
                .Select(g => new { Id=g.Key, Count=g.Count() }).ToDictionaryAsync(x=>x.Id,x=>x.Count,ct);
            var scoped = educationProgramId.HasValue || programAcademicYearId.HasValue || gradeOfferingId.HasValue;
            string Label(TParent p, bool en) => scoped ? p is ProgramAcademicYear year ? Name(year.AcademicYearDefinition,en) : p is GradeOffering offering ? Name(offering.GradeLevel,en) : Name(p,en) : GroupName(p,en);
            return Ok(ApiResponse<object>.Success(new { items=rows.Select(p=>new { id=EFId(p), nameAr=Label(p,false), nameEn=Label(p,true), itemCount=counts.GetValueOrDefault(EFId(p)) }),
                totalCount=total,pageNumber=page,pageSize,totalPages=Math.Max(1,(int)Math.Ceiling(total/(double)pageSize)) },correlationId:HttpContext.TraceIdentifier));
        }
        if (area == "classes" && kind.EndsWith("-groups",StringComparison.Ordinal))
        {
            var assignments = db.ClassRoomAssignments.IgnoreQueryFilters().Where(a=>a.IsDeleted==deleted);
            var sections = db.ClassSections.IgnoreQueryFilters().Where(s=>kind.StartsWith("room-assignments",StringComparison.Ordinal)
                ? assignments.Any(a=>a.ClassSectionId==s.Id) : s.IsDeleted==deleted);
            var offerings = db.GradeOfferings.IgnoreQueryFilters().Where(o=>sections.Any(s=>s.GradeOfferingId==o.Id));
            var years = db.ProgramAcademicYears.IgnoreQueryFilters().Where(y=>offerings.Any(o=>o.ProgramAcademicYearId==y.Id));
            return kind switch
            {
                "stages-groups" => await Groups(db.EducationPrograms,db.EducationStages,"EducationProgramId",true),
                "grades-groups" => await Groups(db.EducationStages.Where(s=>!educationProgramId.HasValue||s.EducationProgramId==educationProgramId.Value),db.GradeLevels,"EducationStageId",true,"EducationProgram"),
                "offerings-groups" => await Groups(db.ProgramAcademicYears,db.GradeOfferings,"ProgramAcademicYearId",true,"EducationProgram","AcademicYearDefinition"),
                "sections-program-groups" or "room-assignments-program-groups" => await Groups(db.EducationPrograms,years,"EducationProgramId",false),
                "sections-year-groups" or "room-assignments-year-groups" => await Groups(db.ProgramAcademicYears.Where(y=>!educationProgramId.HasValue||y.EducationProgramId==educationProgramId.Value),offerings,"ProgramAcademicYearId",false,"EducationProgram","AcademicYearDefinition"),
                "room-assignments-grade-groups" => await Groups(db.GradeOfferings.Where(o=>!programAcademicYearId.HasValue||o.ProgramAcademicYearId==programAcademicYearId.Value),sections,"GradeOfferingId",false,"GradeLevel","ProgramAcademicYear.EducationProgram","ProgramAcademicYear.AcademicYearDefinition"),
                "sections-groups" => await Groups(db.GradeOfferings.Where(o=>!programAcademicYearId.HasValue||o.ProgramAcademicYearId==programAcademicYearId.Value),db.ClassSections,"GradeOfferingId",true,"GradeLevel","ProgramAcademicYear.EducationProgram","ProgramAcademicYear.AcademicYearDefinition"),
                "room-assignments-groups" => await Groups(db.ClassSections.Where(s=>!gradeOfferingId.HasValue||s.GradeOfferingId==gradeOfferingId.Value),db.ClassRoomAssignments,"ClassSectionId",true,"GradeOffering.ProgramAcademicYear.EducationProgram","GradeOffering.ProgramAcademicYear.AcademicYearDefinition"),
                _ => NotFound()
            };
        }
        async Task<IActionResult> Read<T>(IQueryable<T> source, Func<T, object> map, params string[] includes) where T : class
        {
            var query = source.IgnoreQueryFilters().AsNoTracking();
            // Tree queries include intact ancestors needed to reach deleted descendants.
            if (!(area == "facilities" && kind.StartsWith("tree-",StringComparison.Ordinal)))
                query = query.Where(x => EF.Property<bool>(x, "IsDeleted") == deleted);
            var total = await query.CountAsync(ct);
            var page = Math.Min(pageNumber, Math.Max(1, (int)Math.Ceiling(total / (double)pageSize)));
            foreach (var include in includes) query = query.Include(include);
            var orderProperty = typeof(T).GetProperty("Code") != null ? "Code" : typeof(T).GetProperty("EditionCode") != null ? "EditionCode" : null;
            var ordered = orderProperty == null ? query.OrderBy(x => EF.Property<Guid>(x, "Id")) : query.OrderBy(x => EF.Property<string>(x, orderProperty)).ThenBy(x => EF.Property<Guid>(x, "Id"));
            if (typeof(T).GetProperty("SortOrder") != null) ordered=query.OrderBy(x=>EF.Property<int>(x,"SortOrder")).ThenBy(x=>EF.Property<Guid>(x,"Id"));
            var rows = await ordered.Skip((page - 1) * pageSize).Take(pageSize).AsSplitQuery().ToArrayAsync(ct);
            string ParentLabel(T x, bool en) => curriculumPlanId.HasValue && x is CurriculumGradeSubject subject
                ? $"{Name(subject.GradeLevel,en)} / {Name(subject.Subject,en)}"
                : educationProgramId.HasValue && x is GradeSubjectOffering offering
                    ? $"{Name(offering.GradeOffering.ProgramAcademicYear.AcademicYearDefinition,en)} / {Name(offering.CurriculumGradeSubject.GradeLevel,en)} / {Name(offering.CurriculumGradeSubject.Subject,en)}" : Parent(x,en);
            var parents = rows.ToDictionary(x => EFId(x), x => new { ar = ParentLabel(x, false), en = ParentLabel(x, true) });
            return Ok(ApiResponse<object>.Success(new { items = rows.Select(map).ToArray(), parents, totalCount = total, pageNumber = page, pageSize, totalPages = Math.Max(1, (int)Math.Ceiling(total / (double)pageSize)) }, correlationId: HttpContext.TraceIdentifier));
        }
        object Academic(object x) => SchoolAcademicSettingsController.MapForList(x);
        object Classes(object x) => SchoolClassStructureController.MapForList(x);
        object Curriculum(object x) => SchoolCurriculumController.MapForList(x);
        if (area == "facilities" && kind.StartsWith("tree-",StringComparison.Ordinal))
        {
            var rooms = db.SchoolRooms.IgnoreQueryFilters().Where(r=>deleted
                ? r.IsDeleted || r.Capabilities.Any(c=>c.Capability.IsDeleted) : !r.IsDeleted);
            var floors = db.BuildingFloors.IgnoreQueryFilters().Where(f=>deleted
                ? f.IsDeleted || rooms.Any(r=>r.FloorId==f.Id) : !f.IsDeleted);
            var buildings = db.SchoolBuildings.IgnoreQueryFilters().Where(b=>deleted
                ? b.IsDeleted || floors.Any(f=>f.BuildingId==b.Id) : !b.IsDeleted);
            return kind switch
            {
                "tree-branches" => await Read(db.SchoolBranches.Where(b=>deleted?b.IsDeleted||buildings.Any(x=>x.BranchId==b.Id):!b.IsDeleted),Facility),
                "tree-buildings" => await Read(buildings.Where(b=>parentId.HasValue&&b.BranchId==parentId.Value),Facility,"Branch"),
                "tree-floors" => await Read(floors.Where(f=>parentId.HasValue&&f.BuildingId==parentId.Value),Facility,"Building"),
                "tree-rooms" => await Read(rooms.Where(r=>parentId.HasValue&&r.FloorId==parentId.Value),Facility,"Floor.Building","Capabilities"),
                _ => NotFound()
            };
        }
        return (area, kind) switch
        {
            ("facilities", "branches") => await Read(db.SchoolBranches, Facility),
            ("facilities", "buildings") => await Read(db.SchoolBuildings, Facility, "Branch"),
            ("facilities", "floors") => await Read(db.BuildingFloors, Facility, "Building"),
            ("facilities", "room-types") => await Read(db.SchoolRoomTypes, Facility),
            ("facilities", "capabilities") => await Read(db.RoomCapabilities, Facility),
            ("facilities", "room-capabilities") => await Read(db.RoomCapabilities.Where(c=>parentId.HasValue&&db.SchoolRoomCapabilities.Any(link=>link.RoomId==parentId.Value&&link.CapabilityId==c.Id)),Facility),
            ("facilities", "rooms") => await Read(db.SchoolRooms, Facility, "Floor.Building", "Capabilities"),
            ("academic", "programs") => await Read(db.EducationPrograms, Academic),
            ("academic", "year-definitions") => await Read(db.AcademicYearDefinitions, Academic),
            ("academic", "program-years") => await Read(db.ProgramAcademicYears, Academic, "EducationProgram", "AcademicYearDefinition"),
            ("academic", "terms") => await Read(db.AcademicTerms.Where(x => !programAcademicYearId.HasValue || x.ProgramAcademicYearId == programAcademicYearId.Value), Academic, "ProgramAcademicYear"),
            ("academic", "periods") => await Read(db.AcademicPeriods.Where(x => !academicTermId.HasValue || x.AcademicTermId == academicTermId.Value), Academic, "AcademicTerm"),
            ("academic", "schedules") => await Read(db.SchoolDaySchedules, Academic, "EducationProgram"),
            ("academic", "calendar-events") => await Read(db.SchoolCalendarEvents, Academic, "EducationProgram"),
            ("classes", "stages") => await Read(db.EducationStages.Where(s=>!parentId.HasValue||s.EducationProgramId==parentId.Value), Classes, "EducationProgram"),
            ("classes", "tracks") => await Read(db.EducationTracks, Classes, "EducationStage"),
            ("classes", "grades") => await Read(db.GradeLevels.Where(x=>!parentId.HasValue||x.EducationStageId==parentId.Value), Classes, "EducationStage"),
            ("classes", "offerings") => await Read(db.GradeOfferings.Where(x=>!parentId.HasValue||x.ProgramAcademicYearId==parentId.Value), Classes, "ProgramAcademicYear", "GradeLevel"),
            ("classes", "sections") => await Read(db.ClassSections.Where(x=>!parentId.HasValue||x.GradeOfferingId==parentId.Value), Classes, "GradeOffering"),
            ("classes", "room-assignments") => await Read(db.ClassRoomAssignments.Where(x=>!parentId.HasValue||x.ClassSectionId==parentId.Value), Classes, "ClassSection", "Room"),
            ("curriculum", "subjects") => await Read(db.Subjects, Curriculum),
            ("curriculum", "plans") => await Read(db.CurriculumPlans, Curriculum, "EducationProgram"),
            ("curriculum", "grade-subjects") => await Read(db.CurriculumGradeSubjects.Where(x=>!curriculumPlanId.HasValue||x.CurriculumPlanId==curriculumPlanId.Value), Curriculum, "CurriculumPlan", "GradeLevel", "Subject"),
            ("curriculum", "books") => await Read(db.Books, Curriculum),
            ("curriculum", "book-versions") => await Read(db.BookVersions.Where(x=>!bookId.HasValue||x.BookId==bookId.Value), Curriculum, "Book"),
            ("curriculum", "book-roles") => await Read(db.BookRoles, Curriculum),
            ("curriculum", "subject-books") => await Read(db.CurriculumSubjectBooks, Curriculum, "BookVersion.Book", "BookRole"),
            ("curriculum", "offerings") => await Read(db.GradeSubjectOfferings.Where(o=>!educationProgramId.HasValue||o.GradeOffering.ProgramAcademicYear.EducationProgramId==educationProgramId.Value), Curriculum, "GradeOffering.ProgramAcademicYear.AcademicYearDefinition", "CurriculumGradeSubject.GradeLevel", "CurriculumGradeSubject.Subject"),
            _ => NotFound()
        };
    }
    private static Guid EFId(object x) => (Guid)x.GetType().GetProperty("Id")!.GetValue(x)!;
    private static object? Nav(object? x, string path) { foreach (var part in path.Split('.')) x = x?.GetType().GetProperty(part)?.GetValue(x); return x; }
    private static string Name(object? x, bool en) => Nav(x, en ? "NameEn" : "NameAr")?.ToString() ?? "";
    private static string GroupName(object x, bool en)
    {
        string Join(params string[] paths) => string.Join(" / ",paths.Select(p=>p.Length==0?Name(x,en):Name(Nav(x,p),en)).Where(s=>s.Length>0));
        return x switch
        {
            EducationStage => Join("","EducationProgram"),
            ProgramAcademicYear => Join("AcademicYearDefinition","EducationProgram"),
            GradeOffering => Join("GradeLevel","ProgramAcademicYear.AcademicYearDefinition","ProgramAcademicYear.EducationProgram"),
            ClassSection => Join("","GradeOffering.ProgramAcademicYear.AcademicYearDefinition","GradeOffering.ProgramAcademicYear.EducationProgram"),
            _ => Name(x,en)
        };
    }
    private static string Parent(object x, bool en)
    {
        string Join(params string[] paths) => string.Join(" / ", paths.Select(p => Name(Nav(x, p), en)).Where(s => s.Length > 0));
        return x switch
        {
            SchoolBuilding => Join("Branch"), BuildingFloor => Join("Building"), SchoolRoom => Join("Floor.Building", "Floor"),
            ProgramAcademicYear => Join("EducationProgram", "AcademicYearDefinition"), AcademicTerm => Join("ProgramAcademicYear"), AcademicPeriod => Join("AcademicTerm"),
            SchoolDaySchedule or SchoolCalendarEvent or EducationStage or CurriculumPlan => Join("EducationProgram"),
            EducationTrack or GradeLevel => Join("EducationStage"), GradeOffering => Join("ProgramAcademicYear", "GradeLevel"), ClassSection => Join("GradeOffering"), ClassRoomAssignment => Join("ClassSection", "Room"),
            CurriculumGradeSubject => Join("CurriculumPlan", "GradeLevel", "Subject"), BookVersion => Join("Book"), CurriculumSubjectBook => Join("BookVersion.Book", "BookVersion", "BookRole"),
            GradeSubjectOffering => Join("GradeOffering", "CurriculumGradeSubject.GradeLevel", "CurriculumGradeSubject.Subject"), _ => ""
        };
    }
    private static FacilityItemResponse Facility(object x) => x switch
    {
        SchoolBranch v => new(v.Id,null,null,v.Code,v.NameAr,v.NameEn,v.IsActive,v.IsDeleted,v.DeletedAtUtc,v.Address,null,null,null,[]),
        SchoolBuilding v => new(v.Id,v.BranchId,null,v.Code,v.NameAr,v.NameEn,v.IsActive,v.IsDeleted,v.DeletedAtUtc,null,null,null,null,[]),
        BuildingFloor v => new(v.Id,v.BuildingId,null,v.Code,v.NameAr,v.NameEn,v.IsActive,v.IsDeleted,v.DeletedAtUtc,null,v.SortOrder,null,null,[]),
        SchoolRoomType v => new(v.Id,null,null,v.Code,v.NameAr,v.NameEn,v.IsActive,v.IsDeleted,v.DeletedAtUtc,null,null,v.IsLaboratory,null,[],null,v.IsClassroom),
        RoomCapability v => new(v.Id,null,null,v.Code,v.NameAr,v.NameEn,v.IsActive,v.IsDeleted,v.DeletedAtUtc,null,null,null,null,[]),
        SchoolRoom v => new(v.Id,v.FloorId,v.RoomTypeId,v.Code,v.NameAr,v.NameEn,v.IsActive,v.IsDeleted,v.DeletedAtUtc,null,null,null,v.Capacity,v.Capabilities.Select(c => c.CapabilityId).ToArray(),v.IsSchedulable),
        _ => throw new ArgumentOutOfRangeException(nameof(x))
    };
}
