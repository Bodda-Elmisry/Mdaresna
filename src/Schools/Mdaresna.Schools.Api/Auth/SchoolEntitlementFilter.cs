using Mdaresna.Api.Contracts;
using Mdaresna.Schools.Infrastructure.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;

namespace Mdaresna.Schools.Api.Auth;

public sealed class SchoolEntitlementFilter(ISchoolDbContextFactory factory, IConfiguration config) : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var controller = context.RouteData.Values["controller"]?.ToString() ?? "";
        var capability = controller switch {
            "SchoolStudents" => context.HttpContext.Request.Path.Value?.Contains("/applications", StringComparison.Ordinal) == true ? "academic.admissions" : "academic.students",
            "SchoolStudentAttendance" => "academic.attendance",
            "SchoolClassActivities" => "academic.activities",
            "SchoolHomework" => "academic.homework",
            "SchoolCurriculum" => "academic.curriculum",
            "SchoolTeachingPlanning" or "SchoolTeachingPlans" => "academic.teaching-plans",
            "SchoolSocial" => "academic.social",
            "SchoolExams" or "SchoolExamManagement" or "SchoolExamWorkflow" or "SchoolStudentLessonEvaluations" or "SchoolStudentLessonNotes" => "academic.assessments",
            "SchoolClassAssignments" or "SchoolOperationalClasses" or "SchoolClassStructure" or "SchoolAcademicSettings" or
            "SchoolPeopleOperations" or "SchoolDepartments" or "SchoolFacilities" => "academic.staff",
            _ => null
        };
        if (capability == null || context.HttpContext.User.Identity?.IsAuthenticated != true) { await next(); return; }
        var ct = context.HttpContext.RequestAborted;
        await using var db = await factory.CreateAsync(context.HttpContext.User.FindFirst(SchoolClaimTypes.SchoolCode)?.Value ?? "", ct);
        if (db == null) { Reject("subscription.not_synced"); return; }
        var states = await db.Subscriptions.AsNoTracking().Where(x => x.IsEnforced && x.CapacityMetric == "Student").ToArrayAsync(ct);
        if (states.Length == 0 && !config.GetValue<bool>("SchoolSubscriptions:RequireEntitlement")) { await next(); return; }
        if (states.Length > 1) { Reject("subscription.capacity_conflict"); return; }
        var state = states.SingleOrDefault();
        if (state == null) { Reject("subscription.not_synced"); return; }
        if (!state.CapabilityCodes.Split('\n').Contains(capability)) { Reject("subscription.not_entitled"); return; }
        var read = HttpMethods.IsGet(context.HttpContext.Request.Method) || HttpMethods.IsHead(context.HttpContext.Request.Method);
        var valid = !state.IsSuspended && DateTimeOffset.UtcNow >= state.ValidFromUtc && DateTimeOffset.UtcNow < state.ValidUntilUtc;
        if (!valid && (!read || config.GetValue<bool>("SchoolSubscriptions:BlockExpiredReads"))) { Reject(state.IsSuspended ? "subscription.suspended" : "subscription.expired"); return; }
        await next();
        void Reject(string code) => context.Result = new ObjectResult(ApiResponse<object>.Failure(403, code, "The school subscription does not allow this operation.")) { StatusCode = 403 };
    }
}
