using Mdaresna.Api.Contracts;
using Mdaresna.Schools.Api.Auth;
using Mdaresna.Schools.Domain.Students;
using Mdaresna.Schools.Domain.Identity;
using Mdaresna.Schools.Infrastructure.Identity;
using Mdaresna.Schools.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Mdaresna.Schools.Api.Controllers;

[ApiController, Route("api/schools/v1/students")]
public sealed class SchoolStudentsController(ISchoolDbContextFactory dbFactory, IGlobalStudentGateway globalStudents) : ControllerBase
{
    [Authorize(Policy = SchoolPermissionPolicies.StudentsView), HttpGet]
    public async Task<IActionResult> List([FromQuery] Guid? programYearId, [FromQuery] Guid? programId,
        [FromQuery] Guid? stageId, [FromQuery] Guid? gradeId, [FromQuery] Guid? classSectionId,
        [FromQuery] string? name, [FromQuery] string? code, [FromQuery] DateOnly? dateOfBirth,
        [FromQuery] StudentEnrollmentStatus? status, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        if (!ValidPaging(pageNumber, pageSize)) return BadRequest(Failure("students.paging_invalid", "Invalid paging values."));
        await using var db = await RequireDb(ct); if (db is null) return Unauthorized();
        var query = db.StudentEnrollments.AsNoTracking().AsQueryable();
        if (programYearId.HasValue) query = query.Where(x => x.GradeOffering.ProgramAcademicYearId == programYearId);
        if (programId.HasValue) query = query.Where(x => x.GradeOffering.ProgramAcademicYear.EducationProgramId == programId);
        if (stageId.HasValue) query = query.Where(x => x.GradeOffering.GradeLevel.EducationStageId == stageId);
        if (gradeId.HasValue) query = query.Where(x => x.GradeOffering.GradeLevelId == gradeId);
        if (classSectionId.HasValue) query = query.Where(x => x.ClassSectionId == classSectionId);
        if (status.HasValue) query = query.Where(x => x.Status == status);
        if (dateOfBirth.HasValue) query = query.Where(x => x.Student.DateOfBirth == dateOfBirth);
        if (!string.IsNullOrWhiteSpace(name)) { var value = name.Trim().ToUpperInvariant(); query = query.Where(x => x.Student.NormalizedName.Contains(value)); }
        if (!string.IsNullOrWhiteSpace(code)) { var value = code.Trim().ToUpperInvariant(); query = query.Where(x => x.Student.StudentCode.Contains(value)); }
        var total = await query.CountAsync(ct);
        var rows = await query.OrderBy(x => x.Student.FullNameAr).ThenBy(x => x.Student.StudentCode)
            .Skip((pageNumber - 1) * pageSize).Take(pageSize)
            .Select(x => new StudentListResponse(x.StudentId, x.Student.StudentCode, x.Student.FullNameAr, x.Student.FullNameEn,
                x.Student.DateOfBirth, x.Student.Gender, x.Id, x.Status,
                x.GradeOffering.ProgramAcademicYear.NameAr, x.GradeOffering.ProgramAcademicYear.NameEn,
                x.GradeOffering.ProgramAcademicYear.EducationProgram.NameAr, x.GradeOffering.ProgramAcademicYear.EducationProgram.NameEn,
                x.GradeOffering.GradeLevel.EducationStage.NameAr, x.GradeOffering.GradeLevel.EducationStage.NameEn,
                x.GradeOffering.GradeLevel.NameAr, x.GradeOffering.GradeLevel.NameEn,
                x.ClassSection.NameAr, x.ClassSection.NameEn,
                x.Student.Guardians.Where(g => g.IsActive && g.IsPrimary).Select(g => g.Guardian.FullName).FirstOrDefault(),
                x.Student.Guardians.Where(g => g.IsActive && g.IsPrimary).Select(g => g.Guardian.Phone).FirstOrDefault()))
            .ToArrayAsync(ct);
        return Ok(PagedApiResponse<StudentListResponse>.Success(rows, total, pageNumber, pageSize, correlationId: HttpContext.TraceIdentifier));
    }

    [Authorize(Policy = SchoolPermissionPolicies.StudentsView), HttpGet("{id:guid}")]
    public async Task<IActionResult> Details(Guid id, CancellationToken ct)
    {
        await using var db = await RequireDb(ct); if (db is null) return Unauthorized();
        var row = await db.Students.AsNoTracking().Where(x => x.Id == id).Select(x => new StudentDetailsResponse(x.Id,
            x.GlobalStudentId, x.StudentCode, x.FullNameAr, x.FullNameEn, x.DateOfBirth, x.Gender, x.NationalId,
            x.BirthCertificateNumber, x.IsActive,
            x.Guardians.Where(g => g.IsActive).OrderByDescending(g => g.IsPrimary).Select(g => new GuardianResponse(g.GuardianId,
                g.Guardian.FullName, g.Guardian.Phone, g.Guardian.Email, g.Guardian.NationalId, g.Relationship, g.IsPrimary, g.CanPickup,
                g.IsFinancialResponsible, g.IsEmergencyContact)).ToArray(),
            x.Enrollments.OrderByDescending(e => e.CreatedAtUtc).Select(e => new EnrollmentResponse(e.Id, e.Status,
                e.EnrollmentDate, e.GradeOffering.ProgramAcademicYear.NameAr, e.GradeOffering.ProgramAcademicYear.NameEn,
                e.GradeOffering.ProgramAcademicYear.EducationProgram.NameAr, e.GradeOffering.ProgramAcademicYear.EducationProgram.NameEn,
                e.GradeOffering.GradeLevel.EducationStage.NameAr, e.GradeOffering.GradeLevel.EducationStage.NameEn,
                e.GradeOffering.GradeLevel.NameAr, e.GradeOffering.GradeLevel.NameEn, e.ClassSection.NameAr, e.ClassSection.NameEn)).ToArray()))
            .SingleOrDefaultAsync(ct);
        return row is null ? NotFound(Failure("students.not_found", "Student was not found.")) : Ok(ApiResponse<StudentDetailsResponse>.Success(row, correlationId: HttpContext.TraceIdentifier));
    }

    [Authorize(Policy = SchoolPermissionPolicies.StudentsManage), HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateDirectStudentRequest request, CancellationToken ct)
    {
        if (!ValidPerson(request.FullNameAr, request.FullNameEn, request.DateOfBirth) || !ValidGuardians(request.Guardians))
            return BadRequest(Failure("students.invalid", "Enter valid student and guardian data."));
        await using var db = await RequireDb(ct); if (db is null) return Unauthorized();
        var section = await db.ClassSections.AsNoTracking().Where(x => x.Id == request.ClassSectionId && x.IsActive)
            .Select(x => new { x.Id, x.GradeOfferingId, x.Capacity }).SingleOrDefaultAsync(ct);
        if (section is null) return BadRequest(Failure("students.class_invalid", "Select an active class section."));
        var occupied = await db.StudentEnrollments.CountAsync(x => x.ClassSectionId == section.Id && x.Status == StudentEnrollmentStatus.Active, ct);
        if (occupied >= section.Capacity) return Conflict(Failure("students.class_full", "The class section has reached its capacity."));
        var schoolId = await db.SchoolInformation.AsNoTracking().Select(x => x.PlatformSchoolReferenceId).SingleAsync(ct);
        var global = await globalStudents.ResolveOrCreateAsync(schoolId, request.StudentCode, request.FullNameAr,
            request.DateOfBirth, request.NationalId, request.BirthCertificateNumber, ct);
        if (global is null) return Conflict(Failure("students.global_unavailable", "Unable to resolve the global student identity."));
        if (await db.Students.AnyAsync(x => x.GlobalStudentId == global.GlobalStudentId || x.StudentCode == global.StudentCode, ct))
            return Conflict(Failure("students.duplicate", "This student is already registered in the school."));
        var now = DateTimeOffset.UtcNow;
        var student = NewStudent(global, request.FullNameAr, request.FullNameEn, request.DateOfBirth, request.Gender,
            request.NationalId, request.BirthCertificateNumber, now);
        AddGuardians(db, student, null, request.Guardians, now);
        student.Enrollments.Add(new StudentEnrollment { Id = Guid.NewGuid(), GradeOfferingId = section.GradeOfferingId,
            ClassSectionId = section.Id, EnrollmentDate = request.EnrollmentDate, Status = StudentEnrollmentStatus.Active,
            CreatedAtUtc = now, UpdatedAtUtc = now });
        db.Students.Add(student); await db.SaveChangesAsync(ct);
        return Ok(ApiResponse<CreateStudentResponse>.Success(new(student.Id, student.GlobalStudentId, student.StudentCode), correlationId: HttpContext.TraceIdentifier));
    }

    [Authorize(Policy = SchoolPermissionPolicies.AdmissionsView), HttpGet("applications")]
    public async Task<IActionResult> Applications([FromQuery] Guid? programYearId, [FromQuery] Guid? programId,
        [FromQuery] Guid? stageId, [FromQuery] Guid? gradeId, [FromQuery] string? name, [FromQuery] string? code,
        [FromQuery] DateOnly? dateOfBirth, [FromQuery] AdmissionApplicationStatus? status,
        [FromQuery] AdmissionApplicationSource? source, [FromQuery] DateOnly? submittedFrom,
        [FromQuery] DateOnly? submittedTo, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        if (!ValidPaging(pageNumber, pageSize)) return BadRequest(Failure("admissions.paging_invalid", "Invalid paging values."));
        await using var db = await RequireDb(ct); if (db is null) return Unauthorized();
        var query = db.AdmissionApplications.AsNoTracking().AsQueryable();
        if (programYearId.HasValue) query = query.Where(x => x.ProgramAcademicYearId == programYearId);
        if (programId.HasValue) query = query.Where(x => x.ProgramAcademicYear.EducationProgramId == programId);
        if (stageId.HasValue) query = query.Where(x => x.GradeLevel.EducationStageId == stageId);
        if (gradeId.HasValue) query = query.Where(x => x.GradeLevelId == gradeId);
        if (status.HasValue) query = query.Where(x => x.Status == status);
        if (source.HasValue) query = query.Where(x => x.Source == source);
        if (dateOfBirth.HasValue) query = query.Where(x => x.DateOfBirth == dateOfBirth);
        if (submittedFrom.HasValue) { var from = new DateTimeOffset(submittedFrom.Value.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero); query = query.Where(x => x.SubmittedAtUtc >= from); }
        if (submittedTo.HasValue) { var to = new DateTimeOffset(submittedTo.Value.AddDays(1).ToDateTime(TimeOnly.MinValue), TimeSpan.Zero); query = query.Where(x => x.SubmittedAtUtc < to); }
        if (!string.IsNullOrWhiteSpace(name)) { var value = name.Trim().ToUpperInvariant(); query = query.Where(x => x.NormalizedName.Contains(value)); }
        if (!string.IsNullOrWhiteSpace(code)) { var value = code.Trim().ToUpperInvariant(); query = query.Where(x => x.StudentCode != null && x.StudentCode.Contains(value)); }
        var total = await query.CountAsync(ct);
        var rows = await query.OrderByDescending(x => x.SubmittedAtUtc).Skip((pageNumber - 1) * pageSize).Take(pageSize)
            .Select(x => new AdmissionListResponse(x.Id, x.ApplicationNumber, x.StudentCode, x.FullNameAr, x.FullNameEn,
                x.DateOfBirth, x.Gender, x.Status, x.Source, x.SubmittedAtUtc,
                x.ProgramAcademicYearId, x.GradeLevelId,
                x.ProgramAcademicYear.NameAr, x.ProgramAcademicYear.NameEn,
                x.ProgramAcademicYear.EducationProgram.NameAr, x.ProgramAcademicYear.EducationProgram.NameEn,
                x.GradeLevel.EducationStage.NameAr, x.GradeLevel.EducationStage.NameEn, x.GradeLevel.NameAr, x.GradeLevel.NameEn,
                x.Guardians.OrderByDescending(g => g.IsPrimary).Select(g => g.Guardian.FullName).FirstOrDefault(),
                x.Guardians.OrderByDescending(g => g.IsPrimary).Select(g => g.Guardian.Phone).FirstOrDefault(), x.Notes))
            .ToArrayAsync(ct);
        return Ok(PagedApiResponse<AdmissionListResponse>.Success(rows, total, pageNumber, pageSize, correlationId: HttpContext.TraceIdentifier));
    }

    [Authorize(Policy = SchoolPermissionPolicies.AdmissionsManage), HttpPost("applications")]
    public async Task<IActionResult> CreateApplication([FromBody] CreateAdmissionApplicationRequest request, CancellationToken ct)
    {
        if (!ValidPerson(request.FullNameAr, request.FullNameEn, request.DateOfBirth) || !ValidGuardians(request.Guardians))
            return BadRequest(Failure("admissions.invalid", "Enter valid application and guardian data."));
        await using var db = await RequireDb(ct); if (db is null) return Unauthorized();
        var targetValid = await db.GradeLevels.AnyAsync(x => x.Id == request.GradeLevelId && x.IsActive &&
            x.EducationStage.EducationProgramId == db.ProgramAcademicYears.Where(y => y.Id == request.ProgramAcademicYearId && y.IsActive).Select(y => y.EducationProgramId).FirstOrDefault(), ct);
        if (!targetValid) return BadRequest(Failure("admissions.target_invalid", "Select a valid academic year and grade."));
        GlobalStudentIdentity? global = null;
        if (!string.IsNullOrWhiteSpace(request.StudentCode))
        {
            var schoolId = await db.SchoolInformation.AsNoTracking().Select(x => x.PlatformSchoolReferenceId).SingleAsync(ct);
            global = await globalStudents.ResolveOrCreateAsync(schoolId, request.StudentCode, request.FullNameAr,
                request.DateOfBirth, request.NationalId, request.BirthCertificateNumber, ct);
            if (global is null) return Conflict(Failure("students.identity_mismatch", "Unable to verify the supplied student code."));
        }
        var now = DateTimeOffset.UtcNow; var app = new AdmissionApplication { Id = Guid.NewGuid(),
            ApplicationNumber = $"APP-{now:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..6].ToUpperInvariant()}",
            GlobalStudentId = global?.GlobalStudentId, StudentCode = global?.StudentCode,
            FullNameAr = request.FullNameAr.Trim(), FullNameEn = request.FullNameEn.Trim(), NormalizedName = Normalize(request.FullNameAr, request.FullNameEn),
            DateOfBirth = request.DateOfBirth, Gender = request.Gender, NationalId = Clean(request.NationalId),
            BirthCertificateNumber = Clean(request.BirthCertificateNumber), ProgramAcademicYearId = request.ProgramAcademicYearId,
            GradeLevelId = request.GradeLevelId, Source = AdmissionApplicationSource.SchoolDesk,
            Status = AdmissionApplicationStatus.Submitted, SubmittedAtUtc = now, Notes = Clean(request.Notes), CreatedAtUtc = now, UpdatedAtUtc = now };
        AddGuardians(db, null, app, request.Guardians, now); db.AdmissionApplications.Add(app); await db.SaveChangesAsync(ct);
        return Ok(ApiResponse<CreateApplicationResponse>.Success(new(app.Id, app.ApplicationNumber), correlationId: HttpContext.TraceIdentifier));
    }

    [Authorize(Policy = SchoolPermissionPolicies.AdmissionsManage), HttpPut("applications/{id:guid}/status")]
    public async Task<IActionResult> ChangeApplicationStatus(Guid id, [FromBody] ChangeApplicationStatusRequest request, CancellationToken ct)
    {
        if (request.Status == AdmissionApplicationStatus.Accepted) return BadRequest(Failure("admissions.use_accept", "Use the accept action to enroll the student."));
        await using var db = await RequireDb(ct); if (db is null) return Unauthorized();
        var app = await db.AdmissionApplications.SingleOrDefaultAsync(x => x.Id == id, ct);
        if (app is null) return NotFound(Failure("admissions.not_found", "Application was not found."));
        if (app.Status == AdmissionApplicationStatus.Accepted) return Conflict(Failure("admissions.final", "An accepted application cannot be changed."));
        app.Status = request.Status; app.Notes = Clean(request.Notes) ?? app.Notes; app.UpdatedAtUtc = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(ct); return Ok(ApiResponse<object?>.Success(null, correlationId: HttpContext.TraceIdentifier));
    }

    [Authorize(Policy = SchoolPermissionPolicies.AdmissionsManage), HttpPost("applications/{id:guid}/accept")]
    public async Task<IActionResult> Accept(Guid id, [FromBody] AcceptAdmissionApplicationRequest request, CancellationToken ct)
    {
        await using var db = await RequireDb(ct); if (db is null) return Unauthorized();
        var app = await db.AdmissionApplications.Include(x => x.Guardians).ThenInclude(x => x.Guardian).SingleOrDefaultAsync(x => x.Id == id, ct);
        if (app is null) return NotFound(Failure("admissions.not_found", "Application was not found."));
        if (app.Status == AdmissionApplicationStatus.Accepted) return Conflict(Failure("admissions.already_accepted", "Application was already accepted."));
        var section = await db.ClassSections.AsNoTracking().Where(x => x.Id == request.ClassSectionId && x.IsActive &&
            x.GradeOffering.ProgramAcademicYearId == app.ProgramAcademicYearId && x.GradeOffering.GradeLevelId == app.GradeLevelId)
            .Select(x => new { x.Id, x.GradeOfferingId, x.Capacity }).SingleOrDefaultAsync(ct);
        if (section is null) return BadRequest(Failure("admissions.class_invalid", "Select a class in the requested grade."));
        if (await db.StudentEnrollments.CountAsync(x => x.ClassSectionId == section.Id && x.Status == StudentEnrollmentStatus.Active, ct) >= section.Capacity)
            return Conflict(Failure("students.class_full", "The class section has reached its capacity."));
        GlobalStudentIdentity? global = app.GlobalStudentId.HasValue ? new(app.GlobalStudentId.Value, app.StudentCode!) : null;
        if (global is null)
        {
            var schoolId = await db.SchoolInformation.AsNoTracking().Select(x => x.PlatformSchoolReferenceId).SingleAsync(ct);
            global = await globalStudents.ResolveOrCreateAsync(schoolId, null, app.FullNameAr, app.DateOfBirth, app.NationalId, app.BirthCertificateNumber, ct);
        }
        if (global is null) return Conflict(Failure("students.global_unavailable", "Unable to allocate the global student code."));
        var existing = await db.Students.SingleOrDefaultAsync(x => x.GlobalStudentId == global.GlobalStudentId, ct);
        if (existing is not null && await db.StudentEnrollments.AnyAsync(x => x.StudentId == existing.Id && x.GradeOfferingId == section.GradeOfferingId, ct))
            return Conflict(Failure("students.already_enrolled", "This student already has an enrollment in the requested grade."));
        var now = DateTimeOffset.UtcNow;
        var student = existing ?? NewStudent(global, app.FullNameAr, app.FullNameEn, app.DateOfBirth, app.Gender, app.NationalId, app.BirthCertificateNumber, now);
        if (existing is null)
            foreach (var link in app.Guardians) student.Guardians.Add(new StudentGuardian { Id = Guid.NewGuid(), GuardianId = link.GuardianId,
                Relationship = link.Relationship, IsPrimary = link.IsPrimary, CanPickup = link.CanPickup,
                IsFinancialResponsible = link.IsFinancialResponsible, IsEmergencyContact = link.IsEmergencyContact,
                CreatedAtUtc = now, UpdatedAtUtc = now });
        student.Enrollments.Add(new StudentEnrollment { Id = Guid.NewGuid(), GradeOfferingId = section.GradeOfferingId,
            ClassSectionId = section.Id, EnrollmentDate = request.EnrollmentDate, Status = StudentEnrollmentStatus.Active,
            CreatedAtUtc = now, UpdatedAtUtc = now });
        if (existing is null) db.Students.Add(student);
        app.GlobalStudentId = global.GlobalStudentId; app.StudentCode = global.StudentCode;
        app.AcceptedStudent = student; app.Status = AdmissionApplicationStatus.Accepted; app.UpdatedAtUtc = now;
        await db.SaveChangesAsync(ct);
        return Ok(ApiResponse<CreateStudentResponse>.Success(new(student.Id, student.GlobalStudentId, student.StudentCode), correlationId: HttpContext.TraceIdentifier));
    }

    [Authorize, HttpGet("options")]
    public async Task<IActionResult> Options(CancellationToken ct)
    {
        if (!User.HasClaim(SchoolClaimTypes.Permission, "school.students.view") &&
            !User.HasClaim(SchoolClaimTypes.Permission, "school.admissions.view")) return Forbid();
        await using var db = await RequireDb(ct); if (db is null) return Unauthorized();
        var rows = await db.ClassSections.AsNoTracking().Where(x => x.IsActive && x.GradeOffering.IsActive && x.GradeOffering.ProgramAcademicYear.IsActive)
            .OrderByDescending(x => x.GradeOffering.ProgramAcademicYear.StartDate)
            .ThenBy(x => x.GradeOffering.GradeLevel.SortOrder).ThenBy(x => x.NameAr)
            .Select(x => new StudentAcademicOptionResponse(x.GradeOffering.ProgramAcademicYearId,
                x.GradeOffering.ProgramAcademicYear.NameAr, x.GradeOffering.ProgramAcademicYear.NameEn,
                x.GradeOffering.ProgramAcademicYear.EducationProgramId, x.GradeOffering.ProgramAcademicYear.EducationProgram.NameAr,
                x.GradeOffering.ProgramAcademicYear.EducationProgram.NameEn, x.GradeOffering.GradeLevel.EducationStageId,
                x.GradeOffering.GradeLevel.EducationStage.NameAr, x.GradeOffering.GradeLevel.EducationStage.NameEn,
                x.GradeOffering.GradeLevelId, x.GradeOffering.GradeLevel.NameAr, x.GradeOffering.GradeLevel.NameEn,
                x.Id, x.NameAr, x.NameEn, x.Capacity,
                db.StudentEnrollments.Count(e => e.ClassSectionId == x.Id && e.Status == StudentEnrollmentStatus.Active))).ToArrayAsync(ct);
        return Ok(ApiResponse<IReadOnlyList<StudentAcademicOptionResponse>>.Success(rows, correlationId: HttpContext.TraceIdentifier));
    }

    private static Student NewStudent(GlobalStudentIdentity global, string ar, string en, DateOnly dob, StudentGender gender,
        string? nationalId, string? birthCertificate, DateTimeOffset now) => new() { Id = Guid.NewGuid(), PersonId = Guid.NewGuid(), GlobalStudentId = global.GlobalStudentId,
        StudentCode = global.StudentCode, FullNameAr = ar.Trim(), FullNameEn = en.Trim(), NormalizedName = Normalize(ar, en),
        DateOfBirth = dob, Gender = gender, NationalId = Clean(nationalId), BirthCertificateNumber = Clean(birthCertificate),
        CreatedAtUtc = now, UpdatedAtUtc = now, Person = new Person { DisplayName = ar.Trim(), DateOfBirth = dob,
            GenderCode = gender.ToString(), Status = PersonStatus.Active, CreatedAtUtc = now, UpdatedAtUtc = now } };
    private static void AddGuardians(SchoolsDbContext db, Student? student, AdmissionApplication? app,
        IReadOnlyList<GuardianInput> inputs, DateTimeOffset now)
    {
        foreach (var input in inputs)
        {
            var guardian = new Guardian { Id = Guid.NewGuid(), FullName = input.FullName.Trim(), Phone = input.Phone.Trim(),
                Email = Clean(input.Email), NationalId = Clean(input.NationalId), CreatedAtUtc = now, UpdatedAtUtc = now };
            db.Guardians.Add(guardian);
            if (student is not null) student.Guardians.Add(new StudentGuardian { Id = Guid.NewGuid(), Guardian = guardian,
                Relationship = input.Relationship, IsPrimary = input.IsPrimary, CanPickup = input.CanPickup,
                IsFinancialResponsible = input.IsFinancialResponsible, IsEmergencyContact = input.IsEmergencyContact,
                CreatedAtUtc = now, UpdatedAtUtc = now });
            else app!.Guardians.Add(new AdmissionApplicationGuardian { Id = Guid.NewGuid(), Guardian = guardian,
                Relationship = input.Relationship, IsPrimary = input.IsPrimary, CanPickup = input.CanPickup,
                IsFinancialResponsible = input.IsFinancialResponsible, IsEmergencyContact = input.IsEmergencyContact,
                CreatedAtUtc = now });
        }
    }
    private static bool ValidGuardians(IReadOnlyList<GuardianInput>? rows) => rows is { Count: > 0 } && rows.Count(x => x.IsPrimary) == 1 &&
        rows.All(x => !string.IsNullOrWhiteSpace(x.FullName) && x.FullName.Trim().Length <= 200 && x.Phone.Trim().Length is >= 8 and <= 20);
    private static bool ValidPerson(string ar, string en, DateOnly dob) => !string.IsNullOrWhiteSpace(ar) && ar.Trim().Length <= 200 &&
        !string.IsNullOrWhiteSpace(en) && en.Trim().Length <= 200 && dob <= DateOnly.FromDateTime(DateTime.UtcNow);
    private static bool ValidPaging(int page, int size) => page > 0 && size is > 0 and <= 100;
    private static string Normalize(string ar, string en) => $"{ar.Trim()} {en.Trim()}".ToUpperInvariant();
    private static string? Clean(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    private async Task<SchoolsDbContext?> RequireDb(CancellationToken ct) => await dbFactory.CreateAsync(User.FindFirst(SchoolClaimTypes.SchoolCode)?.Value ?? string.Empty, ct);
    private ApiResponse<object?> Failure(string code, string message) => ApiResponse<object?>.Failure(400, code, message, correlationId: HttpContext.TraceIdentifier);
}

public sealed record GuardianInput(string FullName, string Phone, string? Email, string? NationalId,
    GuardianRelationship Relationship, bool IsPrimary, bool CanPickup, bool IsFinancialResponsible, bool IsEmergencyContact);
public sealed record CreateDirectStudentRequest(string? StudentCode, string FullNameAr, string FullNameEn, DateOnly DateOfBirth,
    StudentGender Gender, string? NationalId, string? BirthCertificateNumber, Guid ClassSectionId, DateOnly EnrollmentDate,
    IReadOnlyList<GuardianInput> Guardians);
public sealed record CreateAdmissionApplicationRequest(string? StudentCode, string FullNameAr, string FullNameEn,
    DateOnly DateOfBirth, StudentGender Gender, string? NationalId, string? BirthCertificateNumber,
    Guid ProgramAcademicYearId, Guid GradeLevelId, string? Notes, IReadOnlyList<GuardianInput> Guardians);
public sealed record ChangeApplicationStatusRequest(AdmissionApplicationStatus Status, string? Notes);
public sealed record AcceptAdmissionApplicationRequest(Guid ClassSectionId, DateOnly EnrollmentDate);
public sealed record CreateStudentResponse(Guid StudentId, Guid GlobalStudentId, string StudentCode);
public sealed record CreateApplicationResponse(Guid ApplicationId, string ApplicationNumber);
public sealed record StudentListResponse(Guid Id, string StudentCode, string FullNameAr, string FullNameEn, DateOnly DateOfBirth,
    StudentGender Gender, Guid EnrollmentId, StudentEnrollmentStatus EnrollmentStatus, string AcademicYearNameAr,
    string AcademicYearNameEn, string ProgramNameAr, string ProgramNameEn, string StageNameAr, string StageNameEn,
    string GradeNameAr, string GradeNameEn, string ClassNameAr, string ClassNameEn, string? PrimaryGuardianName, string? PrimaryGuardianPhone);
public sealed record GuardianResponse(Guid Id, string FullName, string Phone, string? Email, string? NationalId, GuardianRelationship Relationship,
    bool IsPrimary, bool CanPickup, bool IsFinancialResponsible, bool IsEmergencyContact);
public sealed record EnrollmentResponse(Guid Id, StudentEnrollmentStatus Status, DateOnly EnrollmentDate, string AcademicYearNameAr,
    string AcademicYearNameEn, string ProgramNameAr, string ProgramNameEn, string StageNameAr, string StageNameEn,
    string GradeNameAr, string GradeNameEn, string ClassNameAr, string ClassNameEn);
public sealed record StudentDetailsResponse(Guid Id, Guid GlobalStudentId, string StudentCode, string FullNameAr, string FullNameEn,
    DateOnly DateOfBirth, StudentGender Gender, string? NationalId, string? BirthCertificateNumber, bool IsActive,
    IReadOnlyList<GuardianResponse> Guardians, IReadOnlyList<EnrollmentResponse> Enrollments);
public sealed record AdmissionListResponse(Guid Id, string ApplicationNumber, string? StudentCode, string FullNameAr,
    string FullNameEn, DateOnly DateOfBirth, StudentGender Gender, AdmissionApplicationStatus Status,
    AdmissionApplicationSource Source, DateTimeOffset SubmittedAtUtc, Guid ProgramAcademicYearId, Guid GradeLevelId,
    string AcademicYearNameAr, string AcademicYearNameEn,
    string ProgramNameAr, string ProgramNameEn, string StageNameAr, string StageNameEn, string GradeNameAr, string GradeNameEn,
    string? PrimaryGuardianName, string? PrimaryGuardianPhone, string? Notes);
public sealed record StudentAcademicOptionResponse(Guid ProgramAcademicYearId, string AcademicYearNameAr, string AcademicYearNameEn,
    Guid ProgramId, string ProgramNameAr, string ProgramNameEn, Guid StageId, string StageNameAr, string StageNameEn,
    Guid GradeId, string GradeNameAr, string GradeNameEn, Guid ClassSectionId, string ClassNameAr, string ClassNameEn,
    int Capacity, int EnrolledCount);
