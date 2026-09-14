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
                        "CompletePasswordReset"
                    });
                    continue;
                }

                if (controller.Name is "PlatformAccountLanguageController" or "PlatformAccountContactsController" or "PlatformAccountProfileController")
                {
                    // These endpoints access only the signed-in account's shared profile.
                    Assert.NotNull(controller.GetCustomAttribute<AuthorizeAttribute>());
                    Assert.Contains(action.Name, controller.Name switch
                    {
                        "PlatformAccountLanguageController" => new[] { "Get", "Put" },
                        "PlatformAccountContactsController" => new[] { "Get", "Create", "Update", "Delete" },
                        _ => new[] { "Get", "Update", "GetImage", "UploadImage", "DeleteImage" }
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
