using System.ComponentModel.DataAnnotations;
using Mdaresna.Platform.Application.Errors;
using Mdaresna.Platform.Application.Access.Staff;
using Mdaresna.Platform.Domain.Common;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;

namespace Mdaresna.Platform.Api.Errors;

internal sealed class PlatformExceptionHandler(
    ILogger<PlatformExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (httpContext.Response.HasStarted ||
            !ApiResponseWriter.IsApplicationEndpoint(httpContext.Request.Path))
        {
            return false;
        }

        var (status, code, message, errors) = Map(exception);
        if (status == StatusCodes.Status500InternalServerError)
        {
            logger.LogError(exception, "Unhandled Platform API error");
        }

        await ApiResponseWriter.WriteFailureAsync(
            httpContext,
            status,
            code,
            message,
            errors,
            cancellationToken);
        return true;
    }

    private static (
        int Status,
        string Code,
        string Message,
        IReadOnlyDictionary<string, string[]>? Errors) Map(Exception exception) =>
        exception switch
        {
            PlatformResourceNotFoundException notFound =>
                (StatusCodes.Status404NotFound, notFound.Code, notFound.Message, null),
            PlatformConflictException conflict =>
                (StatusCodes.Status409Conflict, conflict.Code, conflict.Message, null),
            PlatformStaffAccessDeniedException =>
                (StatusCodes.Status403Forbidden, "access.denied", "Access is denied.", null),
            PlatformDomainException domain =>
                (StatusCodes.Status422UnprocessableEntity, domain.Code, domain.Message, null),
            DbUpdateConcurrencyException =>
                (StatusCodes.Status409Conflict, "request.version_conflict",
                    "The resource changed since it was last read.", null),
            ArgumentException =>
                (StatusCodes.Status400BadRequest, "request.invalid",
                    "The request is invalid.", null),
            ValidationException validation =>
                (StatusCodes.Status400BadRequest, "validation.failed", "Validation failed.",
                    GetValidationErrors(validation)),
            BadHttpRequestException badRequest when badRequest.StatusCode is >= 400 and <= 499 =>
                (badRequest.StatusCode,
                    ApiResponseWriter.DescribeStatusCode(badRequest.StatusCode).Code,
                    ApiResponseWriter.DescribeStatusCode(badRequest.StatusCode).Message,
                    null),
            _ =>
                (StatusCodes.Status500InternalServerError, "server.unexpected",
                    "An unexpected error occurred.", null)
        };

    private static IReadOnlyDictionary<string, string[]>? GetValidationErrors(
        ValidationException exception)
    {
        var members = exception.ValidationResult?.MemberNames
            .Where(member => !string.IsNullOrWhiteSpace(member))
            .Distinct(StringComparer.Ordinal)
            .ToArray();

        if (members is not { Length: > 0 })
        {
            return null;
        }

        var fieldMessage = exception.ValidationResult?.ErrorMessage ?? "The value is invalid.";
        return members.ToDictionary(
            member => member,
            _ => new[] { fieldMessage },
            StringComparer.Ordinal);
    }

}
