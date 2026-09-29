using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Mdaresna.Api.Contracts;
using Mdaresna.Schools.Api.Auth;
using Mdaresna.Schools.Domain.Academics;
using Mdaresna.Schools.Domain.Identity;
using Mdaresna.Schools.Domain.Students;
using Mdaresna.Schools.Infrastructure.Identity;
using Mdaresna.Schools.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Mdaresna.Schools.Api.Controllers;

[ApiController, Authorize, Route("api/schools/v1")]
public sealed class SchoolHomeworkController(ISchoolDbContextFactory dbFactory) : ControllerBase
{
    [HttpGet("class-workspace/{classSectionId:guid}/homework/subjects"), Authorize(Policy = SchoolPermissionPolicies.HomeworkView)]
    public async Task<IActionResult> Subjects(Guid classSectionId, CancellationToken ct)
    {
        await using var db = await RequireDb(ct); if (db is null) return Unauthorized();
        if (!await CanAccessClass(db, classSectionId, ct)) return Forbid();
        var admin = await IsAdmin(db, ct); var userId = CurrentUserId();
        var query = db.ClassSectionSubjects.AsNoTracking().Where(x => x.ClassSectionId == classSectionId && x.IsActive && !x.IsDeleted && x.GradeSubjectOffering.IsActive);
        if (!admin) query = query.Where(x => db.ClassSectionTeacherScopes.Any(scope => scope.ClassSectionId == classSectionId && scope.IsActive && !scope.IsDeleted &&
            scope.TeacherGradeSubjectScope.IsActive && !scope.TeacherGradeSubjectScope.IsDeleted && scope.TeacherGradeSubjectScope.TeacherUserId == userId &&
            scope.TeacherGradeSubjectScope.GradeSubjectOfferingId == x.GradeSubjectOfferingId));
        var subjects = await query.OrderBy(x => x.GradeSubjectOffering.CurriculumGradeSubject.Subject.NameAr).Select(x => new HomeworkSubjectResponse(
            x.Id, x.GradeSubjectOffering.CurriculumGradeSubject.Subject.NameAr, x.GradeSubjectOffering.CurriculumGradeSubject.Subject.NameEn,
            x.GradeSubjectOffering.CurriculumGradeSubject.Books.Where(book => book.IsActive && !book.IsDeleted && book.BookVersion.IsActive && !book.BookVersion.IsDeleted)
                .OrderBy(book => book.SortOrder).Select(book => new HomeworkBookResponse(book.Id, book.BookVersion.Book.NameAr, book.BookVersion.Book.NameEn,
                    book.BookVersion.VersionLabel, book.BookRole.NameAr, book.BookRole.NameEn)).ToArray())).ToArrayAsync(ct);
        return Ok(ApiResponse<IReadOnlyList<HomeworkSubjectResponse>>.Success(subjects, correlationId: HttpContext.TraceIdentifier));
    }

    [HttpGet("class-workspace/{classSectionId:guid}/homework"), Authorize(Policy = SchoolPermissionPolicies.HomeworkView)]
    public async Task<IActionResult> List(Guid classSectionId, [FromQuery] Guid? classSectionSubjectId, CancellationToken ct)
    {
        await using var db = await RequireDb(ct); if (db is null) return Unauthorized();
        if (!await CanAccessClass(db, classSectionId, ct)) return Forbid();
        if (classSectionSubjectId.HasValue && !await CanAccessSubject(db, classSectionId, classSectionSubjectId.Value, ct)) return Forbid();
        var query = db.HomeworkAssignments.AsNoTracking().Where(x => x.ClassSectionSubject.ClassSectionId == classSectionId);
        if (classSectionSubjectId.HasValue) query = query.Where(x => x.ClassSectionSubjectId == classSectionSubjectId);
        var rows = await query.OrderByDescending(x => x.CreatedAtUtc).Select(x => new HomeworkListItemResponse(x.Id, x.ClassSectionSubjectId,
            x.ClassSectionSubject.GradeSubjectOffering.CurriculumGradeSubject.Subject.NameAr,
            x.ClassSectionSubject.GradeSubjectOffering.CurriculumGradeSubject.Subject.NameEn, x.Title, x.DeliveryMode, x.Status,
            x.DueAtUtc, x.TotalScore, x.Students.Count, x.Students.Count(s => s.Status == StudentHomeworkStatus.Submitted || s.Status == StudentHomeworkStatus.Late ||
                s.Status == StudentHomeworkStatus.PendingManualReview || s.Status == StudentHomeworkStatus.AutoGraded || s.Status == StudentHomeworkStatus.Graded || s.Status == StudentHomeworkStatus.Returned),
            x.Students.Count(s => s.Status == StudentHomeworkStatus.PendingManualReview), x.CreatedByUser.Person.DisplayName, x.CreatedAtUtc)).ToArrayAsync(ct);
        return Ok(ApiResponse<IReadOnlyList<HomeworkListItemResponse>>.Success(rows, correlationId: HttpContext.TraceIdentifier));
    }

    [HttpGet("homework/{id:guid}"), Authorize(Policy = SchoolPermissionPolicies.HomeworkView)]
    public async Task<IActionResult> Get(Guid id, CancellationToken ct)
    {
        await using var db = await RequireDb(ct); if (db is null) return Unauthorized();
        var entity = await Load(db, id, ct); if (entity is null) return NotFound(Failure(404, "homework.not_found", "Homework was not found."));
        if (!await CanAccessSubject(db, entity.ClassSectionSubject.ClassSectionId, entity.ClassSectionSubjectId, ct)) return Forbid();
        return Ok(ApiResponse<HomeworkDetailsResponse>.Success(ToDetails(entity), correlationId: HttpContext.TraceIdentifier));
    }

    [HttpPost("class-workspace/{classSectionId:guid}/homework"), Authorize(Policy = SchoolPermissionPolicies.HomeworkManage)]
    public async Task<IActionResult> Create(Guid classSectionId, [FromBody] SaveHomeworkRequest request, CancellationToken ct)
    {
        await using var db = await RequireDb(ct); if (db is null) return Unauthorized();
        if (!await CanAccessSubject(db, classSectionId, request.ClassSectionSubjectId, ct)) return Forbid();
        var validation = await Validate(db, request, ct); if (validation is not null) return validation;
        var school = await db.SchoolInformation.AsNoTracking().SingleOrDefaultAsync(ct);
        if (school is null || string.IsNullOrWhiteSpace(school.TimeZoneId)) return Conflict(Failure(409, "homework.time_zone_required", "Configure the school time zone first."));
        var now = DateTimeOffset.UtcNow;
        var entity = new HomeworkAssignment { Id = Guid.NewGuid(), ClassSectionSubjectId = request.ClassSectionSubjectId,
            CreatedByUserId = CurrentUserId(), CreatedAtUtc = now, UpdatedAtUtc = now, TimeZoneIdSnapshot = school.TimeZoneId };
        Apply(entity, request, now); db.HomeworkAssignments.Add(entity); AddAudit(db, entity, "Created", now); await db.SaveChangesAsync(ct);
        return Ok(ApiResponse<object?>.Success(new { entity.Id }, correlationId: HttpContext.TraceIdentifier));
    }

    [HttpPut("homework/{id:guid}"), Authorize(Policy = SchoolPermissionPolicies.HomeworkManage)]
    public async Task<IActionResult> Update(Guid id, [FromBody] SaveHomeworkRequest request, CancellationToken ct)
    {
        await using var db = await RequireDb(ct); if (db is null) return Unauthorized();
        var entity = await Load(db, id, ct); if (entity is null) return NotFound(Failure(404, "homework.not_found", "Homework was not found."));
        if (entity.Status != HomeworkStatus.Draft) return Conflict(Failure(409, "homework.not_draft", "Only draft homework can be edited."));
        if (entity.ClassSectionSubjectId != request.ClassSectionSubjectId || !await CanAccessSubject(db, entity.ClassSectionSubject.ClassSectionId, request.ClassSectionSubjectId, ct)) return Forbid();
        var validation = await Validate(db, request, ct); if (validation is not null) return validation;
        var now = DateTimeOffset.UtcNow; db.HomeworkQuestions.RemoveRange(entity.Questions); entity.Questions.Clear(); Apply(entity, request, now); AddAudit(db, entity, "Updated", now);
        await db.SaveChangesAsync(ct); return Ok(ApiResponse<object?>.Success(null, correlationId: HttpContext.TraceIdentifier));
    }

    [HttpPost("homework/{id:guid}/publish"), Authorize(Policy = SchoolPermissionPolicies.HomeworkPublish)]
    public async Task<IActionResult> Publish(Guid id, CancellationToken ct)
    {
        await using var db = await RequireDb(ct); if (db is null) return Unauthorized();
        var entity = await Load(db, id, ct); if (entity is null) return NotFound(Failure(404, "homework.not_found", "Homework was not found."));
        if (!await CanAccessSubject(db, entity.ClassSectionSubject.ClassSectionId, entity.ClassSectionSubjectId, ct)) return Forbid();
        if (entity.Status != HomeworkStatus.Draft) return Conflict(Failure(409, "homework.not_draft", "Only draft homework can be published."));
        if (entity.DeliveryMode == HomeworkDeliveryMode.Online && entity.Questions.Count == 0) return BadRequest(Failure(400, "homework.questions_required", "Online homework requires at least one question."));
        var now = DateTimeOffset.UtcNow; var enrollmentIds = await db.StudentEnrollments.AsNoTracking().Where(x => x.ClassSectionId == entity.ClassSectionSubject.ClassSectionId && x.Status == StudentEnrollmentStatus.Active && x.Student.IsActive).Select(x => x.Id).ToArrayAsync(ct);
        entity.Status = HomeworkStatus.Published; entity.PublishedAtUtc = now; entity.PublishedByUserId = CurrentUserId(); entity.UpdatedAtUtc = now;
        db.StudentHomework.AddRange(enrollmentIds.Select(enrollmentId => new StudentHomework { Id = Guid.NewGuid(), HomeworkAssignmentId = entity.Id,
            StudentEnrollmentId = enrollmentId, CreatedAtUtc = now, UpdatedAtUtc = now }));
        AddAudit(db, entity, "Published", now, $"{{\"studentCount\":{enrollmentIds.Length}}}");
        await db.SaveChangesAsync(ct); return Ok(ApiResponse<object?>.Success(new { StudentCount = enrollmentIds.Length }, correlationId: HttpContext.TraceIdentifier));
    }

    [HttpPost("homework/{id:guid}/close"), Authorize(Policy = SchoolPermissionPolicies.HomeworkPublish)]
    public async Task<IActionResult> Close(Guid id, CancellationToken ct) => await ChangeStatus(id, HomeworkStatus.Closed, ct);

    [HttpPost("homework/{id:guid}/cancel"), Authorize(Policy = SchoolPermissionPolicies.HomeworkPublish)]
    public async Task<IActionResult> Cancel(Guid id, CancellationToken ct) => await ChangeStatus(id, HomeworkStatus.Cancelled, ct);

    [HttpGet("homework/{id:guid}/students"), Authorize(Policy = SchoolPermissionPolicies.HomeworkView)]
    public async Task<IActionResult> Students(Guid id, CancellationToken ct)
    {
        await using var db = await RequireDb(ct); if (db is null) return Unauthorized();
        var assignment = await db.HomeworkAssignments.AsNoTracking().Where(x => x.Id == id).Select(x => new { x.ClassSectionSubjectId, x.ClassSectionSubject.ClassSectionId }).SingleOrDefaultAsync(ct);
        if (assignment is null) return NotFound(Failure(404, "homework.not_found", "Homework was not found."));
        if (!await CanAccessSubject(db, assignment.ClassSectionId, assignment.ClassSectionSubjectId, ct)) return Forbid();
        var rows = await db.StudentHomework.AsNoTracking().Where(x => x.HomeworkAssignmentId == id).OrderBy(x => x.StudentEnrollment.Student.FullNameAr)
            .Select(x => new StudentHomeworkResponse(x.Id, x.StudentEnrollment.StudentId, x.StudentEnrollment.Student.StudentCode,
                x.StudentEnrollment.Student.FullNameAr, x.StudentEnrollment.Student.FullNameEn, x.Status, x.FinalScore, x.TeacherFeedback)).ToArrayAsync(ct);
        return Ok(ApiResponse<IReadOnlyList<StudentHomeworkResponse>>.Success(rows, correlationId: HttpContext.TraceIdentifier));
    }

    [HttpPost("homework/{id:guid}/students/sync"), Authorize(Policy = SchoolPermissionPolicies.HomeworkManage)]
    public async Task<IActionResult> SyncStudents(Guid id, CancellationToken ct)
    {
        await using var db = await RequireDb(ct); if (db is null) return Unauthorized();
        var entity = await db.HomeworkAssignments.Include(x => x.ClassSectionSubject).SingleOrDefaultAsync(x => x.Id == id, ct);
        if (entity is null) return NotFound(Failure(404, "homework.not_found", "Homework was not found."));
        if (!await CanAccessSubject(db, entity.ClassSectionSubject.ClassSectionId, entity.ClassSectionSubjectId, ct)) return Forbid();
        if (entity.Status == HomeworkStatus.Draft) return Conflict(Failure(409, "homework.not_published", "Publish the homework before synchronizing students."));
        var existing = await db.StudentHomework.AsNoTracking().Where(x => x.HomeworkAssignmentId == id).Select(x => x.StudentEnrollmentId).ToArrayAsync(ct);
        var missing = await db.StudentEnrollments.AsNoTracking().Where(x => x.ClassSectionId == entity.ClassSectionSubject.ClassSectionId && x.Status == StudentEnrollmentStatus.Active && x.Student.IsActive && !existing.Contains(x.Id)).Select(x => x.Id).ToArrayAsync(ct);
        var now = DateTimeOffset.UtcNow; db.StudentHomework.AddRange(missing.Select(enrollmentId => new StudentHomework { Id = Guid.NewGuid(), HomeworkAssignmentId = id, StudentEnrollmentId = enrollmentId, CreatedAtUtc = now, UpdatedAtUtc = now }));
        AddAudit(db, entity, "StudentsSynced", now, $"{{\"addedCount\":{missing.Length}}}"); await db.SaveChangesAsync(ct);
        return Ok(ApiResponse<object?>.Success(new { AddedCount = missing.Length }, correlationId: HttpContext.TraceIdentifier));
    }

    [HttpPost("homework/{id:guid}/students/{studentHomeworkId:guid}/excuse"), Authorize(Policy = SchoolPermissionPolicies.HomeworkGrade)]
    public async Task<IActionResult> Excuse(Guid id, Guid studentHomeworkId, [FromBody] ExcuseHomeworkRequest request, CancellationToken ct)
    {
        await using var db = await RequireDb(ct); if (db is null) return Unauthorized();
        var row = await db.StudentHomework.Include(x => x.HomeworkAssignment).ThenInclude(x => x.ClassSectionSubject).SingleOrDefaultAsync(x => x.Id == studentHomeworkId && x.HomeworkAssignmentId == id, ct);
        if (row is null) return NotFound(Failure(404, "homework.student_not_found", "Student homework was not found."));
        if (!await CanAccessSubject(db, row.HomeworkAssignment.ClassSectionSubject.ClassSectionId, row.HomeworkAssignment.ClassSectionSubjectId, ct)) return Forbid();
        if (string.IsNullOrWhiteSpace(request.Reason)) return BadRequest(Failure(400, "homework.excuse_reason_required", "Enter an excuse reason."));
        var now = DateTimeOffset.UtcNow; row.Status = StudentHomeworkStatus.Excused; row.ExcuseReason = Clean(request.Reason, 1000); row.UpdatedAtUtc = now;
        AddAudit(db, row.HomeworkAssignment, "StudentExcused", now, null, row.Id); await db.SaveChangesAsync(ct);
        return Ok(ApiResponse<object?>.Success(null, correlationId: HttpContext.TraceIdentifier));
    }

    [HttpPost("homework/{id:guid}/students/{studentHomeworkId:guid}/receive-offline"), Authorize(Policy = SchoolPermissionPolicies.HomeworkGrade)]
    public async Task<IActionResult> ReceiveOffline(Guid id, Guid studentHomeworkId, [FromBody] ReceiveOfflineHomeworkRequest request, CancellationToken ct)
    {
        await using var db = await RequireDb(ct); if (db is null) return Unauthorized();
        var row = await db.StudentHomework.Include(x => x.HomeworkAssignment).ThenInclude(x => x.ClassSectionSubject).SingleOrDefaultAsync(x => x.Id == studentHomeworkId && x.HomeworkAssignmentId == id, ct);
        if (row is null) return NotFound(Failure(404, "homework.student_not_found", "Student homework was not found."));
        if (!await CanAccessSubject(db, row.HomeworkAssignment.ClassSectionSubject.ClassSectionId, row.HomeworkAssignment.ClassSectionSubjectId, ct)) return Forbid();
        if (row.HomeworkAssignment.DeliveryMode != HomeworkDeliveryMode.Offline) return BadRequest(Failure(400, "homework.not_offline", "This operation is available for offline homework only."));
        if (request.Score is < 0 || request.Score > row.HomeworkAssignment.TotalScore) return BadRequest(Failure(400, "homework.score_invalid", "Score is outside the homework total."));
        var now = DateTimeOffset.UtcNow; row.Status = StudentHomeworkStatus.Graded; row.FinalScore = request.Score; row.TeacherFeedback = Clean(request.Feedback, 4000);
        row.GradedByUserId = CurrentUserId(); row.GradedAtUtc = now; row.UpdatedAtUtc = now;
        AddAudit(db, row.HomeworkAssignment, "OfflineGraded", now, $"{{\"score\":{request.Score.ToString(CultureInfo.InvariantCulture)}}}", row.Id);
        await db.SaveChangesAsync(ct); return Ok(ApiResponse<object?>.Success(null, correlationId: HttpContext.TraceIdentifier));
    }

    private async Task<IActionResult> ChangeStatus(Guid id, HomeworkStatus status, CancellationToken ct)
    {
        await using var db = await RequireDb(ct); if (db is null) return Unauthorized();
        var entity = await db.HomeworkAssignments.Include(x => x.ClassSectionSubject).SingleOrDefaultAsync(x => x.Id == id, ct);
        if (entity is null) return NotFound(Failure(404, "homework.not_found", "Homework was not found."));
        if (!await CanAccessSubject(db, entity.ClassSectionSubject.ClassSectionId, entity.ClassSectionSubjectId, ct)) return Forbid();
        if (entity.Status is HomeworkStatus.Closed or HomeworkStatus.Cancelled) return Conflict(Failure(409, "homework.status_final", "Homework is already closed or cancelled."));
        var now = DateTimeOffset.UtcNow; entity.Status = status; entity.ClosedAtUtc = now; entity.UpdatedAtUtc = now; AddAudit(db, entity, status.ToString(), now); await db.SaveChangesAsync(ct);
        return Ok(ApiResponse<object?>.Success(null, correlationId: HttpContext.TraceIdentifier));
    }

    private async Task<IActionResult?> Validate(SchoolsDbContext db, SaveHomeworkRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Title) || request.Title.Trim().Length > 200 || string.IsNullOrWhiteSpace(request.Instructions) || request.Instructions.Trim().Length > 6000 || request.DueAtUtc <= DateTimeOffset.UtcNow || request.MaximumAttempts is < 1 or > 20)
            return BadRequest(Failure(400, "homework.invalid", "Enter valid homework data and a future due date."));
        if (request.CurriculumSubjectBookId.HasValue && !await db.CurriculumSubjectBooks.AsNoTracking().AnyAsync(x => x.Id == request.CurriculumSubjectBookId && x.IsActive && !x.IsDeleted &&
            x.CurriculumGradeSubjectId == db.ClassSectionSubjects.Where(s => s.Id == request.ClassSectionSubjectId).Select(s => s.GradeSubjectOffering.CurriculumGradeSubjectId).FirstOrDefault(), ct))
            return BadRequest(Failure(400, "homework.book_invalid", "Select a book assigned to this subject."));
        if (request.Questions is null) return BadRequest(Failure(400, "homework.questions_invalid", "Questions are invalid."));
        if (request.DeliveryMode == HomeworkDeliveryMode.Online && request.Questions.Count == 0) return BadRequest(Failure(400, "homework.questions_required", "Online homework requires at least one question."));
        if (request.Questions.Select(x => x.SortOrder).Distinct().Count() != request.Questions.Count || request.Questions.Any(x => string.IsNullOrWhiteSpace(x.Prompt) || x.MaxScore <= 0)) return BadRequest(Failure(400, "homework.questions_invalid", "Check question prompts, order and scores."));
        foreach (var q in request.Questions)
        {
            var options = q.Options ?? [];
            if (q.Type is HomeworkQuestionType.SingleChoice or HomeworkQuestionType.MultipleChoice)
            {
                var correct = options.Count(x => x.IsCorrect);
                if (options.Count < 2 || (q.Type == HomeworkQuestionType.SingleChoice ? correct != 1 : correct < 1)) return BadRequest(Failure(400, "homework.options_invalid", "Choice questions require valid options and correct answers."));
            }
            if (q.Type == HomeworkQuestionType.Text && string.IsNullOrWhiteSpace(q.ModelAnswer)) return BadRequest(Failure(400, "homework.model_answer_required", "Text questions require a model answer."));
            if (q.Type == HomeworkQuestionType.FillInBlank && (q.Blanks is null || q.Blanks.Count == 0 || q.Blanks.Any(x => x.MaxScore <= 0 || x.AcceptedAnswers is null || x.AcceptedAnswers.Count == 0))) return BadRequest(Failure(400, "homework.blanks_invalid", "Fill-in-the-blank questions require blanks, scores and accepted answers."));
        }
        return null;
    }

    private static void Apply(HomeworkAssignment entity, SaveHomeworkRequest request, DateTimeOffset now)
    {
        entity.DeliveryMode = request.DeliveryMode; entity.Title = request.Title.Trim(); entity.Instructions = request.Instructions.Trim();
        entity.CurriculumSubjectBookId = request.CurriculumSubjectBookId; entity.BookReference = Clean(request.BookReference, 300); entity.DueAtUtc = request.DueAtUtc.ToUniversalTime();
        entity.TotalScore = request.Questions.Sum(x => x.MaxScore); entity.AllowLateSubmission = request.AllowLateSubmission; entity.MaximumAttempts = request.MaximumAttempts;
        entity.AllowUnsubmitBeforeDue = request.AllowUnsubmitBeforeDue; entity.ShowCorrectAnswersAfter = request.ShowCorrectAnswersAfter; entity.ShuffleQuestions = request.ShuffleQuestions; entity.ShuffleOptions = request.ShuffleOptions; entity.UpdatedAtUtc = now;
        foreach (var question in request.Questions.OrderBy(x => x.SortOrder))
        {
            var q = new HomeworkQuestion { Id = Guid.NewGuid(), Type = question.Type, Prompt = question.Prompt.Trim(), SortOrder = question.SortOrder,
                IsRequired = question.IsRequired, MaxScore = question.MaxScore, ModelAnswer = Clean(question.ModelAnswer, 4000), Explanation = Clean(question.Explanation, 4000), CreatedAtUtc = now, UpdatedAtUtc = now };
            foreach (var option in question.Options ?? []) q.Options.Add(new HomeworkQuestionOption { Id = Guid.NewGuid(), Text = option.Text.Trim(), SortOrder = option.SortOrder, IsCorrect = option.IsCorrect });
            foreach (var blank in question.Blanks ?? [])
            {
                var b = new HomeworkQuestionBlank { Id = Guid.NewGuid(), Token = blank.Token.Trim(), SortOrder = blank.SortOrder, MaxScore = blank.MaxScore,
                    IgnoreCase = blank.IgnoreCase, IgnoreDiacritics = blank.IgnoreDiacritics, CollapseWhitespace = blank.CollapseWhitespace, SendUnmatchedToManualReview = blank.SendUnmatchedToManualReview };
                var order = 0; foreach (var answer in blank.AcceptedAnswers) b.AcceptedAnswers.Add(new HomeworkBlankAcceptedAnswer { Id = Guid.NewGuid(), Answer = answer.Trim(), NormalizedAnswer = NormalizeAnswer(answer, blank), SortOrder = order++ });
                q.Blanks.Add(b);
            }
            entity.Questions.Add(q);
        }
    }

    private static string NormalizeAnswer(string value, SaveHomeworkBlankRequest settings)
    {
        var text = value.Trim(); if (settings.CollapseWhitespace) text = string.Join(' ', text.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));
        if (settings.IgnoreDiacritics) text = new string(text.Normalize(NormalizationForm.FormD).Where(c => CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark).ToArray()).Normalize(NormalizationForm.FormC);
        return settings.IgnoreCase ? text.ToUpperInvariant() : text;
    }

    private static HomeworkDetailsResponse ToDetails(HomeworkAssignment x) => new(x.Id, x.ClassSectionSubjectId, x.Title, x.Instructions, x.DeliveryMode, x.Status,
        x.CurriculumSubjectBookId, x.BookReference, x.DueAtUtc, x.TimeZoneIdSnapshot, x.TotalScore, x.AllowLateSubmission, x.MaximumAttempts, x.AllowUnsubmitBeforeDue,
        x.ShowCorrectAnswersAfter, x.ShuffleQuestions, x.ShuffleOptions, x.Questions.OrderBy(q => q.SortOrder).Select(q => new HomeworkQuestionResponse(q.Id, q.Type, q.Prompt, q.SortOrder,
            q.IsRequired, q.MaxScore, q.ModelAnswer, q.Explanation, q.Options.OrderBy(o => o.SortOrder).Select(o => new HomeworkOptionResponse(o.Id, o.Text, o.SortOrder, o.IsCorrect)).ToArray(),
            q.Blanks.OrderBy(b => b.SortOrder).Select(b => new HomeworkBlankResponse(b.Id, b.Token, b.SortOrder, b.MaxScore, b.IgnoreCase, b.IgnoreDiacritics, b.CollapseWhitespace,
                b.SendUnmatchedToManualReview, b.AcceptedAnswers.OrderBy(a => a.SortOrder).Select(a => a.Answer).ToArray())).ToArray())).ToArray());

    private static Task<HomeworkAssignment?> Load(SchoolsDbContext db, Guid id, CancellationToken ct) => db.HomeworkAssignments.Include(x => x.ClassSectionSubject).Include(x => x.Questions).ThenInclude(x => x.Options).Include(x => x.Questions).ThenInclude(x => x.Blanks).ThenInclude(x => x.AcceptedAnswers).SingleOrDefaultAsync(x => x.Id == id, ct);
    private void AddAudit(SchoolsDbContext db, HomeworkAssignment assignment, string action, DateTimeOffset now, string? payload = null, Guid? studentHomeworkId = null) => db.HomeworkAudits.Add(new HomeworkAudit { Id = Guid.NewGuid(), HomeworkAssignment = assignment, StudentHomeworkId = studentHomeworkId, Action = action, ActorUserId = CurrentUserId(), PayloadJson = payload, CreatedAtUtc = now });
    private async Task<bool> CanAccessClass(SchoolsDbContext db, Guid classSectionId, CancellationToken ct) => await IsAdmin(db, ct) || await db.ClassSectionTeacherScopes.AsNoTracking().AnyAsync(x => x.ClassSectionId == classSectionId && x.IsActive && !x.IsDeleted && x.TeacherGradeSubjectScope.IsActive && !x.TeacherGradeSubjectScope.IsDeleted && x.TeacherGradeSubjectScope.TeacherUserId == CurrentUserId(), ct);
    private async Task<bool> CanAccessSubject(SchoolsDbContext db, Guid classSectionId, Guid subjectId, CancellationToken ct)
    {
        var exists = await db.ClassSectionSubjects.AsNoTracking().AnyAsync(x => x.Id == subjectId && x.ClassSectionId == classSectionId && x.IsActive && !x.IsDeleted, ct); if (!exists) return false;
        return await IsAdmin(db, ct) || await db.ClassSectionTeacherScopes.AsNoTracking().AnyAsync(x => x.ClassSectionId == classSectionId && x.IsActive && !x.IsDeleted && x.TeacherGradeSubjectScope.IsActive && !x.TeacherGradeSubjectScope.IsDeleted && x.TeacherGradeSubjectScope.TeacherUserId == CurrentUserId() && x.TeacherGradeSubjectScope.GradeSubjectOfferingId == db.ClassSectionSubjects.Where(s => s.Id == subjectId).Select(s => s.GradeSubjectOfferingId).FirstOrDefault(), ct);
    }
    private Task<bool> IsAdmin(SchoolsDbContext db, CancellationToken ct) => db.LocalUserRoles.AsNoTracking().AnyAsync(x => x.UserId == CurrentUserId() && x.RoleId == SchoolIdentitySeed.SchoolAdminRoleId && x.Role.IsActive, ct);
    private async Task<SchoolsDbContext?> RequireDb(CancellationToken ct) => await dbFactory.CreateAsync(User.FindFirst(SchoolClaimTypes.SchoolCode)?.Value ?? string.Empty, ct);
    private Guid CurrentUserId() => Guid.TryParse(User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value, out var id) ? id : Guid.Empty;
    private ApiResponse<object?> Failure(int status, string code, string message) => ApiResponse<object?>.Failure(status, code, message, correlationId: HttpContext.TraceIdentifier);
    private static string? Clean(string? value, int max) => string.IsNullOrWhiteSpace(value) ? null : value.Trim()[..Math.Min(value.Trim().Length, max)];
}

public sealed record HomeworkSubjectResponse(Guid Id, string NameAr, string NameEn, IReadOnlyList<HomeworkBookResponse> Books);
public sealed record HomeworkBookResponse(Guid Id, string NameAr, string NameEn, string VersionLabel, string RoleNameAr, string RoleNameEn);
public sealed record HomeworkListItemResponse(Guid Id, Guid ClassSectionSubjectId, string SubjectNameAr, string SubjectNameEn, string Title, HomeworkDeliveryMode DeliveryMode, HomeworkStatus Status, DateTimeOffset DueAtUtc, decimal TotalScore, int StudentCount, int SubmittedCount, int PendingManualReviewCount, string CreatedBy, DateTimeOffset CreatedAtUtc);
public sealed record HomeworkDetailsResponse(Guid Id, Guid ClassSectionSubjectId, string Title, string Instructions, HomeworkDeliveryMode DeliveryMode, HomeworkStatus Status, Guid? CurriculumSubjectBookId, string? BookReference, DateTimeOffset DueAtUtc, string TimeZoneId, decimal TotalScore, bool AllowLateSubmission, int MaximumAttempts, bool AllowUnsubmitBeforeDue, HomeworkCorrectAnswersPolicy ShowCorrectAnswersAfter, bool ShuffleQuestions, bool ShuffleOptions, IReadOnlyList<HomeworkQuestionResponse> Questions);
public sealed record HomeworkQuestionResponse(Guid Id, HomeworkQuestionType Type, string Prompt, int SortOrder, bool IsRequired, decimal MaxScore, string? ModelAnswer, string? Explanation, IReadOnlyList<HomeworkOptionResponse> Options, IReadOnlyList<HomeworkBlankResponse> Blanks);
public sealed record HomeworkOptionResponse(Guid Id, string Text, int SortOrder, bool IsCorrect);
public sealed record HomeworkBlankResponse(Guid Id, string Token, int SortOrder, decimal MaxScore, bool IgnoreCase, bool IgnoreDiacritics, bool CollapseWhitespace, bool SendUnmatchedToManualReview, IReadOnlyList<string> AcceptedAnswers);
public sealed record StudentHomeworkResponse(Guid Id, Guid StudentId, string StudentCode, string FullNameAr, string FullNameEn, StudentHomeworkStatus Status, decimal? FinalScore, string? TeacherFeedback);
public sealed record SaveHomeworkRequest(Guid ClassSectionSubjectId, HomeworkDeliveryMode DeliveryMode, string Title, string Instructions, Guid? CurriculumSubjectBookId, string? BookReference, DateTimeOffset DueAtUtc, bool AllowLateSubmission, int MaximumAttempts, bool AllowUnsubmitBeforeDue, HomeworkCorrectAnswersPolicy ShowCorrectAnswersAfter, bool ShuffleQuestions, bool ShuffleOptions, IReadOnlyList<SaveHomeworkQuestionRequest> Questions);
public sealed record SaveHomeworkQuestionRequest(HomeworkQuestionType Type, string Prompt, int SortOrder, bool IsRequired, decimal MaxScore, string? ModelAnswer, string? Explanation, IReadOnlyList<SaveHomeworkOptionRequest>? Options, IReadOnlyList<SaveHomeworkBlankRequest>? Blanks);
public sealed record SaveHomeworkOptionRequest(string Text, int SortOrder, bool IsCorrect);
public sealed record SaveHomeworkBlankRequest(string Token, int SortOrder, decimal MaxScore, bool IgnoreCase, bool IgnoreDiacritics, bool CollapseWhitespace, bool SendUnmatchedToManualReview, IReadOnlyList<string> AcceptedAnswers);
public sealed record ReceiveOfflineHomeworkRequest(decimal Score, string? Feedback);
public sealed record ExcuseHomeworkRequest(string Reason);
