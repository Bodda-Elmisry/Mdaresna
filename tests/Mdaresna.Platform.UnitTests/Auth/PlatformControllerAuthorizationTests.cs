using System.Reflection;
using Mdaresna.Platform.Api.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;

namespace Mdaresna.Platform.UnitTests.Auth;

public sealed class PlatformControllerAuthorizationTests
{
    [Fact]
    public void Every_business_action_requires_a_platform_permission()
    {
        var controllers = typeof(Program).Assembly.GetTypes()
            .Where(type => !type.IsAbstract && typeof(ControllerBase).IsAssignableFrom(type))
            .ToArray();

        Assert.NotEmpty(controllers);
        foreach (var controller in controllers)
        {
            var actions = controller.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .Where(method => method.GetCustomAttributes<HttpMethodAttribute>().Any())
                .ToArray();

            foreach (var action in actions)
            {
                if (action.GetCustomAttribute<AllowAnonymousAttribute>() is not null)
                {
                    Assert.Equal("PlatformAuthController", controller.Name);
                    Assert.Contains(action.Name, new[]
                    {
                        "Login",
                        "StartFirstOwnerActivation",
                        "CompleteFirstOwnerActivation",
                        "StartPasswordReset",
                        "CompletePasswordReset",
                        "StartStaffActivation",
                        "CompleteStaffActivation"
                    });
                    continue;
                }

                if (controller.Name is "PlatformAccountLanguageController" or "PlatformAccountContactsController" or
                    "PlatformAccountProfileController" or "PlatformNotificationsController" or
                    "PlatformCurrentAccessController")
                {
                    // These endpoints access only the signed-in account's own profile, device,
                    // notifications, or effective access snapshot.
                    Assert.NotNull(controller.GetCustomAttribute<AuthorizeAttribute>());
                    Assert.Contains(action.Name, controller.Name switch
                    {
                        "PlatformAccountLanguageController" => new[] { "Get", "Put" },
                        "PlatformAccountContactsController" => new[] { "Get", "Create", "Update", "Delete" },
                        "PlatformAccountProfileController" => new[] { "Get", "Update", "GetImage", "UploadImage", "DeleteImage" },
                        "PlatformNotificationsController" => new[]
                        {
                            "RegisterDevice", "RemoveDevice", "List", "UnreadCount", "MarkRead", "MarkAllRead"
                        },
                        _ => new[] { "Get" }
                    });
                    continue;
                }

                Assert.True(
                    action.GetCustomAttribute<PlatformPermissionAttribute>() is not null ||
                    controller.GetCustomAttribute<PlatformPermissionAttribute>() is not null,
                    $"{controller.Name}.{action.Name} lacks a Platform permission.");
            }
        }
    }
}
