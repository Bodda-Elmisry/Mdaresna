using Mdaresna.Schools.Domain.Exams;
using Mdaresna.Schools.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;

namespace Mdaresna.Schools.Api.Exams;

// Explicit tenant allow-list: never infer or scan other schools' databases.
public sealed class ExamWorkflowReminderService(IServiceScopeFactory scopes, IConfiguration configuration,
    ILogger<ExamWorkflowReminderService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
        using var timer = new PeriodicTimer(TimeSpan.FromMinutes(1));
        while (await timer.WaitForNextTickAsync(stoppingToken)) {
            foreach (var code in configuration.GetSection("ExamWorkflow:SchoolCodes").Get<string[]>() ?? []) {
                try {
                    await using var scope = scopes.CreateAsyncScope();
                    var factory = scope.ServiceProvider.GetRequiredService<ISchoolDbContextFactory>();
                    await using var db = await factory.CreateAsync(code, stoppingToken); if (db is null) continue;
                    var now = DateTimeOffset.UtcNow;
                    var runs = await db.ExamWorkflows.Include(x => x.Series).Include(x => x.Steps)
                        .Where(x => x.Status == ExamWorkflowStatus.Active && x.Steps.Any(s => s.Status == ExamWorkflowStepStatus.Active && s.DueAtUtc <= now &&
                            (!s.RemindedAtUtc.HasValue || !s.EscalatedAtUtc.HasValue && s.SupervisorUserId.HasValue && s.DueAtUtc <= now.AddHours(-24))))
                        .OrderBy(x => x.UpdatedAtUtc).Take(100).ToListAsync(stoppingToken);
                    foreach (var run in runs) {
                        var changed = false;
                        foreach (var step in run.Steps.Where(x => x.Status == ExamWorkflowStepStatus.Active && x.DueAtUtc <= now)) {
                            var papers = await db.ExamPapers.Where(p => p.ExamSeriesId == run.ExamSeriesId && (!step.ExamPaperId.HasValue || p.Id == step.ExamPaperId)).Select(p => p.Id).ToArrayAsync(stoppingToken);
                            if (!step.RemindedAtUtc.HasValue) {
                                if (!changed) { run.Revision++; changed = true; }
                                if (!await ExamWorkflowEngine.Eligible(db, step.AssigneeUserId, step.Stage, papers, stoppingToken) && step.BackupUserId.HasValue &&
                                    await ExamWorkflowEngine.Eligible(db, step.BackupUserId.Value, step.Stage, papers, stoppingToken)) step.AssigneeUserId = step.BackupUserId.Value;
                                if (await ExamWorkflowEngine.Eligible(db, step.AssigneeUserId, step.Stage, papers, stoppingToken))
                                    ExamWorkflowEngine.Notify(db, run, step.AssigneeUserId, "ExamWorkflowOverdue", "مهمة اختبار متأخرة", "Exam task overdue", $"{run.Series.NameAr} — {ExamWorkflowEngine.StageAr(step.Stage)}. يرجى إتمام المهمة.", $"{run.Series.NameEn} — {step.Stage}. Please complete your task.", Guid.Empty, step.Id);
                                step.RemindedAtUtc = now;
                            }
                            if (!step.EscalatedAtUtc.HasValue && step.SupervisorUserId.HasValue && step.DueAtUtc <= now.AddHours(-24)) {
                                if (!changed) { run.Revision++; changed = true; }
                                if (await ExamWorkflowEngine.Eligible(db, step.SupervisorUserId.Value, step.Stage, papers, stoppingToken))
                                    ExamWorkflowEngine.Notify(db, run, step.SupervisorUserId.Value, "ExamWorkflowEscalated", "مرحلة اختبار تحتاج متابعة", "Exam stage needs follow-up", $"{run.Series.NameAr} — {ExamWorkflowEngine.StageAr(step.Stage)} متأخرة أكثر من 24 ساعة.", $"{run.Series.NameEn} — {step.Stage} is over 24 hours late.", Guid.Empty, step.Id);
                                step.EscalatedAtUtc = now;
                            }
                        }
                        if (changed) run.UpdatedAtUtc = now;
                    }
                    await db.SaveChangesAsync(stoppingToken);
                } catch (DbUpdateConcurrencyException) { /* Another action won; retry current state next tick. */ }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { return; }
                catch (Exception) { logger.LogWarning("Exam workflow reminder processing failed; retrying next tick."); }
            }
        }
    }
}
