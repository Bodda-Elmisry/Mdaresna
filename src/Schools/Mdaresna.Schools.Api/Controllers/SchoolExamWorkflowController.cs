using System.Text.Json;
using Mdaresna.Api.Contracts;
using Mdaresna.Schools.Api.Auth;
using Mdaresna.Schools.Api.Exams;
using Mdaresna.Schools.Domain.Exams;
using Mdaresna.Schools.Domain.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace Mdaresna.Schools.Api.Controllers;

public sealed partial class SchoolExamsController
{
    [HttpGet("exam-series/{id:guid}/workflow"), Authorize(Policy = SchoolPermissionPolicies.ExamsView)]
    public async Task<IActionResult> Workflow(Guid id, CancellationToken ct) {
        await using var db = await RequireDb(ct); if (db is null) return Unauthorized();
        if (!await CanAccessSeries(db, id, ct) && !await db.ExamInvigilatorAssignments.AnyAsync(x => x.ExamCommittee.ExamSeriesId == id && x.UserId == CurrentUserId() && x.Status == ExamInvigilatorStatus.Assigned, ct)) return Forbid();
        var series = await LoadSeries(db, id, ct); if (series is null) return NotFound();
        var run = await db.ExamWorkflows.Include(x => x.Steps).SingleOrDefaultAsync(x => x.ExamSeriesId == id, ct);
        var users = await db.LocalUsers.Where(x => x.Status == LocalUserStatus.Active).Select(x => new { x.Id, x.Person.DisplayName }).ToListAsync(ct);
        var templates = new List<object>(); var defaults = WorkflowDefaults(series);
        foreach (var t in ExamWorkflowEngine.Templates(series)) {
            var ids = t.PaperId.HasValue ? new[] { t.PaperId.Value } : series.Papers.Select(p => p.Id).ToArray();
            var eligible = new List<object>(); var eligibleIds = new List<Guid>();
            foreach (var u in users) if (await ExamWorkflowEngine.Eligible(db, u.Id, t.Stage, ids, ct)) { eligible.Add(new { u.Id, name = u.DisplayName }); eligibleIds.Add(u.Id); }
            var selected = defaults.FirstOrDefault(x => x.Stage == t.Stage)?.AssigneeUserId;
            if (!selected.HasValue || !eligibleIds.Contains(selected.Value)) selected = eligibleIds.Count == 1 ? eligibleIds[0] : eligibleIds.Contains(series.CreatedByUserId) && t.Stage != ExamWorkflowStage.DepartmentReview ? series.CreatedByUserId : null;
            templates.Add(new { t.Stage, t.PaperId, t.Order, t.NameAr, t.NameEn, users = eligible, defaultUserId = selected });
        }
        return Ok(ApiResponse<object>.Success(new { series.Id, series.NameAr, series.NameEn, series.Status, workflowStatus = run?.Status,
            revision = run?.Revision ?? 0, templates, steps = run?.Steps.OrderBy(x => x.Order).Select(x => new { x.Id, x.Stage, x.ExamPaperId,
                x.AssigneeUserId, assigneeName = users.FirstOrDefault(u => u.Id == x.AssigneeUserId)?.DisplayName, x.BackupUserId, x.SupervisorUserId,
                x.DueAtUtc, x.Status, x.CompletedAtUtc, x.ReturnReason }), canManage = await ExamWorkflowEngine.Permission(db, CurrentUserId(), "school.exams.manage", ct),
            actorId = CurrentUserId() }));
    }
    [HttpGet("exam-workflow/tasks"), Authorize(Policy = SchoolPermissionPolicies.ExamsView)]
    public async Task<IActionResult> WorkflowTasks(int pageNumber = 1, int pageSize = 20, CancellationToken ct = default) {
        await using var db = await RequireDb(ct); if (db is null) return Unauthorized();
        pageNumber = Math.Max(1, pageNumber); pageSize = Math.Clamp(pageSize, 1, 100);
        var actor = CurrentUserId();
        var accessible = ApplyScope(db, db.ExamPapers.AsNoTracking()).Select(x => x.ExamSeriesId);
        var query = ExamWorkflowEngine.Tasks(db, actor, accessible);
        var totalCount = await query.CountAsync(ct);
        var items = await query.OrderBy(x => x.DueAtUtc).ThenBy(x => x.Id).Skip((pageNumber-1)*pageSize).Take(pageSize).ToListAsync(ct);
        return Ok(ApiResponse<object>.Success(new { items, totalCount, pageNumber, totalPages = (totalCount+pageSize-1)/pageSize }));
    }
    [HttpPost("exam-series/{id:guid}/workflow"), Authorize(Policy = SchoolPermissionPolicies.ExamsManage)]
    public async Task<IActionResult> ConfigureWorkflow(Guid id, ConfigureWorkflowRequest r, CancellationToken ct) {
        await using var db = await RequireDb(ct); if (db is null) return Unauthorized();
        if (!await CanAccessSeries(db, id, ct)) return Forbid();
        var series = await LoadSeries(db, id, ct); if (series is null) return NotFound();
        var run = await db.ExamWorkflows.Include(x => x.Steps).SingleOrDefaultAsync(x => x.ExamSeriesId == id, ct);
        if (series.Status is ExamSeriesStatus.Published or ExamSeriesStatus.Closed or ExamSeriesStatus.Cancelled || run?.Status is ExamWorkflowStatus.Active or ExamWorkflowStatus.Completed or ExamWorkflowStatus.Cancelled)
            return Conflict(Failure(409, "exams.workflow_locked", "The workflow cannot be configured now."));
        if ((run?.Revision ?? 0) != r.Revision) return WorkflowConflict();
        var templates = ExamWorkflowEngine.Templates(series);
        if (r.Assignments is null || r.Assignments.Any(a => a is null) || r.Assignments.Count != templates.Count || r.Assignments.Select(a => ExamWorkflowRules.Key(a.Stage, a.PaperId)).Distinct().Count() != templates.Count)
            return BadRequest(Failure(400, "exams.workflow_invalid", "Assign every required stage."));
        foreach (var t in templates) {
            var a = r.Assignments.SingleOrDefault(a => a.Stage == t.Stage && a.PaperId == t.PaperId);
            var ids = t.PaperId.HasValue ? new[] { t.PaperId.Value } : series.Papers.Select(p => p.Id).ToArray();
            if (a is null || a.DueAtUtc.HasValue && a.DueAtUtc <= DateTimeOffset.UtcNow || !await ExamWorkflowEngine.Eligible(db, a.AssigneeUserId, t.Stage, ids, ct) ||
                a.BackupUserId.HasValue && (a.BackupUserId == a.AssigneeUserId || !await ExamWorkflowEngine.Eligible(db, a.BackupUserId.Value, t.Stage, ids, ct)) ||
                a.SupervisorUserId.HasValue && !await ExamWorkflowEngine.Eligible(db, a.SupervisorUserId.Value, t.Stage, ids, ct))
                return BadRequest(Failure(400, "exams.workflow_assignee_invalid", "Select eligible assignees and future deadlines."));
        }
        run ??= new ExamWorkflow { ExamSeriesId = id, Series = series, CreatedByUserId = CurrentUserId(), Revision = 0 };
        if (run.Revision == 0) db.ExamWorkflows.Add(run);
        foreach (var t in templates) {
            var a = r.Assignments.Single(x => x.Stage == t.Stage && x.PaperId == t.PaperId);
            var s = run.Steps.SingleOrDefault(x => x.Stage == t.Stage && x.ExamPaperId == t.PaperId);
            if (s is null) { s = new ExamWorkflowStep { Id = Guid.NewGuid(), Workflow = run, Stage = t.Stage, ExamPaperId = t.PaperId, Order = t.Order }; run.Steps.Add(s); }
            s.AssigneeUserId = a.AssigneeUserId; s.BackupUserId = a.BackupUserId; s.SupervisorUserId = a.SupervisorUserId; s.DueAtUtc = a.DueAtUtc;
            s.Status = ExamWorkflowStepStatus.Waiting; s.CompletedAtUtc = null; s.RemindedAtUtc = null; s.EscalatedAtUtc = null;
        }
        run.Revision++; run.UpdatedAtUtc = DateTimeOffset.UtcNow; run.Status = r.Start ? ExamWorkflowStatus.Active : ExamWorkflowStatus.Configured;
        series.Status = ExamSeriesStatus.Draft;
        series.ApprovedByUserId = null; series.ApprovedAtUtc = null;
        if (r.Start) ExamWorkflowEngine.Advance(db, run, CurrentUserId());
        AddAudit(db, series, r.Start ? "WorkflowStarted" : "WorkflowConfigured", run.UpdatedAtUtc);
        return await SaveWorkflow(db, ct);
    }
    [HttpPost("exam-series/{id:guid}/workflow/steps/{stepId:guid}/{action}"), Authorize(Policy = SchoolPermissionPolicies.ExamsView)]
    public async Task<IActionResult> WorkflowAction(Guid id, Guid stepId, string action, WorkflowActionRequest r, CancellationToken ct) {
        await using var db = await RequireDb(ct); if (db is null) return Unauthorized();
        var run = await db.ExamWorkflows.Include(x => x.Steps).Include(x => x.Series).SingleOrDefaultAsync(x => x.ExamSeriesId == id, ct);
        if (run is null) return NotFound(); if (run.Revision != r.Revision) return WorkflowConflict();
        var step = run.Steps.SingleOrDefault(x => x.Id == stepId);
        if (step is null || run.Status != ExamWorkflowStatus.Active || step.Status != ExamWorkflowStepStatus.Active || !await CanAccessSeries(db, id, ct))
            return Conflict(Failure(409, "exams.workflow_transition_invalid", "This task is no longer active."));
        var actor = CurrentUserId(); var papers = await db.ExamPapers.Where(x => x.ExamSeriesId == id && (!step.ExamPaperId.HasValue || x.Id == step.ExamPaperId)).Select(x => x.Id).ToArrayAsync(ct);
        if (action == "reassign") {
            if (!await ExamWorkflowEngine.Permission(db, actor, "school.exams.manage", ct)) return Forbid();
            if (!r.UserId.HasValue || string.IsNullOrWhiteSpace(r.Reason) || r.Reason.Length > 1000 ||
                !await ExamWorkflowEngine.Eligible(db, r.UserId.Value, step.Stage, papers, ct)) return BadRequest(Failure(400, "exams.workflow_assignee_invalid", "Select an eligible assignee and a reason."));
            run.Revision++;
            ExamWorkflowEngine.Notify(db, run, step.AssigneeUserId, "ExamWorkflowReassigned", "أعيد إسناد مهمة الاختبار", "Exam task reassigned", $"{run.Series.NameAr} — {r.Reason}", $"{run.Series.NameEn} — {r.Reason}", actor, step.Id);
            step.AssigneeUserId = r.UserId.Value; step.RemindedAtUtc = null; step.EscalatedAtUtc = null;
            ExamWorkflowEngine.Notify(db, run, step.AssigneeUserId, "ExamWorkflowAction", "مهمة اختبار مسندة إليك", "Exam task assigned", $"{run.Series.NameAr} — {ExamWorkflowEngine.StageAr(step.Stage)}", $"{run.Series.NameEn} — {step.Stage}", actor, step.Id);
        } else {
            if (step.AssigneeUserId != actor || !await ExamWorkflowEngine.Eligible(db, actor, step.Stage, papers, ct)) return Forbid();
            if (action == "return") {
                if (string.IsNullOrWhiteSpace(r.Reason) || r.Reason.Length > 1000) return BadRequest(Failure(400, "exams.reason_required", "Enter a return reason."));
                run.Revision++; run.Status = ExamWorkflowStatus.Returned; run.Series.Status = ExamSeriesStatus.Draft;
                run.Series.ApprovedByUserId = null; run.Series.ApprovedAtUtc = null;
                step.ReturnReason = r.Reason.Trim(); foreach (var s in run.Steps) { s.Status = ExamWorkflowStepStatus.Waiting; s.CompletedAtUtc = null; }
                foreach (var a in await db.ExamApprovals.Where(x => x.ExamSeriesId == id).ToListAsync(ct)) a.Status = ExamApprovalStatus.Pending;
                foreach (var user in run.Steps.Select(x => x.AssigneeUserId).Append(run.Series.CreatedByUserId).Distinct())
                    ExamWorkflowEngine.Notify(db, run, user, "ExamWorkflowReturned", "الاختبار يحتاج تعديلًا", "Exam returned for changes", $"{run.Series.NameAr} — السبب: {r.Reason}. المهام السابقة توقفت لحين إعادة الإرسال.", $"{run.Series.NameEn} — {r.Reason}. Previous tasks are paused until resubmission.", actor);
            } else if (action == "complete" && step.Stage != ExamWorkflowStage.Publish) {
                if (!ExamWorkflowRules.AllEarlierCompleted(run.Steps, step.Order)) return WorkflowConflict();
                if (step.Stage is ExamWorkflowStage.Schedule or ExamWorkflowStage.Committees && (await FindConflicts(db, id, ct)).Any(c => c.Severity == "Blocker"))
                    return Conflict(Failure(409, "exams.schedule_conflict", "Resolve schedule conflicts before handing off."));
                if (step.Stage == ExamWorkflowStage.Schedule && !await db.ExamScheduleWindows.AnyAsync(x => x.ExamSeriesId == id && x.Status != ExamScheduleWindowStatus.Cancelled, ct))
                    return Conflict(Failure(409, "exams.incomplete", "Complete the exam schedule."));
                if (step.Stage == ExamWorkflowStage.Committees && (!await db.ExamCommittees.AnyAsync(x => x.ExamSeriesId == id, ct) ||
                    await db.ExamCommittees.AnyAsync(x => x.ExamSeriesId == id && !x.Invigilators.Any(a => a.Status == ExamInvigilatorStatus.Assigned), ct)))
                    return Conflict(Failure(409, "exams.incomplete", "Complete committees and invigilator assignments."));
                run.Revision++; step.Status = ExamWorkflowStepStatus.Completed; step.CompletedAtUtc = DateTimeOffset.UtcNow;
                if (step.Stage is ExamWorkflowStage.DepartmentReview or ExamWorkflowStage.SchoolApproval) {
                    var approval = await db.ExamApprovals.SingleOrDefaultAsync(x => x.ExamSeriesId == id && x.ExamPaperId == step.ExamPaperId && x.Stage == ExamApprovalStage.Publish && x.StepOrder == step.Order, ct);
                    if (approval is null) { approval = new ExamApproval { Id = Guid.NewGuid(), ExamSeriesId = id, ExamPaperId = step.ExamPaperId, Stage = ExamApprovalStage.Publish, StepOrder = step.Order }; db.ExamApprovals.Add(approval); }
                    approval.Status = ExamApprovalStatus.Approved; approval.ApproverUserId = actor; approval.DecidedAtUtc = DateTimeOffset.UtcNow;
                }
                ExamWorkflowEngine.Advance(db, run, actor);
                if (run.Series.Status == ExamSeriesStatus.Approved) { run.Series.ApprovedByUserId = actor; run.Series.ApprovedAtUtc = DateTimeOffset.UtcNow; }
            } else return BadRequest(Failure(400, "exams.workflow_invalid", "Use the publish action for the publication step."));
        }
        run.UpdatedAtUtc = DateTimeOffset.UtcNow; run.Series.UpdatedAtUtc = run.UpdatedAtUtc;
        AddAudit(db, run.Series, $"Workflow-{action}", run.UpdatedAtUtc, r.Reason, step.ExamPaperId, payload: JsonSerializer.Serialize(new { step.Id, step.Stage, run.Revision, step.AssigneeUserId }));
        return await SaveWorkflow(db, ct);
    }
    private static IReadOnlyList<WorkflowDefault> WorkflowDefaults(ExamSeries series) {
        if (string.IsNullOrEmpty(series.PolicySnapshotJson)) return [];
        using var json = JsonDocument.Parse(series.PolicySnapshotJson);
        if (!json.RootElement.TryGetProperty("WorkflowDefaultsJson", out var v) || v.ValueKind != JsonValueKind.String) return [];
        return JsonSerializer.Deserialize<WorkflowDefault[]>(v.GetString() ?? "[]") ?? [];
    }
    private IActionResult WorkflowConflict() => Conflict(Failure(409, "exams.workflow_conflict", "The workflow changed. Reload before retrying."));
    private async Task<IActionResult> SaveWorkflow(Mdaresna.Schools.Infrastructure.Persistence.SchoolsDbContext db, CancellationToken ct) {
        try { await db.SaveChangesAsync(ct); return Ok(ApiResponse<object?>.Success(null)); }
        catch (DbUpdateConcurrencyException) { return WorkflowConflict(); }
        catch (DbUpdateException e) when (e.InnerException is Npgsql.PostgresException { SqlState: "23505" } or Microsoft.Data.SqlClient.SqlException { Number: 2601 or 2627 }) { return WorkflowConflict(); }
    }
}
public sealed record ConfigureWorkflowRequest(int Revision, IReadOnlyList<WorkflowAssignment> Assignments, bool Start);
public sealed record WorkflowActionRequest(int Revision, string? Reason, Guid? UserId);
