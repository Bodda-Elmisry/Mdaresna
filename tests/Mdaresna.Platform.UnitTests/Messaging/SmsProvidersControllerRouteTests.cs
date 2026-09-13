using System.Reflection;
using Mdaresna.Platform.Api.Auth;
using Mdaresna.Platform.Api.Controllers.Messaging;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;

namespace Mdaresna.Platform.UnitTests.Messaging;

public sealed class SmsProvidersControllerRouteTests
{
    [Theory]
    [InlineData("List", "GET", null)]
    [InlineData("Get", "GET", "{providerId:guid}")]
    [InlineData("Create", "POST", null)]
    [InlineData("Update", "PUT", "{providerId:guid}")]
    [InlineData("Activate", "POST", "{providerId:guid}/activate")]
    [InlineData("Deactivate", "POST", "{providerId:guid}/deactivate")]
    [InlineData("Delete", "DELETE", "{providerId:guid}")]
    public void Management_action_has_expected_route_and_permission(
        string actionName,
        string method,
        string? actionRoute)
    {
        var controller = typeof(SmsProvidersController);
        Assert.Equal("api/platform/v1/sms-providers",
            controller.GetCustomAttribute<RouteAttribute>()?.Template);

        var action = controller.GetMethod(actionName);
        Assert.NotNull(action);
        var httpAttribute = action.GetCustomAttributes<HttpMethodAttribute>().Single();
        Assert.Contains(method, httpAttribute.HttpMethods);
        Assert.Equal(actionRoute, httpAttribute.Template);
        Assert.EndsWith("platform.deployments.manage",
            action.GetCustomAttribute<PlatformPermissionAttribute>()?.Policy,
            StringComparison.Ordinal);
    }
}
