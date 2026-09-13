using System.Reflection;
using Mdaresna.Platform.Api.Auth;
using Mdaresna.Platform.Api.Controllers.Billing;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;

namespace Mdaresna.Platform.UnitTests.Billing;

public sealed class UnitCommerceControllerRouteTests
{
    [Theory]
    [InlineData(typeof(UnitTypesController), "List", "api/platform/v1/unit-types", "GET", null, "platform.billing.read")]
    [InlineData(typeof(UnitTypesController), "Get", "api/platform/v1/unit-types", "GET", "{unitTypeId:guid}", "platform.billing.read")]
    [InlineData(typeof(UnitTypesController), "Create", "api/platform/v1/unit-types", "POST", null, "platform.billing.manage")]
    [InlineData(typeof(UnitTypesController), "Update", "api/platform/v1/unit-types", "PUT", "{unitTypeId:guid}", "platform.billing.manage")]
    [InlineData(typeof(UnitTypesController), "Deactivate", "api/platform/v1/unit-types", "POST", "{unitTypeId:guid}/deactivate", "platform.billing.manage")]
    [InlineData(typeof(UnitPurchasesController), "Get", "api/platform/v1/unit-purchases", "GET", "{requestId:guid}", "platform.billing.read")]
    [InlineData(typeof(UnitPurchasesController), "Submit", "api/platform/v1/unit-purchases", "POST", null, "platform.billing.manage")]
    public void Unit_commerce_endpoint_has_expected_route_and_platform_permission(
        Type controllerType,
        string actionName,
        string route,
        string httpMethod,
        string? actionRoute,
        string permission)
    {
        var controllerRoute = controllerType.GetCustomAttribute<RouteAttribute>();
        var action = controllerType.GetMethod(actionName);
        var actionHttp = action?.GetCustomAttributes<HttpMethodAttribute>().SingleOrDefault();
        var policy = action?.GetCustomAttribute<PlatformPermissionAttribute>()?.Policy;

        Assert.Equal(route, controllerRoute?.Template);
        Assert.NotNull(actionHttp);
        Assert.Contains(httpMethod, actionHttp.HttpMethods);
        Assert.Equal(actionRoute, actionHttp.Template);
        Assert.EndsWith(permission, policy, StringComparison.Ordinal);
    }
}
