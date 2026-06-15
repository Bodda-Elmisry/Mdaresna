using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Mdaresna.Middlewares
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = true)]
    public class PermissionAuthorizeAttribute : TypeFilterAttribute
    {
        public PermissionAuthorizeAttribute(string permissionKey) : base(typeof(PermissionAuthorizeFilter))
        {
            Arguments = new object[] { permissionKey };
        }
    }

    public class PermissionAuthorizeFilter : IAsyncActionFilter
    {
        private readonly string permissionKey;

        public PermissionAuthorizeFilter(string permissionKey)
        {
            this.permissionKey = permissionKey;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var user = context.HttpContext.User;

            if (user == null || !user.Identity.IsAuthenticated)
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            // Extract permissions claims from JWT token
            var permissionClaims = user.FindAll("permissions").Select(c => c.Value).ToList();

            // Try to find the matching permission
            // Format: "PermissionKey" or "PermissionKey:Classroom1,Classroom2..."
            var hasPermission = false;
            List<string>? allowedClassrooms = null;

            foreach (var claimVal in permissionClaims)
            {
                if (string.IsNullOrWhiteSpace(claimVal)) continue;

                var parts = claimVal.Split(':');
                var claimKey = parts[0];

                if (string.Equals(claimKey, permissionKey, StringComparison.OrdinalIgnoreCase))
                {
                    hasPermission = true;
                    if (parts.Length > 1)
                    {
                        allowedClassrooms = parts[1].Split(',').Select(c => c.Trim()).ToList();
                    }
                    break;
                }
            }

            if (!hasPermission)
            {
                context.Result = new ForbidResult();
                return;
            }

            // If the permission is classroom-restricted, verify ClassroomId context
            if (allowedClassrooms != null && allowedClassrooms.Any())
            {
                string? requestClassroomId = null;

                // 1. Try to find classroomId in route values
                if (context.RouteData.Values.TryGetValue("classroomId", out var routeVal) && routeVal != null)
                {
                    requestClassroomId = routeVal.ToString();
                }
                else if (context.RouteData.Values.TryGetValue("id", out var idVal) && idVal != null)
                {
                    requestClassroomId = idVal.ToString();
                }

                // 2. Try to find classroomId in query string
                if (string.IsNullOrEmpty(requestClassroomId))
                {
                    if (context.HttpContext.Request.Query.TryGetValue("classroomId", out var queryVal))
                    {
                        requestClassroomId = queryVal.ToString();
                    }
                }

                // 3. Try to find classroomId in the DTO parameter passed to the action method
                if (string.IsNullOrEmpty(requestClassroomId))
                {
                    foreach (var arg in context.ActionArguments.Values)
                    {
                        if (arg == null) continue;

                        // Use reflection to find ClassRoomId property
                        var prop = arg.GetType().GetProperty("ClassRoomId") 
                                   ?? arg.GetType().GetProperty("ClassroomId");
                        if (prop != null)
                        {
                            var val = prop.GetValue(arg);
                            if (val != null)
                            {
                                requestClassroomId = val.ToString();
                                break;
                            }
                        }
                    }
                }

                if (string.IsNullOrEmpty(requestClassroomId) || 
                    !allowedClassrooms.Any(c => string.Equals(c, requestClassroomId, StringComparison.OrdinalIgnoreCase)))
                {
                    context.Result = new ForbidResult();
                    return;
                }
            }

            await next();
        }
    }
}
