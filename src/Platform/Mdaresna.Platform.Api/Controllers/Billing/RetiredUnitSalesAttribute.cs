using Mdaresna.Api.Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Mdaresna.Platform.Api.Controllers.Billing;

/// <summary>Historical records stay readable; no new unit sales or catalog mutations.</summary>
public sealed class RetiredUnitSalesAttribute : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        if (HttpMethods.IsGet(context.HttpContext.Request.Method) || HttpMethods.IsHead(context.HttpContext.Request.Method)) return;
        context.Result = new ObjectResult(ApiResponse<object>.Failure(410, "unit_sales.retired", "Use systems, plans and subscriptions.")) { StatusCode = 410 };
    }
}
